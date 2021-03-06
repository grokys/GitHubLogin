﻿using System;
using System.Threading.Tasks;
using GitHubLogin;
using Octokit;

namespace LoginConsole
{
    class ConsoleTwoFactorChallengeHandler : ITwoFactorChallengeHandler
    {
        public Task<TwoFactorChallengeResult> HandleTwoFactorException(
            TwoFactorAuthorizationException exception)
        {
            Console.Write("Two Factor Code: ");
            var code = Console.ReadLine();

            if (code != "resend")
            {
                return Task.FromResult(new TwoFactorChallengeResult(code));
            }
            else
            {
                return Task.FromResult(TwoFactorChallengeResult.RequestResendCode);
            }
        }

        public Task ChallengeFailed(Exception e)
        {
            Console.WriteLine("Challenge failed: {0}", e.Message);
            return Task.CompletedTask;
        }
    }
}
