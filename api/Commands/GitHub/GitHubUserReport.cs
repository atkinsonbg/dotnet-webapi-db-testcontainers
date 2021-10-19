using System;

namespace dotnet_webapi_db_testcontainers.Commands.GitHub
{
    public class GitHubUserReport
    {
        public String Name { get; set; }
        public int PullRequestCount { get; set; }
        public int CommitsCount { get; set; }
        public int CommentCount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}