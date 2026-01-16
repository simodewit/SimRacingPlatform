using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimRacingPlatform.Utilities;
using SimRacingPlatform.Windows;
using System;

namespace SimRacingPlatform.Pages
{
    public sealed partial class ForgotPasswordPage : Page
    {
        public ForgotPasswordPage()
        {
            InitializeComponent();
        }

        private async void SendResetEmailClick(object sender, RoutedEventArgs args)
        {
            StatusText.Visibility = Visibility.Collapsed;
            SentInfoBar.IsOpen = false;

            var email = EmailBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                StatusText.Text = "Please enter your email address.";
                StatusText.Visibility = Visibility.Visible;
                return;
            }

            SendButton.IsEnabled = false;

            try
            {
                await FirebaseUtility.Instance.SendPasswordResetEmailAsync(email);

                // ALWAYS marshal back to UI thread before navigation
                DispatcherQueue.TryEnqueue(() =>
                {
                    MainWindow.Instance.NavigateTo(typeof(PasswordResetSentPage));
                });
            }
            catch
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    StatusText.Text = "We couldn't send the reset email. Please try again.";
                    StatusText.Visibility = Visibility.Visible;
                });
            }
            finally
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    SendButton.IsEnabled = true;
                });
            }
        }

        private void BackToLoginClick(object sender, RoutedEventArgs args)
        {
            MainWindow.Instance.NavigateTo(typeof(LoginPage));
        }
    }
}
