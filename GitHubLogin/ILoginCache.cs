using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubLogin
{
    public interface ILoginCache
    {
        Task SaveLogin(string user, string password, HostAddress hostAddress);
    }
}
