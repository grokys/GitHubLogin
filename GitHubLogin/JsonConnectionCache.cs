using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace GitHubLogin
{
    public class JsonConnectionCache : IConnectionCache
    {
        string cachePath;

        public JsonConnectionCache(string cachePath)
        {
            this.cachePath = cachePath;
        }

        public Task<IEnumerable<ConnectionDetails>> Load()
        {
            if (File.Exists(cachePath))
            {
                try
                {
                    // TODO: Need a ReadAllTextAsync method here.
                    var data = File.ReadAllText(cachePath);
                    var result = SimpleJson.DeserializeObject<CacheData>(data);
                    return Task.FromResult(result.Connections.Select(FromCache));
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to read connection cache from {CachePath}", cachePath);
                }
            }

            return Task.FromResult(Enumerable.Empty<ConnectionDetails>());
        }

        public Task Save(IEnumerable<ConnectionDetails> connections)
        {
            var data = SimpleJson.SerializeObject(new CacheData
            {
                Connections = connections.Select(ToCache).ToList(),
            });

            try
            {
                File.WriteAllText(cachePath, data);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to write connection cache to {CachePath}", cachePath);
            }

            return Task.CompletedTask;
        }

        static ConnectionDetails FromCache(ConnectionCacheItem c)
        {
            return new ConnectionDetails(HostAddress.Create(c.HostUrl), c.UserName);
        }

        static ConnectionCacheItem ToCache(ConnectionDetails c)
        {
            return new ConnectionCacheItem
            {
                HostUrl = c.HostAddress.WebUri,
                UserName = c.UserName,
            };
        }

        class CacheData
        {
            public IEnumerable<ConnectionCacheItem> Connections { get; set; }
        }

        class ConnectionCacheItem
        {
            public Uri HostUrl { get; set; }
            public string UserName { get; set; }
        }
    }
}
