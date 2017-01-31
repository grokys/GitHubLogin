using System;
using System.Threading.Tasks;
using Octokit;

namespace GitHubLogin
{
    public interface ITwoFactorChallengeHandler
    {
        Task<TwoFactorChallengeResult> HandleTwoFactorException(TwoFactorAuthorizationException exception);
    }
}
