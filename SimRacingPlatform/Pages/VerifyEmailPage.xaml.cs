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
            MainWindow.Instance.NavigateBack();
        }

        private async void ResendClick(object sender, RoutedEventArgs args)
        {
            await FirebaseUtility.Instance.SendVerificationEmailForCurrentUserAsync();
        }

        private void ConfirmedEmail()
        {
            MainWindow.Instance.NavigateTo(typeof(EmailConfirmedPage));
        }
    }
}
