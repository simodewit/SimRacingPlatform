using Firebase.Auth;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimRacingPlatform.Utilities;
using SimRacingPlatform.Windows;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SimRacingPlatform.Pages
{
    public sealed partial class RegisterPage : Page
    {
        private bool _isBusy;
        private readonly DispatcherQueue _dispatcherQueue;

        public RegisterPage()
        {
            InitializeComponent();
            _dispatcherQueue = DispatcherQueue;
        }

        private async void RegisterClick(object sender, RoutedEventArgs args)
        {
            if (_isBusy)
                return;

            var username = UsernameBox.Text?.Trim() ?? string.Empty;
            var email = EmailBox.Text?.Trim() ?? string.Empty;
            var password = PasswordBox.Password ?? string.Empty;
            var confirmPassword = ConfirmPasswordBox.Password ?? string.Empty;

            // Validation with user feedback
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowStatus("Please choose a username.");
                RunOnUIThread(() => UsernameBox.Focus(FocusState.Programmatic));
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowStatus("Please enter your email address.");
                RunOnUIThread(() => EmailBox.Focus(FocusState.Programmatic));
                return;
            }

            if (!LooksLikeEmail(email))
            {
                ShowStatus("That email address doesn’t look valid. Example: you@example.com");
                RunOnUIThread(() => EmailBox.Focus(FocusState.Programmatic));
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowStatus("Please create a password.");
                RunOnUIThread(() => PasswordBox.Focus(FocusState.Programmatic));
                return;
            }

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                ShowStatus("Please confirm your password.");
                RunOnUIThread(() => ConfirmPasswordBox.Focus(FocusState.Programmatic));
                return;
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                ShowStatus("Passwords do not match. Please make sure both passwords are the same.");
                RunOnUIThread(() => ConfirmPasswordBox.Focus(FocusState.Programmatic));
                return;
            }

            try
            {
                SetBusy(true);
                HideStatus();

                var cred = await App.AuthService.RegisterAsync(email, password, username);

                _ = await cred.User.GetIdTokenAsync();
                await FirebaseUtility.Instance.SendVerificationEmailForCurrentUserAsync();

                RunOnUIThread(() =>
                    MainWindow.Instance.NavigateTo(typeof(VerifyEmailPage)));
            }
            catch (FirebaseAuthException ex)
            {
                Debug.WriteLine(ex);
                ShowStatus(MapFirebaseAuthError(ex));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                ShowStatus("Registration failed due to an unexpected error. Please try again.");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void LoginLinkClick(object sender, RoutedEventArgs args)
        {
            RunOnUIThread(() =>
                MainWindow.Instance.NavigateTo(typeof(LoginPage)));
        }

        // Clear status when user edits fields

        private void UsernameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (StatusText.Visibility == Visibility.Visible)
                HideStatus();
        }

        private void EmailBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (StatusText.Visibility == Visibility.Visible)
                HideStatus();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (StatusText.Visibility == Visibility.Visible)
                HideStatus();
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (StatusText.Visibility == Visibility.Visible)
                HideStatus();
        }

        // ==== UI-thread helpers ================================================

        private void RunOnUIThread(Action action)
        {
            if (action == null)
                return;

            if (_dispatcherQueue == null)
            {
                action();
                return;
            }

            _dispatcherQueue.TryEnqueue(() => action());
        }

        private void SetBusy(bool busy)
        {
            _isBusy = busy;

            RunOnUIThread(() =>
            {
                if (RegisterButton != null)
                {
                    RegisterButton.IsEnabled = !busy;
                    RegisterButton.Content = busy ? "Registering..." : "Register";
                }

                // Optionally also disable fields while busy:
                // UsernameBox.IsEnabled = !busy;
                // EmailBox.IsEnabled = !busy;
                // PasswordBox.IsEnabled = !busy;
                // ConfirmPasswordBox.IsEnabled = !busy;
            });
        }

        private void ShowStatus(string message)
        {
            RunOnUIThread(() =>
            {
                StatusText.Text = message ?? string.Empty;
                StatusText.Visibility = Visibility.Visible;
            });
        }

        private void HideStatus()
        {
            RunOnUIThread(() =>
            {
                StatusText.Text = string.Empty;
                StatusText.Visibility = Visibility.Collapsed;
            });
        }

        // ==== Utility methods ===================================================

        private static bool LooksLikeEmail(string email)
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }

        private static string MapFirebaseAuthError(FirebaseAuthException ex)
        {
            var msg = (ex.Message ?? string.Empty).ToLowerInvariant();

            if (msg.Contains("email-already-in-use") || msg.Contains("email already in use"))
                return "An account with this email already exists.";

            if (msg.Contains("invalid-email"))
                return "That email address is invalid.";

            if (msg.Contains("weak-password"))
                return "Your password is too weak. Try a stronger password.";

            if (msg.Contains("operation-not-allowed"))
                return "Email/password sign-up is not enabled for this project.";

            if (msg.Contains("network") || msg.Contains("timeout") || msg.Contains("unavailable"))
                return "Network problem. Check your connection and try again.";

            return "Registration failed. Please check your details and try again.";
        }
    }
}
