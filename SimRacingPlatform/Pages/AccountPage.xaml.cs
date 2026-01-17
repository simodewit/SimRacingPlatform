using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using SimRacingPlatform.Services;
using SimRacingPlatform.Utilities;
using SimRacingPlatform.ViewModels;
using SimRacingPlatform.Windows;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace SimRacingPlatform.Pages
{
    public sealed partial class AccountPage : Page
    {
        public UserSessionViewModel Session => UserSessionViewModel.Instance;

        public AccountPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);

            var user = FirebaseUtility.Instance.CurrentUser;
            if (user is null)
            {
                MainWindow.Instance.NavigateTo(typeof(LoginPage));
                return;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.NavigateBack();
        }

        private async void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
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

            var result = await WindowUtility.ShowContentDialogAsync(() =>
            {
                var panel = new StackPanel { Spacing = 8 };
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
                };
            });

            if (result != ContentDialogResult.Primary)
                return;

            var oldPassword = oldPasswordBox.Password;
            var newPassword = newPasswordBox.Password;
            var confirmPassword = confirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(oldPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                await WindowUtility.ShowMessageAsync("Change password", "Please fill in all fields.");
                return;
            }

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                await WindowUtility.ShowMessageAsync("Change password", "The new passwords do not match.");
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
            UserSessionService.ClearSession();
            MainWindow.Instance.NavigateTo(typeof(LoginPage));
        }

        private async void ChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new FileOpenPicker
                {
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                    ViewMode = PickerViewMode.Thumbnail
                };

                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".webp");

                var hwnd = WindowNative.GetWindowHandle(MainWindow.Instance);
                InitializeWithWindow.Initialize(picker, hwnd);

                StorageFile file = await picker.PickSingleFileAsync();
                if (file is null)
                    return;

                // Upload + get download URL
                string downloadUrl = await FirebaseUtility.Instance.UploadAndSetProfilePhotoAsync(file);

                // Save local cache
                await FirebaseUtility.Instance.SaveCachedProfilePhotoUrlAsync(downloadUrl);

                // cache-bust for the BitmapImage refresh
                string uriString = downloadUrl;
                string separator = uriString.Contains("?") ? "&" : "?";
                uriString += $"{separator}t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

                DispatcherQueue.TryEnqueue(async () =>
                {
                    // Update shared session image; sidebar + this page will reflect it
                    Session.ProfileImage = new BitmapImage(new Uri(uriString));

                    await WindowUtility.ShowMessageAsync("Profile updated", "Your profile picture has been changed.");
                });
            }
            catch (Exception ex)
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    await WindowUtility.ShowMessageAsync(
                        "Change photo",
                        $"Could not update profile photo:\n{ex.Message}");
                });
            }
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
                    UserSessionService.ClearSession();
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
        }
    }
}
