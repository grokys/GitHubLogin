using System;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Akavache;

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

        public Task SaveLogin(string userName, string password, HostAddress hostAddress)
        {
            Guard.ArgumentNotNullOrWhiteSpace(userName, nameof(userName));
            Guard.ArgumentNotNullOrWhiteSpace(password, nameof(password));
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            return cache.SaveLogin(userName, password, hostAddress.CredentialCacheKeyHost).ToTask();
        }
    }
}
