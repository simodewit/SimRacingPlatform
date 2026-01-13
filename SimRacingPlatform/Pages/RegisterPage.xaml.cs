using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimRacingPlatform.Windows;

namespace SimRacingPlatform.Pages
{
    public sealed partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void RegisterClick(object sender, RoutedEventArgs args)
        {
            MainWindow.instance.NavigateTo(typeof(LandingPage));
        }

        private void LoginLinkClick(object sender, RoutedEventArgs args)
        {
            MainWindow.instance.NavigateTo(typeof(LoginPage));
        }
    }
}
