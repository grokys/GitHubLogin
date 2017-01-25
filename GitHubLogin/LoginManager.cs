using System;
using System.Threading.Tasks;
using Octokit;

namespace GitHubLogin
{
    public class LoginManager
    {
        readonly string[] scopes = { "user", "repo", "gist", "write:public_key" };
        readonly ILoginCache loginCache;
        readonly ITwoFactorChallengeHandler twoFactorChallengeHandler;
        readonly string clientId;
        readonly string clientSecret;
        readonly string authorizationNote;
        readonly string fingerprint;

        public LoginManager(
            ILoginCache loginCache,
            ITwoFactorChallengeHandler twoFactorChallengeHandler,
            string clientId = null,
            string clientSecret = null,
            string authorizationNote = null,
            string fingerprint = null)
        {
            Guard.ArgumentNotNull(loginCache, nameof(loginCache));
            Guard.ArgumentNotNull(twoFactorChallengeHandler, nameof(twoFactorChallengeHandler));

            this.loginCache = loginCache;
            this.twoFactorChallengeHandler = twoFactorChallengeHandler;
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

            // Start by saving the username and password, these will be used by the `IGitHubClient`
            // until an authorization token has been created and acquired:
            await loginCache.SaveLogin(userName, password, hostAddress).ConfigureAwait(false);

            var newAuth = new NewAuthorization
            {
                Scopes = scopes,
                Note = authorizationNote,
                Fingerprint = fingerprint,
            };

            ApplicationAuthorization auth;

            try
            {
                auth = await client.Authorization.Create(
                    clientId,
                    clientSecret,
                    newAuth).ConfigureAwait(false);
            }
            catch (TwoFactorAuthorizationException e)
            {
                var challengeResult = await twoFactorChallengeHandler.HandleTwoFactorException(client, e);

                auth = await client.Authorization.Create(
                    clientId,
                    clientSecret,
                    newAuth,
                    challengeResult.AuthenticationCode).ConfigureAwait(false);
            }

            await loginCache.SaveLogin(userName, auth.Token, hostAddress).ConfigureAwait(false);
            return await client.User.Current().ConfigureAwait(false);
        }
    }
}
