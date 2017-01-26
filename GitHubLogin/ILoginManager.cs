using System.Threading.Tasks;
using Octokit;

namespace GitHubLogin
{
    public interface ILoginManager
    {
        Task<User> Login(HostAddress hostAddress, IGitHubClient client, string userName, string password);
    }
}