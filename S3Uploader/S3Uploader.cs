using System.Net;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using S3Uploader.Models;

namespace S3Uploader
{
    public class S3Uploader
    {
        private readonly ILogger _logger;
        private readonly IAmazonS3 _client;
        private readonly string _key;
        private readonly string _secret;
        private readonly string _region;

        public S3Uploader(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<S3Uploader>();
            _key = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? throw new ArgumentNullException("AWS_ACCESS_KEY_ID");
            _secret = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? throw new ArgumentNullException("AWS_SECRET_ACCESS_KEY");
            _region = Environment.GetEnvironmentVariable("AWS_REGION") ?? throw new ArgumentNullException("AWS_REGION");
            _client = new AmazonS3Client(_key, _secret, RegionEndpoint.GetBySystemName(_region));
        }

        [Function("S3Uploader")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("S3Uploader function processed a new request.");
            
            var request = await req.ReadFromJsonAsync<UploaderRequest>();
            if (request == null || !request.IsValid())
            {
                var error = "Invalid request body";
                _logger.LogError(error);
                var httpResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await httpResponse.WriteStringAsync(error);
                return httpResponse;
            }

            try
            {
                _logger.LogInformation("Creating object...");

                var putRequest = new PutObjectRequest
                {
                    BucketName = request.BucketName,
                    Key = request.KeyName,
                    ContentBody = request.ContentBody
                };

                PutObjectResponse response = await _client.PutObjectAsync(putRequest);

                _logger.LogInformation("Object created successfully!");
            }
            catch (AmazonS3Exception e)
            {
                var error = $"Error encountered. Message:'{e.Message}' when writing an object";
                _logger.LogError(error);
                var httpResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await httpResponse.WriteStringAsync(error);
                return httpResponse;
            }
            catch (Exception e)
            {

                var error = $"Unknown encountered on server. Message:{e.Message} when writing an object";
                _logger.LogError(error);
                var httpResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await httpResponse.WriteStringAsync(error);
                return httpResponse;
            }

            return req.CreateResponse(HttpStatusCode.Created);
        }
    }
}
