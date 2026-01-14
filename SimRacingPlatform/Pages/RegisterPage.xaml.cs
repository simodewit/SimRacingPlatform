using Firebase.Auth;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimRacingPlatform.Windows;
using System.Diagnostics;

namespace SimRacingPlatform.Pages
{
    public sealed partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }
        
        private async void RegisterClick(object sender, RoutedEventArgs args)
        {
            Debug.WriteLine("triggers");

            if (string.IsNullOrEmpty(UsernameBox.Text))
            {
                return;
            }

            if(string.IsNullOrEmpty(EmailBox.Text))
            {
                return;
            }

            if(string.IsNullOrEmpty(PasswordBox.Password))
            {
                return;
            }

            if(PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                return;
            }

            Debug.WriteLine("passes");

            try
            {
                Debug.WriteLine("tries");
                var email = EmailBox.Text.Trim();
                var password = PasswordBox.Password;
                var displayName = UsernameBox.Text.Trim();

                var cred = await App.AuthService.RegisterAsync(email, password, displayName);

                MainWindow.Instance.NavigateTo(typeof(LandingPage));
            }
            catch (FirebaseAuthException ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void LoginLinkClick(object sender, RoutedEventArgs args)
        {
            MainWindow.Instance.NavigateTo(typeof(LoginPage));
        }
    }
}
