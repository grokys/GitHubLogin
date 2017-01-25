using System;
using System.Threading.Tasks;
using GitHubLogin;
using Octokit;

namespace LoginConsole
{
    class ConsoleTwoFactorChallengeHandler : ITwoFactorChallengeHandler
    {
        public Task<TwoFactorChallengeResult> HandleTwoFactorException(
            IGitHubClient client,
            TwoFactorAuthorizationException exception)
        {
            Console.Write("Two Factor Code: ");
            var code = Console.ReadLine();
            return Task.FromResult(new TwoFactorChallengeResult(code));
        }
    }
}
