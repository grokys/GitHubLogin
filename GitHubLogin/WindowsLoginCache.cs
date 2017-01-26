using System;
using System.Threading.Tasks;
using GitHub.Authentication.CredentialManagement;

namespace GitHubLogin
{
    /// <summary>
    /// A login cache that stores logins in the windows credential cache.
    /// </summary>
    public class WindowsLoginCache : ILoginCache
    {
        public Task<Tuple<string, string>> GetLogin(HostAddress hostAddress)
        {
            var keyHost = GetKeyHost(hostAddress.CredentialCacheKeyHost);

            using (var credential = new Credential())
            {
                credential.Target = keyHost;
                credential.Type = CredentialType.Generic;
                if (credential.Load())
                    return Task.FromResult(Tuple.Create(credential.Username, credential.Password));
            }

            return Task.FromResult(Tuple.Create<string, string>(null, null));
        }

        public Task SaveLogin(string user, string password, HostAddress hostAddress)
        {
            var keyHost = GetKeyHost(hostAddress.CredentialCacheKeyHost);

            using (var credential = new Credential(user, password, keyHost))
            {
                credential.Save();
            }

            return Task.CompletedTask;
        }

        static string GetKeyHost(string key)
        {
            key = FormatKey(key);
            if (key.StartsWith("git:", StringComparison.Ordinal))
                key = key.Substring("git:".Length);
            if (!key.EndsWith("/", StringComparison.Ordinal))
                key += '/';
            return key;
        }

        static string FormatKey(string key)
        {
            if (key.StartsWith("login:", StringComparison.Ordinal))
                key = key.Substring("login:".Length);
            return key;
        }
    }
}
