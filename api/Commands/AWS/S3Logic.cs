using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace dotnet_webapi_db_testcontainers.Commands.AWS
{
    public class S3Logic : IS3Logic
    {
        private IAmazonS3 _s3Client;

        public S3Logic(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        // Saves a report to an S3 bucket
        public async Task<bool> PutReport(string json, string filename)
        {
            try
            {
                PutObjectRequest request = new PutObjectRequest()
                {
                    BucketName = "testbucket",
                    Key = filename,
                    InputStream = new MemoryStream(Encoding.UTF8.GetBytes(json))
                };

                PutObjectResponse response = await _s3Client.PutObjectAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return false;
            }
        }

        // Pulls a report from an S3 bucket
        public async Task<string> GetReport(string filename)
        {
            try
            {
                GetObjectRequest request = new GetObjectRequest()
                {
                    BucketName = "testbucket",
                    Key = filename
                };

                GetObjectResponse response = await _s3Client.GetObjectAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (var stream = response.ResponseStream)
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                return null;
            }
        }
    }
}