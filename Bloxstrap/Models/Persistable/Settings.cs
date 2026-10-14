using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Bloxstrap.Models.Persistable
{
    public class Settings
    {
        public BootstrapperStyle BootstrapperStyle { get; set; } = BootstrapperStyle.FluentDialog;
        public BootstrapperIcon BootstrapperIcon { get; set; } = BootstrapperIcon.IconBloxstrap;
        public string BootstrapperTitle { get; set; } = App.ProjectName;
        public string BootstrapperIconCustomLocation { get; set; } = "";
        public Theme Theme { get; set; } = Theme.Default;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool DeveloperMode { get; set; } = false;
        public bool CheckForUpdates { get; set; } = true;
        public bool ConfirmLaunches { get; set; } = false;
        public string Locale { get; set; } = "nil";
        public bool UseFastFlagManager { get; set; } = true;
        public bool WPFSoftwareRender { get; set; } = false;
        public bool EnableAnalytics { get; set; } = true;
        public bool BackgroundUpdatesEnabled { get; set; } = false;
        public bool DebugDisableVersionPackageCleanup { get; set; } = false;
        public string? SelectedCustomTheme { get; set; } = null;
        public WebEnvironment WebEnvironment { get; set; } = WebEnvironment.Production;

        public bool EnableActivityTracking { get; set; } = true;
        public bool UseDiscordRichPresence { get; set; } = true;
        public bool HideRPCButtons { get; set; } = true;
        public bool ShowAccountOnRichPresence { get; set; } = false;
        public bool ShowServerDetails { get; set; } = false;
        public ObservableCollection<CustomIntegration> CustomIntegrations { get; set; } = new();

        public bool UseDisableAppPatch { get; set; } = false;

        /// <summary>
        /// Приоритет процесса Roblox при запуске.
        /// Normal — поведение Windows по умолчанию.
        /// High требует прав администратора.
        /// </summary>
        public ProcessPriorityClass SelectedCpuPriority { get; set; } = ProcessPriorityClass.Normal;

        /// <summary>
        /// Предпочитаемый графический API (Vulkan / D3D11 / OpenGL).
        /// Default — не навязывать движку выбор.
        /// </summary>
        public RenderingMode SelectedRenderingMode { get; set; } = RenderingMode.Default;
    }
}
