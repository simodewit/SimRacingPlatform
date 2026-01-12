using Microsoft.UI.Xaml;
using SimRacingPlatform.Utilities;
using System.Threading.Tasks;

namespace SimRacingPlatform.Windows
{
    public sealed partial class Updater : Window
    {
        private int _windowHeight = 600;
        private int _windowWidth = 600;

        public Updater()
        {
            InitializeComponent();

            WindowUtility.HideTitleBar(this);
            WindowUtility.DisableResizing(this);
            WindowUtility.SetSize(this, _windowWidth, _windowHeight);

            ShouldCheckForUpdateHere();
        }

        private async void ShouldCheckForUpdateHere()
        {
            await Task.Delay(5000);
            Updated();
        }

        private void Updated()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                var nextWindow = new MainWindow();
                App.window = nextWindow;
                nextWindow.Activate();

                Close();
            });
        }
    }
}
