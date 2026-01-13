using Microsoft.UI.Xaml;
using SimRacingPlatform.Utilities;
using SimRacingPlatform.Windows;

namespace SimRacingPlatform
{
    public partial class App : Application
    {
        public static Window? window;
        public static FirebaseAuthService AuthService { get; private set; } = null!;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            StartFirebaseService();

            window = new Updater();
            window.Activate();
        }

        private static void StartFirebaseService()
        {
            AuthService = new FirebaseAuthService(
                apiKey: "AIzaSyAQoyg7y5OmivRsyOyy2eT7qUfFT16VI_M",
                authDomain: "simracingplatform-1370c.firebaseapp.com");
        }
    }
}
