using System.Collections.Generic;
using dotnet_webapi_db_testcontainers.Data;

namespace dotnet_webapi_db_testcontainers.Commands.GitHub
{
    public interface IGitHubLogic
    {
        List<GithubUser> GetAllUsers();
        GithubUser GetUser(string userid);
        GitHubUserReport GetUserReport(string userid);
        bool SaveUserReport(string userid);
        string GetSavedUserReport(string userid);
    }
}