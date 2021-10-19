using System;

namespace dotnet_webapi_db_testcontainers.tests
{
    static class Constants
    {
        public const string sqlInitPath = "../../../../fixtures/sql/init.sql";
        public const string mockJsonPath = "../../../../fixtures/mocks";
        public const string mockAwsPath = "../../../../fixtures/localstack";
        public static string dockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST");
        public const string localDockerSocketPath = "unix:///var/run/docker.sock";
    }
}