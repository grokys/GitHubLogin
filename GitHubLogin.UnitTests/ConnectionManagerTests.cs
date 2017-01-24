using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace GitHubLogin.UnitTests
{
    public class ConnectionManagerTests
    {
        [Fact]
        public void Connections_Should_Be_Initially_Empty()
        {
            var target = new ConnectionManager(MockCache());

            Assert.Empty(target.Connections);
        }

        [Fact]
        public async Task Connections_Should_Be_Setup_On_Initialze()
        {
            var target = new ConnectionManager(MockCache());

            await target.Initialize();

            Assert.Equal(new[]
            {
                new ConnectionDetails("https://github.com", "foo"),
                new ConnectionDetails("https://ghe.io", "bar"),
            }, target.Connections.ToArray());
        }

        [Fact]
        public async Task ConnectionsAdded_Should_Be_Raised_On_Initialze()
        {
            var target = new ConnectionManager(MockCache());
            var raised = false;

            target.ConnectionsAdded += (s, e) =>
            {
                Assert.Equal(new[]
                {
                    new ConnectionDetails("https://github.com", "foo"),
                    new ConnectionDetails("https://ghe.io", "bar"),
                }, e.Connections.ToArray());

                raised = true;
            };

            await target.Initialize();

            Assert.True(raised);
        }

        [Fact]
        public async Task Connection_Can_Be_Added()
        {
            var target = new ConnectionManager(MockCache());
            await target.Initialize();

            var expected = new ConnectionDetails("https://foo.com", "baz");
            var raised = false;

            target.ConnectionsAdded += (s, e) =>
            {
                Assert.Equal(new[] { expected }, e.Connections.ToArray());
                raised = true;
            };

            var connection = await target.Add(HostAddress.Create("https://foo.com"), "baz");

            Assert.Equal(new[]
            {
                new ConnectionDetails("https://github.com", "foo"),
                new ConnectionDetails("https://ghe.io", "bar"),
                expected,
            }, target.Connections.ToArray());

            Assert.Equal(expected, connection);
            Assert.True(raised);
        }

        [Fact]
        public async Task Adding_A_Connection_Saves_To_Cache()
        {
            var cache = MockCache();
            var target = new ConnectionManager(cache);
            await target.Initialize();

            var add = new ConnectionDetails("https://foo.com", "baz");
            var connection = await target.Add(HostAddress.Create("https://foo.com"), "baz");

            await cache.Received().Save(Arg.Is<IEnumerable<ConnectionDetails>>(x => 
                x.SequenceEqual(new[]
                {
                    new ConnectionDetails("https://github.com", "foo"),
                    new ConnectionDetails("https://ghe.io", "bar"),
                    add,
                })));
        }

        [Fact]
        public async Task Adding_Existing_Connection_Throws()
        {
            var target = new ConnectionManager(MockCache());
            await target.Initialize();

            var raised = false;

            target.ConnectionsAdded += (s, e) => raised = true;

            await Assert.ThrowsAsync<ArgumentException>(() => target.Add(HostAddress.Create("https://ghe.io"), "baz"));
            Assert.False(raised);
        }

        [Fact]
        public async Task Connection_Can_Be_Removed()
        {
            var target = new ConnectionManager(MockCache());
            await target.Initialize();

            var remove = new ConnectionDetails("https://ghe.io", "bar");
            var raised = false;

            target.ConnectionsRemoved += (s, e) =>
            {
                Assert.Equal(new[] { remove }, e.Connections.ToArray());
                raised = true;
            };

            var removed = await target.Remove(HostAddress.Create("https://ghe.io"));

            Assert.Equal(new[]
            {
                new ConnectionDetails("https://github.com", "foo"),
            }, target.Connections.ToArray());

            Assert.True(removed);
            Assert.True(raised);
        }

        [Fact]
        public async Task Removing_A_Connection_Saves_To_Cache()
        {
            var cache = MockCache();
            var target = new ConnectionManager(cache);
            await target.Initialize();

            var connection = await target.Remove(HostAddress.Create("https://github.com"));

            await cache.Received().Save(Arg.Is<IEnumerable<ConnectionDetails>>(x =>
                x.SequenceEqual(new[]
                {
                    new ConnectionDetails("https://ghe.io", "bar"),
                })));
        }

        IConnectionCache MockCache()
        {
            var result = Substitute.For<IConnectionCache>();
            result.Load().Returns(new[]
            {
                new ConnectionDetails("https://github.com", "foo"),
                new ConnectionDetails("https://ghe.io", "bar"),
            });
            return result;
        }
    }
}
