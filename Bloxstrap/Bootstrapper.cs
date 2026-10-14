        private void StartRoblox()
        {
            const string LOG_IDENT = "Bootstrapper::StartRoblox";

            SetStatus(Strings.Bootstrapper_Status_Starting);

            var startInfo = new ProcessStartInfo()
            {
                FileName = AppData.ExecutablePath,
                Arguments = _launchCommandLine,
                WorkingDirectory = AppData.Directory
            };

            if (_launchMode == LaunchMode.Player && ShouldRunAsAdmin())
            {
                startInfo.Verb = "runas";
                startInfo.UseShellExecute = true;
            }
            else if (_launchMode == LaunchMode.StudioAuth)
            {
                Process.Start(startInfo);
                return;
            }

            string? logFileName = null;

            string rbxDir = Path.Combine(Paths.LocalAppData, "Roblox");
            if (!Directory.Exists(rbxDir))
                Directory.CreateDirectory(rbxDir);

            string rbxLogDir = Path.Combine(rbxDir, "logs");
            if (!Directory.Exists(rbxLogDir))
                Directory.CreateDirectory(rbxLogDir);

            var logWatcher = new FileSystemWatcher()
            {
                Path = rbxLogDir,
                Filter = "*.log",
                EnableRaisingEvents = true
            };

            var logCreatedEvent = new AutoResetEvent(false);

            logWatcher.Created += (_, e) =>
            {
                logWatcher.EnableRaisingEvents = false;
                logFileName = e.FullPath;
                logCreatedEvent.Set();
            };

            // v2.2.0 - byfron will trip if we keep a process handle open for over a minute, so we're doing this now
            try
            {
                using var process = Process.Start(startInfo)!;
                _appPid = process.Id;

                // === Приоритет процесса ===
                try
                {
                    if (App.Settings.Prop.SelectedCpuPriority != ProcessPriorityClass.Normal)
                    {
                        process.PriorityClass = App.Settings.Prop.SelectedCpuPriority;
                        App.Logger.WriteLine(LOG_IDENT, $"CPU priority set to {App.Settings.Prop.SelectedCpuPriority} (PID {process.Id})");
                    }
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Failed to set CPU priority: {ex.Message}");
                }
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                return;
            }
            catch (Exception)
            {
                File.Delete(AppData.ExecutablePath);
                throw;
            }

            App.Logger.WriteLine(LOG_IDENT, $"Started Roblox (PID {_appPid}), waiting for log file");

            logCreatedEvent.WaitOne(TimeSpan.FromSeconds(15));

            if (String.IsNullOrEmpty(logFileName))
            {
                App.Logger.WriteLine(LOG_IDENT, "Unable to identify log file");
                Frontend.ShowPlayerErrorDialog();
                return;
            }
            else
            {
                App.Logger.WriteLine(LOG_IDENT, $"Got log file as {logFileName}");
            }

            _mutex?.ReleaseAsync();

            if (IsStudioLaunch)
                return;

            var autoclosePids = new List<int>();

            foreach (var integration in App.Settings.Prop.CustomIntegrations)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Launching custom integration '{integration.Name}' ({integration.Location} {integration.LaunchArgs} - autoclose is {integration.AutoClose})");

                int pid = 0;
                try
                {
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = integration.Location,
                        Arguments = integration.LaunchArgs.Replace("\r\n", " "),
                        WorkingDirectory = Path.GetDirectoryName(integration.Location),
                        UseShellExecute = true
                    })!;
                    pid = process.Id;
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Failed to launch integration '{integration.Name}'!");
                    App.Logger.WriteLine(LOG_IDENT, ex.Message);
                }

                if (integration.AutoClose && pid != 0)
                    autoclosePids.Add(pid);
            }

            if (App.Settings.Prop.EnableActivityTracking || App.LaunchSettings.TestModeFlag.Active || autoclosePids.Any())
            {
                using var ipl = new InterProcessLock("Watcher", TimeSpan.FromSeconds(5));

                var watcherData = new WatcherData
                {
                    ProcessId = _appPid,
                    LogFile = logFileName,
                    AutoclosePids = autoclosePids
                };

                string watcherDataArg = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(watcherData)));
                string args = $"-watcher \"{watcherDataArg}\"";

                if (App.LaunchSettings.TestModeFlag.Active)
                    args += " -testmode";

                if (ipl.IsAcquired)
                    Process.Start(Paths.Process, args);
            }

            Thread.Sleep(1000);
        }
