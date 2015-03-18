using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1;
using Google.Apis.Storage.v1.Data;

namespace Apprenda.SaaSGrid.Addons.Google.Storage
{
    internal class BucketOperations
    {
        internal string Message { get; private set; }
        private string ProjectId { get; set; }
        private string Email { get; set; }
        private string BucketName { get; set; }
        private string ClientId { get; set; }
        private string ClientSecret { get; set; }

        //[Obsolete]
        //public BucketOperations(string _projectID, string _email, string _clientID, string _clientSecret)
        //{
        //    ProjectId = _projectID;
        //    Email = _email;
        //    ClientId = _clientID;
        //    ClientSecret = _clientSecret;
        //    Message = "";
        //}

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
            ClientId = manifestprops["GoogleClientKey"];
            ClientSecret = manifestprops["GoogleSecretKey"];
            Email = manifestprops["email"];
            ProjectId = manifestprops["projectID"];
            }
            catch (Exception e)
            {
                throw new ArgumentException("Argument syntax is incorrect - " + e.Message);
            }
        }

        private async Task AddBucketTask()
        {
            /*var clientSecrets = new ClientSecrets();
            clientSecrets.ClientId = client;
            clientSecrets.ClientSecret = secret;
            */
            Message += "Starting run... \n";

            var clientSecrets = new ClientSecrets();
            clientSecrets.ClientId = ClientId;
            clientSecrets.ClientSecret = ClientSecret;
            var scopes = new[] { @"https://www.googleapis.com/auth/devstorage.full_control" };

            var cts = new CancellationTokenSource();
            var userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets, scopes, Email, cts.Token);

            await userCredential.RefreshTokenAsync(cts.Token);

            var service = new StorageService();

            var newBucket = new Bucket
            {
                Name = BucketName
            };

            var newBucketQuery = service.Buckets.Insert(newBucket, ProjectId);
            newBucketQuery.OauthToken = userCredential.Token.AccessToken;

            try
            {
                newBucketQuery.Execute();
                Message += "Added new bucket " + BucketName + " to project " + ProjectId + "\n";

            }
            catch (AggregateException ex)
            {
                foreach (var err in ex.InnerExceptions)
                {
                    Message += "Error adding bucket:" + err.Message + " \n";

                }
            }
            Message += "Finished run \n";
        }

        internal void AddBucket()
        {
            Message += "Attempting to add a bucket \n";

            //verify parameters passed in correctly
            Message += "Project ID: " + ProjectId + "\n";
            Message += "Email: " + Email + "\n";
            Message += "Client ID: " + ClientId + "\n";
            Message += "Client Secret: " + ClientSecret + "\n";
            try
            {
                // *********************************************************************
                // this was the killer right here. when we added the bucket, it was regenerating the
                // class without the bucketName :)
                // var newop = new BucketOperations(projectID, email, clientID, clientSecret);
                //
                // we can just reference the class property in running the Task, no need to pass the bucketName parameter around
                //
                // *********************************************************************
                // this is so smart. nice!
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
            // TODO
            throw new NotImplementedException();
        }

        internal void RemoveBucket()
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
