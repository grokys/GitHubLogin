using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHubLogin
{
    public interface IConnectionCache
    {
        Task<IEnumerable<ConnectionDetails>> Load();
        Task Save(IEnumerable<ConnectionDetails> connections);
    }
}
