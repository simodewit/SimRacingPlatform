using Microsoft.UI.Xaml.Media.Imaging;
using SimRacingPlatform.Utilities;
using SimRacingPlatform.ViewModels;
using SimRacingPlatform.Windows;
using System;
using System.Threading.Tasks;

namespace SimRacingPlatform.Services
{
    public static class UserSessionService
    {
        public static void ClearSession()
        {
            var session = UserSessionViewModel.Instance;
            var dispatcher = MainWindow.Instance.DispatcherQueue;

            dispatcher.TryEnqueue(() => session.Clear());
        }

        public static async Task RefreshFromCurrentUserAsync()
        {
            var session = UserSessionViewModel.Instance;
            var dispatcher = MainWindow.Instance.DispatcherQueue;

            var firebaseUser = FirebaseUtility.Instance.CurrentUser;
            if (firebaseUser is null)
            {
                dispatcher.TryEnqueue(() => session.Clear());
                return;
            }

            // Gather identity data off the UI thread
            string email = firebaseUser.Info.Email;
            string displayName = firebaseUser.Info.DisplayName;
            string uid = firebaseUser.Uid;

            // Avatar URLs
            string cachedUrl = FirebaseUtility.Instance.TryGetCachedProfilePhotoUrl();
            string remotePhotoUrl = null;

            try
            {
                remotePhotoUrl = await FirebaseUtility.Instance.GetProfilePhotoUrlAsync();
                if (!string.IsNullOrWhiteSpace(remotePhotoUrl))
                {
                    await FirebaseUtility.Instance.SaveCachedProfilePhotoUrlAsync(remotePhotoUrl);
                }
            }
            catch
            {
                // Ignore refresh failures; we'll fall back to cache/default
            }

            // Now push everything into the ViewModel on the UI thread
            dispatcher.TryEnqueue(() =>
            {
                session.Email = email ?? string.Empty;
                session.DisplayName = displayName ?? string.Empty;
                session.Uid = uid ?? string.Empty;

                // Decide which URL to use for the avatar
                string chosenUrl = !string.IsNullOrWhiteSpace(remotePhotoUrl)
                    ? remotePhotoUrl
                    : (!string.IsNullOrWhiteSpace(cachedUrl) ? cachedUrl : null);

                if (!string.IsNullOrWhiteSpace(chosenUrl))
                {
                    try
                    {
                        session.ProfileImage = new BitmapImage(new Uri(chosenUrl));
                    }
                    catch
                    {
                        // If anything goes wrong, we simply keep the existing image
                    }
                }
            });
        }
    }
}
