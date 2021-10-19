using System;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Modules;
using DotNet.Testcontainers.Containers.WaitStrategies;
using dotnet_webapi_db_testcontainers.Commands.GitHub;
using Xunit;

namespace dotnet_webapi_db_testcontainers.tests
{
    public class MockAPIFixture : IDisposable
    {
        public TestcontainersContainer _testcontainer { get; set; }

        public MockAPIFixture()
        {
            var testcontainersBuilder = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("atkinsonbg/mock-server:0.1")
                .WithMount(Constants.mockJsonPath, "/api/Mocks")
                .WithDockerEndpoint(!String.IsNullOrEmpty(Constants.dockerHost) ? Constants.dockerHost : Constants.localDockerSocketPath)
                .WithPortBinding(5000, 5000)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5000));

            _testcontainer = testcontainersBuilder.Build();
            Console.WriteLine("--> Starting Simple Mock Server container...");
            _testcontainer.StartAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Console.WriteLine("<-- Stopping Simple Mock Server container...");
            _testcontainer.StopAsync();
            _testcontainer.DisposeAsync();
        }
    }

    [CollectionDefinition("Mock APIs collection")]
    public class MockAPICollection : ICollectionFixture<MockAPIFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    [Collection("Mock APIs collection")]
    public class TestAPICalls
    {
        private IGitHubLogic _query;

        public TestAPICalls(IGitHubLogic query)
        {
            _query = query;

            Environment.SetEnvironmentVariable("GITHUB_BASE_URL", "http://localhost:5000");
        }

        [Fact]
        public void TestUseridPayload()
        {
            Console.WriteLine("Starting TestUseridPayload...");
            var report = _query.GetUserReport("atkinsonbg");
            var expected = "atkinsonbg";
            Assert.Equal(report.Name, expected);
        }

        [Fact]
        public void TestVanityNamePayload()
        {
            Console.WriteLine("Starting TestVanityNamePayload...");
            var report = _query.GetUserReport("raptor");
            var expected = "raptor";
            Assert.Equal(report.Name, expected);
        }
    }
}
