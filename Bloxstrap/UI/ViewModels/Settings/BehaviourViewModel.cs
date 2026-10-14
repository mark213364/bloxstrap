using System.Diagnostics;

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

        // === НОВОЕ: приоритет процесса Roblox ===
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
    }
}
