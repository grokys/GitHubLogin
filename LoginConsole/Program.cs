using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Akavache;
using GitHubLogin;
using Octokit;

namespace LoginConsole
{
    class Program
    {
        // Services that would be registered with a DI container
        static IConnectionCache connectionCache;
        static IConnectionManager connectionManager;
        static ILoginCache loginCache;
        static ILoginManager loginManager;

        static void Main(string[] args)
        {
            BlobCache.ApplicationName = "LoginConsole";

            connectionCache = new JsonConnectionCache("connections.json");
            connectionManager = new ConnectionManager(connectionCache);
            connectionManager.Initialize().Wait();

            loginCache = new WindowsLoginCache();
            loginManager = new LoginManager(
                loginCache,
                new ConsoleTwoFactorChallengeHandler(),
                args[0],
                args[1],
                $"LoginConsole on {Dns.GetHostName()}");

            PrintConnections();
            PrintHelp();
            RunCommandLine().Wait();
        }

        static void PrintConnections()
        {
            if (connectionManager.Connections.Count > 0)
            {
                int index = 0;

                foreach (var c in connectionManager.Connections)
                {
                    Console.WriteLine($"{index}. {c.HostAddress} {c.UserName}");
                }
            }
            else
            {
                Console.WriteLine("No connections configured.");
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine();
            Console.WriteLine(
                "Commands:\n" +
                "  login [connection #]\n" +
                "  login [user] [pass] ([host])");
            Console.WriteLine();
        }

        static async Task RunCommandLine()
        {
            for (;;)
            {
                try
                {
                    var line = Console.ReadLine();
                    var tokens = line.Split(' ');

                    if (tokens.Length > 0)
                    {
                        switch (tokens[0])
                        {
                            case "login":
                                await DoLogin(tokens.Skip(1).ToArray());
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        static async Task DoLogin(string[] tokens)
        {
            User user;

            if (tokens.Length == 1)
            {
                var index = int.Parse(tokens[0]);
                var connection = connectionManager.Connections[index];

                var client = new GitHubClient(
                    new ProductHeaderValue("LoginConsole"),
                    new CredentialStore(connection.HostAddress, loginCache));

                user = await loginManager.LoginFromCache(HostAddress.GitHubDotComHostAddress, client);
            }
            else if (tokens.Length == 2 || tokens.Length == 3)
            {
                var host = tokens.Length == 3 ? HostAddress.Create(tokens[2]) : HostAddress.GitHubDotComHostAddress;

                if (connectionManager.Exists(host))
                {
                    throw new Exception($"Connection to {host.WebUri} already exists. Log out first.");
                }

                var client = new GitHubClient(
                    new ProductHeaderValue("LoginConsole"),
                    new CredentialStore(host, loginCache));

                user = await loginManager.Login(
                    HostAddress.GitHubDotComHostAddress,
                    client,
                    tokens[0],
                    tokens[1]);

                var connection = await connectionManager.Add(host, tokens[0]);
            }
            else
            {
                throw new Exception("Usage:\n" +
                    "  login [connection #]\n" +
                    "  login [user] [pass] ([host])");
            }

            Console.WriteLine($"Logged in: {user.Email}");
        }
    }
}
