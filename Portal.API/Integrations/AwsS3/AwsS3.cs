using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Portal.API.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Integrations.AwsS3
{
    public class AwsS3Integration
    {

        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.SAEast1;
        private static IAmazonS3 s3Client;
        public static IConfiguration _configuration;
        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;

        }
        public static async Task UploadFileAsync(Stream FileStream, string keyName)
        {
            var environmentsBase = new EnvironmentsBase(_configuration);
            string bucketName = environmentsBase.AWSS3_BUCKET_NAME;
            s3Client = new AmazonS3Client(environmentsBase.AWSS3_ACCESS_KEY, environmentsBase.AWSS3_SECRET_KEY, bucketRegion);
            var fileTransferUtility = new TransferUtility(s3Client);
            await fileTransferUtility.UploadAsync(FileStream, bucketName, keyName);
        }
        public static string GetSignedUrl(string key, int minutes = 5)
        {
            var environmentsBase = new EnvironmentsBase(_configuration);
            string bucketName = environmentsBase.AWSS3_BUCKET_NAME;
            s3Client = new AmazonS3Client(environmentsBase.AWSS3_ACCESS_KEY, environmentsBase.AWSS3_SECRET_KEY, bucketRegion);
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Expires = DateTime.Now.AddMinutes(minutes)
            };
            var url = s3Client.GetPreSignedURL(request);
            return url;
        }

    }
}
