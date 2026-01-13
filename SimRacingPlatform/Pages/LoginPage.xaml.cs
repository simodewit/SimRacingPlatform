using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimRacingPlatform.Windows;

namespace SimRacingPlatform.Pages
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginClick(object sender, RoutedEventArgs args)
        {
            MainWindow.instance.NavigateTo(typeof(LandingPage));
        }

        private void RegisterLinkClick(object sender, RoutedEventArgs args)
        {
            MainWindow.instance.NavigateTo(typeof(RegisterPage));
        }
    }
}
