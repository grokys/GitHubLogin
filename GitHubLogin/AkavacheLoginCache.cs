using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Akavache;
using Octokit;

namespace GitHubLogin
{
    public class AkavacheLoginCache : ILoginCache
    {
        ISecureBlobCache cache;

        public AkavacheLoginCache(ISecureBlobCache cache)
        {
            Guard.ArgumentNotNull(cache, nameof(cache));

            this.cache = cache;
        }

        public async Task<Tuple<string, string>> GetLogin(HostAddress hostAddress)
        {
            var login = await cache.GetLoginAsync(hostAddress.CredentialCacheKeyHost);
            return Tuple.Create(login.UserName, login.Password);
        }

        public Task SaveLogin(string userName, string password, HostAddress hostAddress)
        {
            Guard.ArgumentNotNullOrWhiteSpace(userName, nameof(userName));
            Guard.ArgumentNotNullOrWhiteSpace(password, nameof(password));
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            return cache.SaveLogin(userName, password, hostAddress.CredentialCacheKeyHost).ToTask();
        }
    }
}
