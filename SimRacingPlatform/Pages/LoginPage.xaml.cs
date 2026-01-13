using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimRacingPlatform.Windows;
using System.Diagnostics;

namespace SimRacingPlatform.Pages
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void LoginClick(object sender, RoutedEventArgs args)
        {
            if(string.IsNullOrEmpty(UsernameBox.Text))
            {
                return;
            }

            if(string.IsNullOrEmpty(PasswordBox.Password))
            {
                return;
            }

            try
            {
                var email = UsernameBox.Text.Trim();
                var password = PasswordBox.Password;

                var cred = await App.AuthService.LoginAsync(email, password);
                var idToken = await cred.User.GetIdTokenAsync();

                MainWindow.instance.NavigateTo(typeof(LandingPage));
            }
            catch
            {
                Debug.WriteLine("failed");
            }
        }

        private void RegisterLinkClick(object sender, RoutedEventArgs args)
        {
            MainWindow.instance.NavigateTo(typeof(RegisterPage));
        }
    }
}
