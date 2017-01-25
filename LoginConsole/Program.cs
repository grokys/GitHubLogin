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
        static void Main(string[] args)
        {
            BlobCache.ApplicationName = "LoginConsole";

            var loginCache = new AkavacheLoginCache(BlobCache.Secure);
            var loginManager = new LoginManager(
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
                DoLogin(client, loginManager, userName, password).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException.Message);
            }

            Console.ReadKey();
        }

        static async Task DoLogin(IGitHubClient client, LoginManager loginManager, string userName, string password)
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
