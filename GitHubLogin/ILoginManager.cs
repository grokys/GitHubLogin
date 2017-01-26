using System.Threading.Tasks;
using Octokit;

namespace GitHubLogin
{
    /// <summary>
    /// Provides services for logging into a GitHub server.
    /// </summary>
    public interface ILoginManager
    {
        /// <summary>
        /// Attempts to log into a GitHub server.
        /// </summary>
        /// <param name="hostAddress">The address of the server.</param>
        /// <param name="client">An octokit client configured to access the server.</param>
        /// <param name="userName">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>The logged in user.</returns>
        /// <exception cref="AuthorizationException">
        /// The login authorization failed.
        /// </exception>
        Task<User> Login(HostAddress hostAddress, IGitHubClient client, string userName, string password);

        /// <summary>
        /// Attempts to log into a GitHub server using existing credentials.
        /// </summary>
        /// <param name="hostAddress">The address of the server.</param>
        /// <param name="client">An octokit client configured to access the server.</param>
        /// <returns>The logged in user.</returns>
        /// <exception cref="AuthorizationException">
        /// The login authorization failed.
        /// </exception>
        Task<User> LoginFromCache(HostAddress hostAddress, IGitHubClient client);
    }
}