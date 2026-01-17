using Firebase.Auth;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimRacingPlatform.Services;
using SimRacingPlatform.Utilities;
using SimRacingPlatform.Windows;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SimRacingPlatform.Pages
{
    public sealed partial class LoginPage : Page
    {
        private bool _isBusy;
        private readonly DispatcherQueue _dispatcherQueue;

        public LoginPage()
        {
            InitializeComponent();

            // Capture the UI thread's DispatcherQueue once.
            _dispatcherQueue = DispatcherQueue;
        }

        private async void LoginClick(object sender, RoutedEventArgs args)
        {
            if (_isBusy)
                return;

            var email = EmailBox.Text?.Trim() ?? string.Empty;
            var password = PasswordBox.Password ?? string.Empty;

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
                ShowStatus("Please enter your password.");
                RunOnUIThread(() => PasswordBox.Focus(FocusState.Programmatic));
                return;
            }

            try
            {
                SetBusy(true);
                HideStatus();

                var cred = await App.AuthService.LoginAsync(email, password);
                _ = await cred.User.GetIdTokenAsync();

                bool verified = await FirebaseUtility.Instance.IsCurrentUserEmailVerifiedAsync();

                if (!verified)
                {
                    RunOnUIThread(() =>
                        MainWindow.Instance.NavigateTo(typeof(VerifyEmailPage)));
                }
                else
                {
                    await UserSessionService.RefreshFromCurrentUserAsync();

                    RunOnUIThread(() =>
                        MainWindow.Instance.NavigateTo(typeof(LandingPage)));
                }
            }
            catch (FirebaseAuthException ex)
            {
                Debug.WriteLine(ex);
                ShowStatus(MapFirebaseAuthError(ex));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                ShowStatus("Login failed due to an unexpected error. Please try again.");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void RegisterLinkClick(object sender, RoutedEventArgs args)
        {
            RunOnUIThread(() =>
                MainWindow.Instance.NavigateTo(typeof(RegisterPage)));
        }

        private void ForgotPasswordClick(object sender, RoutedEventArgs args)
        {
            RunOnUIThread(() =>
                MainWindow.Instance.NavigateTo(typeof(ForgotPasswordPage)));
        }

        // Clears errors as the user types/changes password
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

        // ==== UI-thread helpers =================================================

        private void RunOnUIThread(Action action)
        {
            if (action == null)
                return;

            if (_dispatcherQueue == null)
            {
                // Fallback: should not normally happen, but avoids null ref.
                action();
                return;
            }

            // Always enqueue to the UI dispatcher to avoid COM threading issues.
            _dispatcherQueue.TryEnqueue(() => action());
        }

        private void SetBusy(bool busy)
        {
            _isBusy = busy;

            RunOnUIThread(() =>
            {
                if (LoginButton != null)
                {
                    LoginButton.IsEnabled = !busy;
                    LoginButton.Content = busy ? "Logging in..." : "Login";
                }

                // Optional: disable fields while busy
                // EmailBox.IsEnabled = !busy;
                // PasswordBox.IsEnabled = !busy;
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

            if (msg.Contains("invalid-email"))
                return "That email address is invalid.";

            if (msg.Contains("user-not-found") || msg.Contains("email not found"))
                return "No account was found for that email address.";

            if (msg.Contains("wrong-password") || msg.Contains("invalid password"))
                return "Incorrect password. Please try again.";

            if (msg.Contains("user-disabled"))
                return "This account has been disabled. Contact support if you think this is a mistake.";

            if (msg.Contains("too-many-requests") || msg.Contains("try again later"))
                return "Too many attempts. Please wait a moment and try again.";

            if (msg.Contains("network") || msg.Contains("timeout") || msg.Contains("unavailable"))
                return "Network problem. Check your connection and try again.";

            return "Login failed. Please check your email and password and try again.";
        }
    }
}
