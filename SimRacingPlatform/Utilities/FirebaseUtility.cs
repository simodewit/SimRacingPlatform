using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using System;
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
                continueUrl = "https://simracingplatform-1370c.web.app/verified"
            };

            var response = await _http.PostAsJsonAsync(url, payload);
            response.EnsureSuccessStatusCode();
        }
    }
}

