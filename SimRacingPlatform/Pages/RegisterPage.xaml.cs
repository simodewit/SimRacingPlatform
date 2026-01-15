using Firebase.Auth;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimRacingPlatform.Utilities;
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

            try
            {
                var email = EmailBox.Text.Trim();
                var password = PasswordBox.Password;
                var displayName = UsernameBox.Text.Trim();

                var cred = await App.AuthService.RegisterAsync(email, password, displayName);

                var token = await cred.User.GetIdTokenAsync();
                await FirebaseUtility.Instance.SendVerificationEmailForCurrentUserAsync();

                MainWindow.Instance.NavigateTo(typeof(VerifyEmailPage));
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
