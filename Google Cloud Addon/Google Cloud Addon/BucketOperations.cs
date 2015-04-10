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
            catch (AggregateException e)
            {
                throw new AggregateException(e);
            }

    }

        internal void AddBucket()
        {
            try
            {
                AddBucketTask().Wait();
            }
            catch (AggregateException e)
            {
                throw new AggregateException(e);
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
            catch (AggregateException e)
            {
                throw new AggregateException(e);
            }

        }

        internal void RemoveBucket()
        {
            try
            {
                RemoveBucketTask().Wait();
            }
            catch (AggregateException e)
            {
                throw new AggregateException(e);
            }

        }
    }
}
