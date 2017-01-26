using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace GitHubLogin
{
    /// <summary>
    /// Maintains a list of <see cref="ConnectionDetails"/> objects which are loaded from and
    /// saved to a cache.
    /// </summary>
    public class ConnectionManager : IConnectionManager
    {
        readonly IConnectionCache cache;
        readonly List<ConnectionDetails> inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionManager"/> class.
        /// </summary>
        /// <param name="cache">The cache from which to load/save connections.</param>
        public ConnectionManager(IConnectionCache cache)
        {
            Guard.ArgumentNotNull(cache, nameof(cache));

            this.cache = cache;
            inner = new List<ConnectionDetails>();
        }

        /// <inheritdoc/>
        public IReadOnlyList<ConnectionDetails> Connections => inner;

        /// <inheritdoc/>
        public event EventHandler<ConnectionEventArgs> ConnectionsAdded;

        /// <inheritdoc/>
        public event EventHandler<ConnectionEventArgs> ConnectionsRemoved;

        /// <inheritdoc/>
        public async Task Initialize()
        {
            try
            {
                var connections = (await cache.Load().ConfigureAwait(false)).ToList();
                inner.AddRange(connections);

                if (ConnectionsAdded != null)
                {
                    ConnectionsAdded(this, new ConnectionEventArgs(connections));
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to load connection cache");
            }
        }

        /// <inheritdoc/>
        public async Task<ConnectionDetails> Add(HostAddress hostAddress, string userName)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));
            Guard.ArgumentNotNullOrWhiteSpace(userName, nameof(userName));

            if (Exists(hostAddress))
            {
                throw new ArgumentException($"A connection with the host address '{hostAddress}' already exists.");
            }

            var connection = new ConnectionDetails(hostAddress, userName);

            inner.Add(connection);

            if (ConnectionsAdded != null)
            {
                ConnectionsAdded(this, new ConnectionEventArgs(connection));
            }

            await cache.Save(inner).ConfigureAwait(false);
            return connection;
        }

        /// <inheritdoc/>
        public async Task<bool> Remove(HostAddress hostAddress)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            var connection = Find(hostAddress);

            if (connection != null)
            {
                inner.Remove(connection);

                if (ConnectionsRemoved != null)
                {
                    ConnectionsRemoved(this, new ConnectionEventArgs(connection));
                }

                await cache.Save(inner).ConfigureAwait(false);
            }

            return connection != null;
        }

        /// <inheritdoc/>
        public bool Exists(HostAddress hostAddress)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            return Find(hostAddress) != null;
        }

        /// <inheritdoc/>
        public ConnectionDetails Find(HostAddress hostAddress)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            return inner.FirstOrDefault(x => x.HostAddress == hostAddress);
        }
    }
}
