using Microsoft.UI.Xaml;
using SimRacingPlatform.Windows;
using Velopack;

namespace SimRacingPlatform
{
    public partial class App : Application
    {
        public static Window? window;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            window = new Updater();
            window.Activate();
        }
    }
}
