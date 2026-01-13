using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using SimRacingPlatform.Utilities;
using System.Threading.Tasks;

namespace SimRacingPlatform.Windows
{
    public sealed partial class Updater : Window
    {
        private const int WindowHeight = 600;
        private const int WindowWidth = 600;

        private bool _started;

        public Updater()
        {
            InitializeComponent();

            WindowUtility.HideTitleBar(this);
            WindowUtility.DisableResizing(this);
            WindowUtility.SetSize(this, WindowWidth, WindowHeight);

            Activated += Updater_Activated;
        }

        private async void Updater_Activated(object sender, WindowActivatedEventArgs args)
        {
            // Prevent running twice (Activated can fire more than once)
            if (_started)
            {
                return;
            }
            _started = true;

            await RunUpdateThenContinueAsync();
        }

        private async Task RunUpdateThenContinueAsync()
        {
            UpdateResult result = await UpdateUtility.RunUpdateFlowAsync(
                onProgress: progress =>
                {
                    UpdateProgressBar.Value = progress;
                    ProgressText.Text = "Downloading update";
                    PercentText.Text = $"{progress}%";
                });

            if (result == UpdateResult.Restarting)
            {
                return;
            }

            OpenMainWindowAndClose();
        }

        private void OpenMainWindowAndClose()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                MainWindow nextWindow = new MainWindow();
                App.window = nextWindow;
                nextWindow.Activate();

                Close();
            });
        }
    }
}
