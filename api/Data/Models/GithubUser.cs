using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_webapi_db_testcontainers.Data
{
    [Table("github_users")]
    public class GithubUser
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("userid")]
        public string Userid { get; set; }
        [Column("vanity_name")]
        public string VanityName { get; set; }

    }
}