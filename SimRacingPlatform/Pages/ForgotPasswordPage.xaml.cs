using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimRacingPlatform.Windows;

namespace SimRacingPlatform.Pages
{
    public sealed partial class ForgotPasswordPage : Page
    {
        public ForgotPasswordPage()
        {
            InitializeComponent();
        }

        private void SendResetEmailClick(object sender, RoutedEventArgs args)
        {

        }

        private void SendAgainClick(object sender, RoutedEventArgs args)
        {

        }

        private void BackToLoginClick(object sender, RoutedEventArgs args)
        {
            MainWindow.Instance.NavigateTo(typeof(LoginPage));
        }
    }
}
