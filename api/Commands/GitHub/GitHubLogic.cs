using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using dotnet_webapi_db_testcontainers.Commands.AWS;
using dotnet_webapi_db_testcontainers.Data;

namespace dotnet_webapi_db_testcontainers.Commands.GitHub
{
    public class GitHubLogic : IGitHubLogic
    {
        private GithubUsersContext _context;
        private HttpClient _httpClient;
        private IS3Logic _s3Query;

        public GitHubLogic(GithubUsersContext context, HttpClient httpClient, IS3Logic s3Query)
        {
            _context = context;
            _httpClient = httpClient;
            _s3Query = s3Query;
        }

        // Pulls are users from the database
        public List<GithubUser> GetAllUsers()
        {
            try
            {
                return _context.GithubUsers.ToList();
            }
            catch (DbException exception)
            {
                Console.WriteLine(exception);
                return null;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        // Pulls a specific user from the database
        public GithubUser GetUser(string userid)
        {
            try
            {
                return _context.GithubUsers.Where(u => u.Userid == userid).FirstOrDefault();
            }
            catch (DbException exception)
            {
                Console.WriteLine(exception);
                return null;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        // Pulls specific GitHub metrics for a user
        public GitHubUserReport GetUserReport(string userid)
        {
            GitHubUserReport report = new GitHubUserReport();

            var response = _httpClient.GetStringAsync($"{Environment.GetEnvironmentVariable("GITHUB_BASE_URL")}/api/v3/users/{userid}/events").Result;
            var json = JsonDocument.Parse(response);

            report.Name = userid;
            report.Timestamp = DateTime.Now;
            report.PullRequestCount = json.RootElement.EnumerateArray().Where(z => z.GetProperty("type").ToString() == "PullRequestEvent").Count();
            report.CommentCount = json.RootElement.EnumerateArray().Where(z => z.GetProperty("type").ToString() == "PullRequestReviewCommentEvent").Count();
            report.CommitsCount = json.RootElement.EnumerateArray().Where(z => z.GetProperty("type").ToString() == "PushEvent" || z.GetProperty("type").ToString() == "CreateEvent").Count();

            return report;
        }

        // Pulls specific GitHub metrics for a user, then saves a report to S3
        public bool SaveUserReport(string userid)
        {
            var report = this.GetUserReport(userid);
            string jsonString = JsonSerializer.Serialize(report);
            return _s3Query.PutReport(jsonString, userid).Result;
        }

        // Pulls a saved report from S3
        public string GetSavedUserReport(string userid)
        {
            var report = _s3Query.GetReport(userid);
            return report.Result;
        }
    }
}