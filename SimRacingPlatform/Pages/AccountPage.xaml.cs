using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using SimRacingPlatform.Utilities;
using SimRacingPlatform.Windows;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

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

            // 1) Instant load: use cached avatar URL if we have one
            var cachedUrl = FirebaseUtility.Instance.TryGetCachedProfilePhotoUrl();
            if (!string.IsNullOrWhiteSpace(cachedUrl))
            {
                try
                {
                    ViewModel.ProfileImage = new BitmapImage(new Uri(cachedUrl));
                }
                catch
                {
                    // If something goes wrong, we just keep the default image.
                }
            }

            // 2) Then refresh in background (multi-device, first run, etc.)
            _ = LoadProfileImageAsync();
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
                };
            });

            if (result != ContentDialogResult.Primary)
            {
                return;
            }

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
            MainWindow.Instance.NavigateTo(typeof(LoginPage));
        }

        private async void ChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // File picker must be created on the UI thread
                var picker = new FileOpenPicker
                {
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                    ViewMode = PickerViewMode.Thumbnail
                };

                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".webp");

                // WinUI 3: initialize picker with window handle
                var hwnd = WindowNative.GetWindowHandle(MainWindow.Instance);
                InitializeWithWindow.Initialize(picker, hwnd);

                StorageFile file = await picker.PickSingleFileAsync();
                if (file is null)
                    return;

                // Upload to Firebase Storage (can run off the UI thread)
                string downloadUrl = await FirebaseUtility.Instance.UploadAndSetProfilePhotoAsync(file);

                await FirebaseUtility.Instance.SaveCachedProfilePhotoUrlAsync(downloadUrl);

                // Build a cache-busted URL *without* breaking existing query parameters
                string uriString = downloadUrl;
                string separator = uriString.Contains("?") ? "&" : "?";
                uriString += $"{separator}t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

                // Update UI on the UI thread
                DispatcherQueue.TryEnqueue(async () =>
                {
                    ViewModel.ProfileImage = new BitmapImage(new Uri(uriString));
                    await WindowUtility.ShowMessageAsync(
                        "Profile updated",
                        "Your profile picture has been changed.");
                });
            }
            catch (Exception ex)
            {
                // Also show error dialog on the UI thread
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

        private async Task LoadProfileImageAsync()
        {
            try
            {
                // Ask Firebase for the current "truth" (Auth or Storage)
                var photoUrl = await FirebaseUtility.Instance.GetProfilePhotoUrlAsync();
                if (string.IsNullOrWhiteSpace(photoUrl))
                    return;

                // Keep cache in sync
                await FirebaseUtility.Instance.SaveCachedProfilePhotoUrlAsync(photoUrl);

                var uri = new Uri(photoUrl);

                // Update UI on the UI thread
                DispatcherQueue.TryEnqueue(() =>
                {
                    ViewModel.ProfileImage = new BitmapImage(uri);
                });
            }
            catch
            {
                // Silently keep whatever we already display
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

        // CHANGED: make ProfileImage settable so UI can update after upload
        private BitmapImage _profileImage = new BitmapImage(new Uri("ms-appx:///Assets/SquareLogo.png"));
        public BitmapImage ProfileImage
        {
            get => _profileImage;
            set { _profileImage = value; Raise(); }
        }

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
