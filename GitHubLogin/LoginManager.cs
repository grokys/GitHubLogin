using System;
using System.Threading.Tasks;
using Octokit;

namespace GitHubLogin
{
    public class LoginManager
    {
        readonly string[] scopes = { "user", "repo", "gist", "write:public_key" };
        readonly ILoginCache loginCache;
        readonly string clientId;
        readonly string clientSecret;
        readonly string authorizationNote;
        readonly string fingerprint;

        public LoginManager(
            ILoginCache loginCache,
            string clientId = null,
            string clientSecret = null,
            string authorizationNote = null,
            string fingerprint = null)
        {
            Guard.ArgumentNotNull(loginCache, nameof(loginCache));

            this.loginCache = loginCache;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.authorizationNote = authorizationNote;
            this.fingerprint = fingerprint;
        }

        public async Task<User> Login(
            HostAddress hostAddress,
            IGitHubClient client,
            string userName,
            string password)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));
            Guard.ArgumentNotNull(client, nameof(client));
            Guard.ArgumentNotNullOrWhiteSpace(userName, nameof(userName));
            Guard.ArgumentNotNullOrWhiteSpace(password, nameof(password));

            var newAuth = new NewAuthorization
            {
                Scopes = scopes,
                Note = authorizationNote,
                Fingerprint = fingerprint,
            };

            var auth = await client.Authorization.GetOrCreateApplicationAuthentication(
                clientId,
                clientSecret,
                newAuth);

            await loginCache.SaveLogin(userName, auth.Token, hostAddress);

            return await client.User.Current();
        }
    }
}
