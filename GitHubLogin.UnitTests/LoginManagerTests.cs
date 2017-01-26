using System;
using System.Threading.Tasks;
using NSubstitute;
using Octokit;
using Xunit;

namespace GitHubLogin.UnitTests
{
    public class LoginManagerTests
    {
        static readonly HostAddress host = HostAddress.GitHubDotComHostAddress;

        [Fact]
        public async Task Login_Token_Is_Saved_To_Cache()
        {
            var client = Substitute.For<IGitHubClient>();
            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>())
                .Returns(new ApplicationAuthorization("123abc"));

            var loginCache = Substitute.For<ILoginCache>();
            var tfa = Substitute.For<ITwoFactorChallengeHandler>();

            var target = new LoginManager(loginCache, tfa, "id", "secret");
            await target.Login(host, client, "foo", "bar");

            await loginCache.Received().SaveLogin("foo", "123abc", host);
        }

        [Fact]
        public async Task Logged_In_User_Is_Returned()
        {
            var client = Substitute.For<IGitHubClient>();
            var user = new User();
            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>())
                .Returns(new ApplicationAuthorization("123abc"));
            client.User.Current().Returns(user);

            var loginCache = Substitute.For<ILoginCache>();
            var tfa = Substitute.For<ITwoFactorChallengeHandler>();

            var target = new LoginManager(loginCache, tfa, "id", "secret");
            var result = await target.Login(host, client, "foo", "bar");

            Assert.Same(user, result);
        }

        [Fact]
        public async Task TwoFactor_Exception_Is_Passed_To_Handler()
        {
            var client = Substitute.For<IGitHubClient>();
            var exception = new TwoFactorChallengeFailedException();

            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ => { throw exception; });
            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>(), "def567")
                .Returns(new ApplicationAuthorization("123abc"));

            var loginCache = Substitute.For<ILoginCache>();
            var tfa = Substitute.For<ITwoFactorChallengeHandler>();
            tfa.HandleTwoFactorException(client, exception).Returns(new TwoFactorChallengeResult("def567"));

            var target = new LoginManager(loginCache, tfa, "id", "secret");
            await target.Login(host, client, "foo", "bar");
        }

        [Fact]
        public async Task RequestResendCode_Results_In_Retrying_Login()
        {
            var client = Substitute.For<IGitHubClient>();
            var exception = new TwoFactorChallengeFailedException();
            var user = new User();

            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ => { throw exception; });
            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>(), "def567")
                .Returns(new ApplicationAuthorization("456def"));
            client.User.Current().Returns(user);

            var loginCache = Substitute.For<ILoginCache>();
            var tfa = Substitute.For<ITwoFactorChallengeHandler>();
            tfa.HandleTwoFactorException(client, exception).Returns(
                TwoFactorChallengeResult.RequestResendCode,
                new TwoFactorChallengeResult("def567"));

            var target = new LoginManager(loginCache, tfa, "id", "secret");
            await target.Login(host, client, "foo", "bar");

            await client.Authorization.Received(2).Create("id", "secret", Arg.Any<NewAuthorization>());
        }
    }
}
