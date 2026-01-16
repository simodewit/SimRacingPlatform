using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimRacingPlatform.Utilities;
using SimRacingPlatform.Windows;
using System;

namespace SimRacingPlatform.Pages
{
    public sealed partial class PasswordResetSentPage : Page
    {
        public PasswordResetSentPage()
        {
            InitializeComponent();
        }

        private async void ResendEmailClick(object sender, RoutedEventArgs e)
        {
            // The email that was last used for reset:
            var email = FirebaseUtility.Instance.LastPasswordResetEmail;

            if (string.IsNullOrWhiteSpace(email))
            {
                // We don't know which email to resend to (e.g. app restarted)
                // -> send the user back to the ForgotPasswordPage
                ContentDialog dialog = new()
                {
                    Title = "Email address unknown",
                    Content = "Please enter your email again so we can resend the reset link.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
                MainWindow.Instance.NavigateTo(typeof(ForgotPasswordPage));
                return;
            }

            ResendButton.IsEnabled = false;

            try
            {
                await FirebaseUtility.Instance.SendPasswordResetEmailAsync(email);

                // Optionally show a confirmation dialog
                ContentDialog dialog = new()
                {
                    Title = "Email resent",
                    Content = "If an account exists for that email, you’ll receive another reset link shortly.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                ContentDialog dialog = new()
                {
                    Title = "Could not resend email",
                    Content = "Something went wrong while resending the reset link. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
            }
            finally
            {
                ResendButton.IsEnabled = true;
            }
        }

        private void BackToLoginClick(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.NavigateTo(typeof(LoginPage));
        }
    }
}
