using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace S3Uploader.Models
{
    public class UploaderRequest
    {
        [JsonPropertyName("bucketName")]
        public string? BucketName { get; set; }

        [JsonPropertyName("keyName")]
        public string? KeyName { get; set; }

        [JsonPropertyName("contentBody")]
        public string? ContentBody { get; set; }

        public bool IsValid() => !string.IsNullOrWhiteSpace(BucketName) 
            && !string.IsNullOrWhiteSpace(KeyName) 
            && !string.IsNullOrWhiteSpace(ContentBody);
    }
}
