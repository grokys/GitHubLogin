using System;
using System.Collections.Generic;

namespace GitHubLogin
{
    public class ConnectionEventArgs : EventArgs
    {
        public ConnectionEventArgs(ConnectionDetails connection)
        {
            Connections = new[] { connection };
        }

        public ConnectionEventArgs(IReadOnlyList<ConnectionDetails> connections)
        {
            Connections = connections;
        }

        public IReadOnlyList<ConnectionDetails> Connections { get; }
    }
}
