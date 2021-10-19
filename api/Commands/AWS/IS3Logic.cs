using System.Threading.Tasks;

namespace dotnet_webapi_db_testcontainers.Commands.AWS
{
    public interface IS3Logic
    {
        Task<bool> PutReport(string json, string filename);
        Task<string> GetReport(string filename);
    }
}