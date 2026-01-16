using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using SimRacingPlatform.Utilities;
using SimRacingPlatform.Windows;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimRacingPlatform.Pages
{
    public sealed partial class AccountPage : Page
    {
        public AccountViewModel ViewModel { get; } = new();

        public AccountPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);

            var user = FirebaseUtility.Instance.CurrentUser;
            if (user is null)
            {
                MainWindow.Instance.NavigateTo(typeof(LoginPage));
                return;
            }

            ViewModel.Email = user.Info.Email;
            ViewModel.DisplayName = user.Info.DisplayName;
            ViewModel.Uid = user.Uid;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.NavigateBack();
        }

        private async void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            // Create controls *here* so we can read them later
            var oldPasswordBox = new PasswordBox
            {
                Header = "Current password",
                Margin = new Thickness(0, 0, 0, 8)
            };

            var newPasswordBox = new PasswordBox
            {
                Header = "New password",
                Margin = new Thickness(0, 0, 0, 8)
            };

            var confirmPasswordBox = new PasswordBox
            {
                Header = "Confirm new password"
            };

            // Show the dialog via the utility
            var result = await WindowUtility.ShowContentDialogAsync(() =>
            {
                var panel = new StackPanel
                {
                    Spacing = 8
                };
                panel.Children.Add(oldPasswordBox);
                panel.Children.Add(newPasswordBox);
                panel.Children.Add(confirmPasswordBox);

                return new ContentDialog
                {
                    Title = "Change password",
                    Content = panel,
                    PrimaryButtonText = "Change password",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary
                    // XamlRoot will be set in the utility if needed
                };
            });

            if (result != ContentDialogResult.Primary)
                return;

            // Read values after dialog closes
            var oldPassword = oldPasswordBox.Password;
            var newPassword = newPasswordBox.Password;
            var confirmPassword = confirmPasswordBox.Password;

            // Basic validation
            if (string.IsNullOrWhiteSpace(oldPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                await WindowUtility.ShowMessageAsync(
                    "Change password",
                    "Please fill in all fields.");

                return;
            }

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                await WindowUtility.ShowMessageAsync(
                    "Change password",
                    "The new passwords do not match.");

                return;
            }

            try
            {
                await FirebaseUtility.Instance.ChangePasswordAsync(oldPassword, newPassword);

                await WindowUtility.ShowMessageAsync(
                    "Password changed",
                    "Your password has been changed successfully.");
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("The current password is incorrect", StringComparison.Ordinal))
            {
                await WindowUtility.ShowMessageAsync(
                    "Change password",
                    "The current password you entered is incorrect.");
            }
            catch (Exception ex)
            {
                await WindowUtility.ShowMessageAsync(
                    "Error changing password",
                    $"Something went wrong while changing your password:\n{ex.Message}");
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            FirebaseUtility.Instance.Logout();
            MainWindow.Instance.NavigateTo(typeof(LoginPage));
        }

        private void ChangePhoto_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Delete account?",
                Content = "This will permanently delete your account and all associated data. " +
                          "This action cannot be undone. Are you sure you want to continue?",
                PrimaryButtonText = "Delete account",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    await FirebaseUtility.Instance.DeleteCurrentUserAsync();
                    MainWindow.Instance.NavigateTo(typeof(LoginPage));
                }
                catch (Exception ex)
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Error deleting account",
                        Content = $"Something went wrong while deleting your account:\n{ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };

                    await errorDialog.ShowAsync();
                }
            }
            else
            {

            }
        }
    }

    public class AccountViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void Raise([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
            
        public BitmapImage ProfileImage { get; } = new BitmapImage(new Uri("ms-appx:///Assets/SquareLogo.png"));

        private string _displayName = "";
        public string DisplayName
        {
            get => _displayName;
            set { _displayName = value; Raise(); }
        }

        private string _email = "";
        public string Email
        {
            get => _email;
            set { _email = value; Raise(); }
        }

        private string _uid = "";
        public string Uid
        {
            get => _uid;
            set { _uid = value; Raise(); }
        }
    }
}
