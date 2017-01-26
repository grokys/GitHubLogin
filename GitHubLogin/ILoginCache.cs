using System;
using System.Threading.Tasks;

namespace GitHubLogin
{
    public interface ILoginCache
    {
        Task<Tuple<string, string>> GetLogin(HostAddress hostAddress);
        Task SaveLogin(string userName, string password, HostAddress hostAddress);
        Task EraseLogin(HostAddress hostAddress);
    }
}
