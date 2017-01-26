using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Akavache;
using Octokit;
using Serilog;

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
            try
            {
                var login = await cache.GetLoginAsync(hostAddress.CredentialCacheKeyHost);
                return Tuple.Create(login.UserName, login.Password);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to get login for {HostAddress} from akavache cache", hostAddress);
                return Tuple.Create<string, string>(null, null);
            }
        }

        public Task SaveLogin(string userName, string password, HostAddress hostAddress)
        {
            Guard.ArgumentNotNullOrWhiteSpace(userName, nameof(userName));
            Guard.ArgumentNotNullOrWhiteSpace(password, nameof(password));
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            try
            {
                return cache.SaveLogin(userName, password, hostAddress.CredentialCacheKeyHost).ToTask();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to save login for {HostAddress} to akavache cache", hostAddress);
                return Task.CompletedTask;
            }
        }
    }
}
