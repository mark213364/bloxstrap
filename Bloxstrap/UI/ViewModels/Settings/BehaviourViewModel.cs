using System.Diagnostics;
using Bloxstrap.Enums.FlagPresets;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class BehaviourViewModel : NotifyPropertyChangedViewModel
    {
        public bool ConfirmLaunches
        {
            get => App.Settings.Prop.ConfirmLaunches;
            set => App.Settings.Prop.ConfirmLaunches = value;
        }

        public bool BackgroundUpdates
        {
            get => App.Settings.Prop.BackgroundUpdatesEnabled;
            set => App.Settings.Prop.BackgroundUpdatesEnabled = value;
        }

        public bool IsRobloxInstallationMissing => !App.IsPlayerInstalled && !App.IsStudioInstalled;

        public bool ForceRobloxReinstallation
        {
            get => App.State.Prop.ForceReinstall || IsRobloxInstallationMissing;
            set => App.State.Prop.ForceReinstall = value;
        }

        public ProcessPriorityClass SelectedCpuPriority
        {
            get => App.Settings.Prop.SelectedCpuPriority;
            set
            {
                if (App.Settings.Prop.SelectedCpuPriority == value)
                    return;
                App.Settings.Prop.SelectedCpuPriority = value;
                OnPropertyChanged(nameof(SelectedCpuPriority));
            }
        }

        public RenderingMode SelectedRenderingMode
        {
            get => App.Settings.Prop.SelectedRenderingMode;
            set
            {
                if (App.Settings.Prop.SelectedRenderingMode == value)
                    return;

                App.Settings.Prop.SelectedRenderingMode = value;

                // применяем флаг через FastFlagManager
                switch (value)
                {
                    case RenderingMode.Default:
                        App.FastFlags.SetPresetEnum("Rendering.Mode", "Vulkan", null);
                        App.FastFlags.SetPresetEnum("Rendering.Mode", "D3D11", null);
                        App.FastFlags.SetPresetEnum("Rendering.Mode", "OpenGL", null);
                        App.FastFlags.SetValue("FFlagDebugGraphicsDisableDirect3D11", null);
                        break;
                    case RenderingMode.Vulkan:
                        App.FastFlags.SetValue("FFlagDebugGraphicsPreferVulkan", "True");
                        App.FastFlags.SetValue("FFlagDebugGraphicsDisableDirect3D11", "True");
                        App.FastFlags.SetValue("FFlagDebugGraphicsPreferD3D11", null);
                        App.FastFlags.SetValue("FFlagDebugGraphicsPreferOpenGL", null);
                        break;
                    case RenderingMode.D3D11:
                        App.FastFlags.SetValue("FFlagDebugGraphicsPreferD3D11", "True");
                        App.FastFlags.SetValue("FFlagDebugGraphicsPreferVulkan", null);
                        App.FastFlags.SetValue("FFlagDebugGraphicsPreferOpenGL", null);
                        App.FastFlags.SetValue("FFlagDebugGraphicsDisableDirect3D11", null);
                        break;
                    case RenderingMode.OpenGL:
                        App.FastFlags.SetValue("FFlagDebugGraphicsPreferOpenGL", "True");
                        App.FastFlags.SetValue("FFlagDebugGraphicsPreferVulkan", null);
                        App.FastFlags.SetValue("FFlagDebugGraphicsPreferD3D11", null);
                        App.FastFlags.SetValue("FFlagDebugGraphicsDisableDirect3D11", null);
                        break;
                }

                App.FastFlags.Save();
                OnPropertyChanged(nameof(SelectedRenderingMode));
            }
        }
    }
}
