/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Services;
using Google.Apis.Storage.v1;
using Google.Apis.Storage.v1.Data;
using Google.Apis.Util.Store;
using System.Security.Cryptography.X509Certificates;*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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

        internal BucketOperations(AddonManifest manifest, IEnumerable<AddonParameter> addonParameters)
        {
            try
            {
            // ok two things here. 
            // 1. the manifest properties and developer options should be decoupled. there are things the developer shouldn't have access to (ie. secret keys)
            // so what we will do here is bring in the manifest and parse the manifest properties controlled by devOps
            // 2. the bucketName should come from the developerOptions (and maybe the projectID if not using the default).
            var developerOptions = GoogleStorageDeveloperOptions.Parse(addonParameters);
            // ok and then assign the information to the 
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
