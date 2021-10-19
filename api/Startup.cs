using Amazon.Runtime;
using Amazon.S3;
using dotnet_webapi_db_testcontainers.Commands.AWS;
using dotnet_webapi_db_testcontainers.Commands.GitHub;
using dotnet_webapi_db_testcontainers.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace dotnet_webapi_db_testcontainers
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddRouting();
            services.AddDbContext<GithubUsersContext>();
            services.AddScoped<IGitHubLogic, GitHubLogic>();
            services.AddScoped<IS3Logic, S3Logic>();
            services.AddHttpClient<IGitHubLogic, GitHubLogic>();

            services.AddSingleton<IAmazonS3>(provider =>
            {
                var config = new AmazonS3Config { };

                if (provider.GetService<IHostEnvironment>().IsDevelopment())
                {
                    // If in Dev mode, we have to setup the client with fake credentials
                    // and configure the settings to talk to the LocalStack container
                    var creds = new BasicAWSCredentials("xxx", "xxx");
                    config.ForcePathStyle = true;
                    // This url must match the name of the container in the compose file
                    config.ServiceURL = "http://domain_localstack:4566";
                    config.UseHttp = true;
                    return new AmazonS3Client(creds, config);
                }
                else
                {
                    // Otherwise, just return a vanilla Client with an empty config
                    // or config as needed for deployment
                    return new AmazonS3Client(config);
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
