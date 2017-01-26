using System;
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
        static ILoginCache loginCache;
        static ILoginManager loginManager;

        static void Main(string[] args)
        {
            BlobCache.ApplicationName = "LoginConsole";

            loginCache = new AkavacheLoginCache(BlobCache.Secure);
            loginManager = new LoginManager(
                loginCache,
                new ConsoleTwoFactorChallengeHandler(),
                args[0],
                args[1],
                $"LoginConsole on {Dns.GetHostName()}");

            var client = new GitHubClient(
                new ProductHeaderValue("LoginConsole"),
                new CredentialStore(HostAddress.GitHubDotComHostAddress, loginCache));

            Console.Write("Username: ");
            var userName = Console.ReadLine();
            Console.Write("Password: ");
            var password = Console.ReadLine();

            try
            {
                DoLogin(client, userName, password).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException.Message);
            }

            Console.ReadKey();
        }

        static async Task DoLogin(IGitHubClient client, string userName, string password)
        {
            var user = await loginManager.Login(
                HostAddress.GitHubDotComHostAddress,
                client,
                userName,
                password);

            Console.WriteLine($"Logged in: {user.Email}");
        }
    }
}
