using System;
using System.Threading.Tasks;

namespace GitHubLogin
{
    public interface ILoginCache
    {
        Task<Tuple<string, string>> GetLogin(HostAddress hostAddress);
        Task SaveLogin(string user, string password, HostAddress hostAddress);
    }
}
