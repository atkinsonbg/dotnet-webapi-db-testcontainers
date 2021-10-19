using dotnet_webapi_db_testcontainers.Commands.AWS;
using dotnet_webapi_db_testcontainers.Commands.GitHub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace dotnet_webapi_db_testcontainers.Controllers
{
    [Route("githubusers")]
    public class DefaultController : ControllerBase
    {
        private readonly ILogger<DefaultController> _logger;
        private IGitHubLogic _query;
        private IS3Logic _s3Query;

        public DefaultController(ILogger<DefaultController> logger, IGitHubLogic query, IS3Logic s3Query)
        {
            _logger = logger;
            _query = query;
            _s3Query = s3Query;
        }

        // Get all users from the database (Database call)
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var t = _query.GetAllUsers();
            return Ok(t);
        }

        // Get a specific user from the database (Database call)
        [HttpGet("{userid}")]
        public IActionResult GetUser(string userid)
        {
            var t = _query.GetUser(userid);
            return Ok(t);
        }

        // Get GitHub details about a specific user (3rd Party API call)
        [HttpGet("{name}/report")]
        public IActionResult GetUserReport(string name)
        {
            var t = _query.GetUserReport(name);
            return Ok(t);
        }

        // Save GitHub details about a specific user in a report in S3 (3rd Party API and AWS calls)
        [HttpPost("{name}/report/save")]
        public IActionResult SaveUserReport(string name)
        {
            var b = _query.SaveUserReport(name);
            return Ok(b);
        }

        // Get a saved report from S3 (AWS call)
        [HttpGet("{name}/report/retrieve")]
        public IActionResult GetSavedUserReport(string name)
        {
            var b = _query.GetSavedUserReport(name);
            return Ok(b);
        }
    }
}
