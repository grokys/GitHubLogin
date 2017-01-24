using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHubLogin
{
    /// <summary>
    /// Maintains a list of <see cref="ConnectionDetails"/> objects which are loaded from and
    /// saved to a cache.
    /// </summary>
    public interface IConnectionManager
    {
        /// <summary>
        /// Gets the current connections.
        /// </summary>
        IReadOnlyList<ConnectionDetails> Connections { get; }

        /// <summary>
        /// Raised when connections are added.
        /// </summary>
        event EventHandler<ConnectionEventArgs> ConnectionsAdded;

        /// <summary>
        /// Raised when connections are removed.
        /// </summary>
        event EventHandler<ConnectionEventArgs> ConnectionsRemoved;

        /// <summary>
        /// Adds a connection.
        /// </summary>
        /// <param name="address">The host address of the connection.</param>
        /// <param name="userName">The username.</param>
        /// <returns>A task which returns the added <see cref="ConnectionDetails"/> object.</returns>
        /// <exception cref="ArgumentException">
        /// A connection with the same host address already exists.
        /// </exception>
        Task<ConnectionDetails> Add(HostAddress address, string userName);

        /// <summary>
        /// Checks wether a connection with the specified host address is present.
        /// </summary>
        /// <param name="address">The host address of the connection.</param>
        /// <returns>
        /// True if a connection with the specified host address is present; otherwise false.
        /// </returns>
        bool Exists(HostAddress address);

        /// <summary>
        /// Trries to find a connection with the specified host address.
        /// </summary>
        /// <param name="address">The host address of the connection.</param>
        /// <returns>The matching connection if found, otherwise null.</returns>
        ConnectionDetails Find(HostAddress address);

        /// <summary>
        /// Initializes the connection manager by loading connections from the cache.
        /// </summary>
        /// <returns>A task representing the operation.</returns>
        Task Initialize();

        /// <summary>
        /// Removes a connection.
        /// </summary>
        /// <param name="address">The host address of the connection.</param>
        /// <returns>A task which returns a value indicating whether a matching connection was found.</returns>
        Task<bool> Remove(HostAddress address);
    }
}