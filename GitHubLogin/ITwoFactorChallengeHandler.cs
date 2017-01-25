using System;
using System.Threading.Tasks;
using Octokit;

namespace GitHubLogin
{
    public interface ITwoFactorChallengeHandler
    {
        Task<TwoFactorChallengeResult> HandleTwoFactorException(
            IGitHubClient client,
            TwoFactorAuthorizationException exception);
    }
}
