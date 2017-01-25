using System;
using System.Threading.Tasks;
using Octokit;

namespace GitHubLogin
{
    public class CredentialStore : ICredentialStore
    {
        readonly HostAddress hostAddress;
        readonly ILoginCache loginCache;

        public CredentialStore(HostAddress hostAddress, ILoginCache loginCache)
        {
            this.hostAddress = hostAddress;
            this.loginCache = loginCache;
        }

        public async Task<Credentials> GetCredentials()
        {
            try
            {
                var login = await loginCache.GetLogin(hostAddress).ConfigureAwait(false);
                return new Credentials(login.Item1, login.Item2);
            }
            catch
            {
                // TODO: Log and rethrow critical exceptions.
                return Credentials.Anonymous;
            }
        }
    }
}
