using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimRacingPlatform.Utilities;
using SimRacingPlatform.Windows;

namespace SimRacingPlatform.Pages
{
    public sealed partial class VerifyEmailPage : Page
    {
        public VerifyEmailPage()
        {
            InitializeComponent();
        }

        private void BackClick(object sender, RoutedEventArgs args)
        {
            if (!MainWindow.Instance.CanGoBack)
            {
                MainWindow.Instance.NavigateTo(typeof(LoginPage));
            }
            else
            {
                MainWindow.Instance.NavigateBack();
            }
        }

        private async void ResendClick(object sender, RoutedEventArgs args)
        {
            await FirebaseUtility.Instance.SendVerificationEmailForCurrentUserAsync();
        }
    }
}
