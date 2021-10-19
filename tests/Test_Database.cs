using System;
using System.Linq;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Modules;
using DotNet.Testcontainers.Containers.WaitStrategies;
using dotnet_webapi_db_testcontainers.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace dotnet_webapi_db_testcontainers.tests
{
    public class MockDatabaseFixture : IDisposable
    {
        public TestcontainersContainer _testcontainer { get; set; }

        public MockDatabaseFixture()
        {
            Environment.SetEnvironmentVariable("CONNECTION_STRING", "Host=localhost;Database=testcontainer-db;Username=postgres");

            var testcontainersBuilder = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("postgres:11.6")
                .WithEnvironment("POSTGRES_DB", "testcontainer-db")
                .WithMount(Constants.sqlInitPath, "/docker-entrypoint-initdb.d/init.sql")
                .WithPortBinding(5432, 5432)
                .WithDockerEndpoint(!String.IsNullOrEmpty(Constants.dockerHost) ? Constants.dockerHost : Constants.localDockerSocketPath)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"));

            _testcontainer = testcontainersBuilder.Build();
            Console.WriteLine("--> Starting Postgres container...");
            _testcontainer.StartAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Console.WriteLine("<-- Stopping Postgres container...");
            _testcontainer.StopAsync();
            _testcontainer.DisposeAsync();
        }
    }

    [CollectionDefinition("Mock Database collection")]
    public class MockDatabaseCollection : ICollectionFixture<MockDatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    [Collection("Mock Database collection")]
    public class TestDatabase
    {
        [Fact]
        public async void TestCountOfUsers()
        {
            Console.WriteLine("Starting TestCountOfUsers...");
            var ctx = new GithubUsersContext();
            var r = await ctx.GithubUsers.ToListAsync();
            Assert.Equal(3, r.Count);
        }

        [Fact]
        public async void Testatkinsonbg()
        {
            Console.WriteLine("Starting Testatkinsonbg...");
            var ctx = new GithubUsersContext();
            var r = await ctx.GithubUsers.ToListAsync();
            var s = r.Where(x => x.Userid == "atkinsonbg").FirstOrDefault();
            Assert.Equal("Brandon Atkinson", s.Name);
        }
    }
}
