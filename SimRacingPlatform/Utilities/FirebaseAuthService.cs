using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using System.Threading.Tasks;

namespace SimRacingPlatform.Utilities
{
    public sealed class FirebaseAuthService
    {
        public FirebaseAuthClient Client { get; }

        public FirebaseAuthService(string apiKey, string authDomain)
        {
            var config = new FirebaseAuthConfig
            {
                ApiKey = apiKey,
                AuthDomain = authDomain,
                Providers = new FirebaseAuthProvider[]
                {
                new EmailProvider()
                },

                // Persist the signed-in user between app launches
                // (stores under %AppData%\YourAppName by default for this repo type)
                UserRepository = new FileUserRepository("YourAppName")
            };

            Client = new FirebaseAuthClient(config);
        }

        public async Task<UserCredential> RegisterAsync(string email, string password, string displayName)
        {
            // CreateUserWithEmailAndPasswordAsync(email, pwd, displayName) is provided by the library. :contentReference[oaicite:3]{index=3}
            return await Client.CreateUserWithEmailAndPasswordAsync(email, password, displayName);
        }

        public async Task<UserCredential> LoginAsync(string email, string password)
        {
            // SignInWithEmailAndPasswordAsync(email, pwd) is provided by the library. :contentReference[oaicite:4]{index=4}
            return await Client.SignInWithEmailAndPasswordAsync(email, password);
        }

        public void Logout() => Client.SignOut();
    }

}

