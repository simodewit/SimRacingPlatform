using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using System.Threading.Tasks;

namespace SimRacingPlatform.Utilities
{
    public sealed class FirebaseUtility
    {
        public static FirebaseUtility Instance;
        public FirebaseAuthClient Client { get; }
        public User? CurrentUser => Client.User;

        public FirebaseUtility(string apiKey, string authDomain)
        {
            Instance = this;

            var config = new FirebaseAuthConfig
            {
                ApiKey = apiKey,
                AuthDomain = authDomain,
                Providers = [new EmailProvider()],
                UserRepository = new FileUserRepository("YourAppName")
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
    }

}

