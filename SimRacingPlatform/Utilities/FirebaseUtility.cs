using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimRacingPlatform.Utilities
{
    public sealed class FirebaseUtility
    {
        public static FirebaseUtility Instance;

        private readonly string _apiKey;
        private static readonly HttpClient _http = new();

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
                // Re-authenticate the user with the old password
                await Client.SignInWithEmailAndPasswordAsync(email, currentPassword);
            }
            catch (FirebaseAuthException ex)
            {
                // Wrap it so the UI can distinguish "wrong password" from other errors if needed
                throw new InvalidOperationException("The current password is incorrect.", ex);
            }

            // After successful re-authentication, change the password
            if (Client.User is null)
            {
                throw new InvalidOperationException("User is no longer signed in.");
            }

            await Client.User.ChangePasswordAsync(newPassword);
        }

        public async Task SendPasswordResetEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email must not be empty.", nameof(email));

            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_apiKey}";

            var payload = new
            {
                requestType = "PASSWORD_RESET",
                email
                // The link will go to the URL configured in Firebase Console
                // for the Password reset email template.
            };

            var response = await _http.PostAsJsonAsync(url, payload);
            response.EnsureSuccessStatusCode();

            // Remember it so the "Resend" button can use it later
            LastPasswordResetEmail = email;
        }
    }
}

