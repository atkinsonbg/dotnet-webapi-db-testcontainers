using System;
using System.IO;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Modules;
using DotNet.Testcontainers.Containers.OutputConsumers;
using DotNet.Testcontainers.Containers.WaitStrategies;
using dotnet_webapi_db_testcontainers.Commands.AWS;
using Xunit;

namespace dotnet_webapi_db_testcontainers.tests
{
    public class MockAWSFixture : IDisposable
    {
        public TestcontainersContainer _testcontainer { get; set; }
        public IOutputConsumer _consumer { get; set; }

        public MockAWSFixture()
        {
            _consumer = Consume.RedirectStdoutAndStderrToStream(new MemoryStream(), new MemoryStream());

            var testcontainersBuilder = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("localstack/localstack:latest")
                .WithEnvironment("SERVICES", "s3")
                .WithEnvironment("DEBUG", "1")
                .WithEnvironment("DATA_DIR", "/tmp/localstack/data")
                .WithMount(Constants.mockAwsPath, "/tmp/localstack")
                .WithPortBinding(4566, 4566)
                .WithDockerEndpoint(!String.IsNullOrEmpty(Constants.dockerHost) ? Constants.dockerHost : Constants.localDockerSocketPath)
                .WithOutputConsumer(_consumer)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(_consumer.Stdout, "Ready."));

            _testcontainer = testcontainersBuilder.Build();
            Console.WriteLine("--> Starting LocalStack container...");
            _testcontainer.StartAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Console.WriteLine("<-- Stopping LocalStack container...");
            _testcontainer.StopAsync();
            _testcontainer.DisposeAsync();
            _consumer.Dispose();
        }
    }

    [CollectionDefinition("Mock AWS collection")]
    public class MockAWSCollection : ICollectionFixture<MockAWSFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    [Collection("Mock AWS collection")]
    public class TestAWSCalls
    {
        private IS3Logic _query;

        public TestAWSCalls(IS3Logic query)
        {
            _query = query;

            Environment.SetEnvironmentVariable("AWS_PROFILE", "stack-profile");
        }

        [Fact]
        public void TestGetUserReport()
        {
            Console.WriteLine("Starting TestGetUserReport...");

            var r = _query.GetReport("atkinsonbg");
            var expected = "atkinsonbg";

            Assert.Contains(expected, r.Result);
        }

        [Fact]
        public void TestGetUserReportFailure()
        {
            Console.WriteLine("Starting TestGetUserReportFailure...");
            var r = _query.GetReport("404");
            Assert.True(String.IsNullOrEmpty(r.Result));
        }
    }
}
