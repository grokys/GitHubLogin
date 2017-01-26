using System;
using System.Threading.Tasks;
using Octokit;

namespace GitHubLogin
{
    /// <summary>
    /// Provides services for logging into a GitHub server.
    /// </summary>
    public class LoginManager : ILoginManager
    {
        readonly string[] scopes = { "user", "repo", "gist", "write:public_key" };
        readonly ILoginCache loginCache;
        readonly ITwoFactorChallengeHandler twoFactorChallengeHandler;
        readonly string clientId;
        readonly string clientSecret;
        readonly string authorizationNote;
        readonly string fingerprint;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginManager"/> class.
        /// </summary>
        /// <param name="loginCache">The cache in which to store login details.</param>
        /// <param name="twoFactorChallengeHandler">The handler for 2FA challenges.</param>
        /// <param name="clientId">The application's client API ID.</param>
        /// <param name="clientSecret">The application's client API secret.</param>
        /// <param name="authorizationNote">An note to store with the authorization.</param>
        /// <param name="fingerprint">The machine fingerprint.</param>
        public LoginManager(
            ILoginCache loginCache,
            ITwoFactorChallengeHandler twoFactorChallengeHandler,
            string clientId,
            string clientSecret,
            string authorizationNote = null,
            string fingerprint = null)
        {
            Guard.ArgumentNotNull(loginCache, nameof(loginCache));
            Guard.ArgumentNotNull(twoFactorChallengeHandler, nameof(twoFactorChallengeHandler));
            Guard.ArgumentNotNullOrWhiteSpace(clientId, nameof(clientId));
            Guard.ArgumentNotNullOrWhiteSpace(clientSecret, nameof(clientSecret));

            this.loginCache = loginCache;
            this.twoFactorChallengeHandler = twoFactorChallengeHandler;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.authorizationNote = authorizationNote;
            this.fingerprint = fingerprint;
        }

        /// <inheritdoc/>
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

            ApplicationAuthorization auth = null;

            do
            {
                try
                {
                    auth = await client.Authorization.Create(
                        clientId,
                        clientSecret,
                        newAuth).ConfigureAwait(false);
                    EnsureNonNullAuthorization(auth);
                }
                catch (TwoFactorAuthorizationException e)
                {
                    var challengeResult = await twoFactorChallengeHandler.HandleTwoFactorException(client, e);

                    if (challengeResult == null)
                    {
                        throw new InvalidOperationException(
                            "ITwoFactorChallengeHandler.HandleTwoFactorException returned null.");
                    }

                    if (!challengeResult.ResendCodeRequested)
                    {
                        auth = await client.Authorization.Create(
                            clientId,
                            clientSecret,
                            newAuth,
                            challengeResult.AuthenticationCode).ConfigureAwait(false);
                        EnsureNonNullAuthorization(auth);
                    }
                }
            } while (auth == null);

            await loginCache.SaveLogin(userName, auth.Token, hostAddress).ConfigureAwait(false);
            return await client.User.Current().ConfigureAwait(false);
        }

        public Task<User> LoginFromCache(HostAddress hostAddress, IGitHubClient client) => client.User.Current();

        void EnsureNonNullAuthorization(ApplicationAuthorization auth)
        {
            // If a mock IGitHubClient is not set up correctly, it can return null from
            // IGutHubClient.Authorization.Create - this will cause an infinite loop in Login()
            // so prevent that.
            if (auth == null)
            {
                throw new InvalidOperationException("IGutHubClient.Authorization.Create returned null.");
            }
        }
    }
}
