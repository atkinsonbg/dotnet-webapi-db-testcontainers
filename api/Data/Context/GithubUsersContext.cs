using System;
using Microsoft.EntityFrameworkCore;

namespace dotnet_webapi_db_testcontainers.Data
{
    public class GithubUsersContext : DbContext
    {
        public DbSet<GithubUser> GithubUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING"));
    }
}