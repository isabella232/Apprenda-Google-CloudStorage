using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Services;
using Google.Apis.Storage.v1;
using Google.Apis.Storage.v1.Data;
using Google.Apis.Util.Store;


namespace Apprenda.SaaSGrid.Addons.Google.Storage
{
    internal class BucketOperations
    {
        internal string Message { get; private set; }
        private string ProjectId { get; set; }
        private string ServiceAccountEmail { get; set; }
        private string BucketName { get; set; }
        private string DefaultBucketName { get; set; }
        private string CertificateFile { get; set; }

        internal BucketOperations(AddonManifest manifest, GoogleStorageDeveloperOptions developerOptions)
        {
            try
            {
            BucketName = developerOptions.BucketName;
            // now, let's grab the properties from the manifest.
            var manifestprops = manifest.GetProperties().ToDictionary(x => x.Key, x => x.Value);
            // get the manifest properties and set them.
            ProjectId = manifestprops["ProjectID"];
            ServiceAccountEmail = manifestprops["Email"];
            CertificateFile = manifestprops["CertFile"];
            DefaultBucketName = manifestprops["DefaultBucketName"];
            }
            catch (Exception e)
            {
                throw new ArgumentException("Argument syntax is incorrect - " + e.Message);
            }
        }

        private async Task AddBucketTask()
        {
            string newBucketName = BucketName;
            if(newBucketName == null)
            {
                newBucketName = DefaultBucketName;
            }
            
            //credentials for certificate-based service accounts
            var certificate = new X509Certificate2(CertificateFile, "notasecret", X509KeyStorageFlags.Exportable);
            ServiceAccountCredential credential = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(ServiceAccountEmail)
               {
                   Scopes = new[] { StorageService.Scope.DevstorageFullControl }
               }.FromCertificate(certificate));

            var service = new StorageService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Apprenda Addon",
            });
            var newBucket = new Bucket
            {
                Name = newBucketName
            };

            try
            {
                var newBucketQuery = await service.Buckets.Insert(newBucket, ProjectId).ExecuteAsync();
            }
            catch (AggregateException ex)
            {
                foreach (var err in ex.InnerExceptions)
                {
                    Message += "Error adding bucket:" + err.Message + " \n";
                }
            }
    }

        internal void AddBucket()
        {
            Message += "Attempting to add a bucket \n";
            try
            {
                AddBucketTask().Wait();
                Message += "Successfully added bucket \n";
            }
            catch (AggregateException ex)
            {
                Message += "Failed to add bucket \n";
                foreach (var err in ex.InnerExceptions)
                {
                    Message += "ERROR: " + err.Message + "\n";
                }
            }
        }

        private async Task RemoveBucketTask()
        {
            string newBucketName = BucketName;
            if (newBucketName == null)
            {
                newBucketName = DefaultBucketName;
            }
            //credentials for certificate-based service accounts
            var certificate = new X509Certificate2(CertificateFile, "notasecret", X509KeyStorageFlags.Exportable);
            ServiceAccountCredential credential = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(ServiceAccountEmail)
               {
                   Scopes = new[] { StorageService.Scope.DevstorageFullControl }
               }.FromCertificate(certificate));

            var service = new StorageService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Apprenda Addon",
            });

            try
            {
                var removeBucketQuery = await service.Buckets.Delete(newBucketName).ExecuteAsync();
            }
            catch (AggregateException ex)
            {
                foreach (var err in ex.InnerExceptions)
                {
                    Message += "Error deleting bucket:" + err.Message + " \n";

                }
            }
        }

        internal void RemoveBucket()
        {
            Message += "Attempting to remove a bucket \n";
            try
            {
                RemoveBucketTask().Wait();
                Message += "Successfully removed bucket \n";
            }
            catch (AggregateException ex)
            {
                Message += "Failed to remove bucket \n";
                foreach (var err in ex.InnerExceptions)
                {
                    Message += "ERROR: " + err.Message + "\n";
                }
            }
        }
    }
}
