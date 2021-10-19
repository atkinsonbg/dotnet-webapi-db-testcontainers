using Amazon.Runtime;
using Amazon.S3;
using dotnet_webapi_db_testcontainers.Commands.AWS;
using dotnet_webapi_db_testcontainers.Commands.GitHub;
using dotnet_webapi_db_testcontainers.Data;
using Microsoft.Extensions.DependencyInjection;

// Namespace is different due to how Xunit.DependencyInjection interprets the base namespace
namespace tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<GithubUsersContext>();
            services.AddScoped<IGitHubLogic, GitHubLogic>();
            services.AddScoped<IS3Logic, S3Logic>();
            services.AddHttpClient<IGitHubLogic, GitHubLogic>();

            services.AddSingleton<IAmazonS3>(provider =>
            {
                var config = new AmazonS3Config { };

                // Have to setup the client with fake credentials
                // and configure the settings to talk to the LocalStack container
                var creds = new BasicAWSCredentials("xxx", "xxx");
                config.ForcePathStyle = true;
                // This url must be localhost as these containers are started via `docker run`
                config.ServiceURL = "http://localhost:4566";
                config.UseHttp = true;
                return new AmazonS3Client(creds, config);
            });
        }
    }
}