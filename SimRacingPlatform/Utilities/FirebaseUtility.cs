using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Firebase.Storage;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimRacingPlatform.Utilities
{
    public sealed class FirebaseUtility
    {
        public static FirebaseUtility Instance;

        private readonly string _apiKey;
        private static readonly HttpClient _http = new();

        private const string StorageBucket = "simracingplatform-1370c.firebasestorage.app";

        private readonly FirebaseStorage _storage;

        public FirebaseAuthClient Client { get; }
        public User? CurrentUser => Client.User;

        public string? LastPasswordResetEmail { get; set; }

        public FirebaseUtility(string apiKey, string authDomain)
        {
            Instance = this;
            _apiKey = apiKey;

            var config = new FirebaseAuthConfig
            {
                ApiKey = apiKey,
                AuthDomain = authDomain,
                Providers = [new EmailProvider()],
                UserRepository = new FileUserRepository("SimRacingPlatform")
            };

            Client = new FirebaseAuthClient(config);

            _storage = new FirebaseStorage(
                StorageBucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = async () =>
                    {
                        if (CurrentUser is null)
                        {
                            throw new InvalidOperationException("No signed-in user for storage operations.");
                        }

                        return await CurrentUser.GetIdTokenAsync();
                    },
                    ThrowOnCancel = true
                });
        }

        public async Task<UserCredential> RegisterAsync(string email, string password, string displayName)
        {
            return await Client.CreateUserWithEmailAndPasswordAsync(email, password, displayName);
        }

        public async Task<UserCredential> LoginAsync(string email, string password)
        {
            return await Client.SignInWithEmailAndPasswordAsync(email, password);
        }

        public void Logout() => Client.SignOut();

        public async Task SendVerificationEmailForCurrentUserAsync()
        {
            if (CurrentUser is null)
            {
                throw new InvalidOperationException("No signed-in user.");
            }

            var idToken = await CurrentUser.GetIdTokenAsync();
            await SendVerificationEmailAsync(idToken);
        }

        public async Task<bool> IsCurrentUserEmailVerifiedAsync()
        {
            if (CurrentUser is null)
            {
                return false;
            }

            var idToken = await CurrentUser.GetIdTokenAsync();

            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:lookup?key={_apiKey}";

            var payload = new { idToken };

            var response = await _http.PostAsJsonAsync(url, payload);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var json = await JsonDocument.ParseAsync(stream);

            return json.RootElement.GetProperty("users")[0].GetProperty("emailVerified").GetBoolean();
        }

        private async Task SendVerificationEmailAsync(string idToken)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_apiKey}";

            var payload = new
            {
                requestType = "VERIFY_EMAIL",
                idToken,
            };

            try
            {
                var response = await _http.PostAsJsonAsync(url, payload);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public async Task DeleteCurrentUserAsync()
        {
            if (CurrentUser is null)
            {
                throw new InvalidOperationException("No signed-in user.");
            }

            var idToken = await CurrentUser.GetIdTokenAsync();

            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:delete?key={_apiKey}";

            var payload = new
            {
                idToken
            };

            var response = await _http.PostAsJsonAsync(url, payload);
            response.EnsureSuccessStatusCode();

            Client.SignOut();
        }

        public async Task ChangePasswordAsync(string currentPassword, string newPassword)
        {
            if (CurrentUser is null)
            {
                throw new InvalidOperationException("No signed-in user.");
            }

            var email = CurrentUser.Info.Email;
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException("Current user does not have an email address.");
            }

            try
            {
                await Client.SignInWithEmailAndPasswordAsync(email, currentPassword);
            }
            catch (FirebaseAuthException ex)
            {
                throw new InvalidOperationException("The current password is incorrect.", ex);
            }

            if (Client.User is null)
            {
                throw new InvalidOperationException("User is no longer signed in.");
            }

            await Client.User.ChangePasswordAsync(newPassword);
        }

        public async Task SendPasswordResetEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email must not be empty.", nameof(email));
            }

            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_apiKey}";

            var payload = new
            {
                requestType = "PASSWORD_RESET",
                email
            };

            var response = await _http.PostAsJsonAsync(url, payload);
            response.EnsureSuccessStatusCode();

            LastPasswordResetEmail = email;
        }

        public async Task<string> UploadAndSetProfilePhotoAsync(StorageFile file)
        {
            var user = CurrentUser;
            if (user is null)
                throw new InvalidOperationException("No user is signed in.");

            string uid = user.Uid;

            await using var stream = await file.OpenStreamForReadAsync();

            string objectName = $"profile_photos/{uid}/avatar.jpg";

            var uploadTask = _storage
                .Child(objectName)
                .PutAsync(stream);

            string downloadUrl = await uploadTask;

            await UpdateUserProfilePhotoUrlAsync(downloadUrl);

            return downloadUrl;
        }

        private async Task UpdateUserProfilePhotoUrlAsync(string photoUrl)
        {
            var user = CurrentUser;
            if (user is null)
            {
                throw new InvalidOperationException("No user is signed in.");
            }

            var idToken = await user.GetIdTokenAsync(forceRefresh: true);

            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:update?key={_apiKey}";

            var payload = new
            {
                idToken,
                photoUrl,
                returnSecureToken = true
            };

            var response = await _http.PostAsJsonAsync(url, payload);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Firebase accounts:update failed ({(int)response.StatusCode} {response.ReasonPhrase}).\n{body}");
            }
        }

        public async Task<string?> GetProfilePhotoUrlAsync()
        {
            var user = CurrentUser;
            if (user is null)
                return null;

            // 1) Try the Auth profile first
            var fromAuth = user.Info.PhotoUrl;
            if (!string.IsNullOrWhiteSpace(fromAuth))
            {
                return fromAuth;
            }

            // 2) Fall back to Storage path if needed
            try
            {
                string objectName = $"profile_photos/{user.Uid}/avatar.jpg";

                string url = await _storage
                    .Child(objectName)
                    .GetDownloadUrlAsync();

                return url;
            }
            catch
            {
                // Object not found or rules blocked – just treat as "no avatar"
                return null;
            }
        }

        public async Task SaveCachedProfilePhotoUrlAsync(string url)
        {
            if (CurrentUser is null)
                return;

            var settings = ApplicationData.Current.LocalSettings;
            settings.Values[$"avatarUrl:{CurrentUser.Uid}"] = url;

            await Task.CompletedTask;
        }

        public string? TryGetCachedProfilePhotoUrl()
        {
            if (CurrentUser is null)
                return null;

            var settings = ApplicationData.Current.LocalSettings;
            return settings.Values.TryGetValue($"avatarUrl:{CurrentUser.Uid}", out var value)
                ? value as string
                : null;
        }
    }
}
