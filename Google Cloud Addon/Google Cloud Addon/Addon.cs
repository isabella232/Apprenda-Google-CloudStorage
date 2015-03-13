using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using Apprenda.SaaSGrid.Addons;
using Apprenda.Services.Logging;

using Google.Apis;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Services;
using Google.Apis.Storage.v1;
using Google.Apis.Storage.v1.Data;
using Google.Apis.Util.Store;

namespace Apprenda.SaaSGrid.Addons.Google_Cloud_Addon
{
    public class Google_Cloud_Addon : AddonBase
    {
        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            throw new NotImplementedException();
        }

        private async Task Run(string projectName, string email, string newbucketname, string client, string secret)
        {
            var clientSecrets = new ClientSecrets();
            clientSecrets.ClientId = client;
            clientSecrets.ClientSecret = secret;
            var scopes = new[] { @"https://www.googleapis.com/auth/devstorage.full_control" };

            var cts = new CancellationTokenSource();
            var userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets, scopes, email, cts.Token);

            await userCredential.RefreshTokenAsync(cts.Token);

            var service = new Google.Apis.Storage.v1.StorageService();
                
            var newBucket = new Google.Apis.Storage.v1.Data.Bucket()
            {
                Name = newbucketname
            };
                
            var newBucketQuery = service.Buckets.Insert(newBucket, projectName);
            newBucketQuery.OauthToken = userCredential.Token.AccessToken;

            try
            {
                newBucketQuery.Execute();
                Console.WriteLine("Added new bucket " + newbucketname + " to project " + projectName);
            }
            catch(AggregateException ex)
            {
                foreach (var err in ex.InnerExceptions)
                {
                    Console.WriteLine("ERROR: " + err.Message); 
                    //409 - bucket already exists
                }
            }
        }
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
           /* var provisionResult = new ProvisionAddOnResult("") { IsSuccess = false };
            var manifest = request.Manifest;
            var developerParameters = request.DeveloperParameters;
         
            foreach (var parameter in developerParameters)
            {
                if ("bucketname".Equals(parameter.Key.ToLowerInvariant()))
                {
                    string bucketName = parameter.Value;
                }
            }

            try
            {
                new Google_Cloud_Addon().Run().Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var err in ex.InnerExceptions)
                {
                    Console.WriteLine("ERROR: " + err.Message);
                }
            }
            Console.ReadKey();*/
            throw new NotImplementedException();
        }

        public override OperationResult Test(AddonTestRequest request)
        {
            string testProgress = "";
            var provisionResult = new ProvisionAddOnResult("") { IsSuccess = false };
            var testResult = new OperationResult { IsSuccess = false };

            var manifest = request.Manifest;
            var developerParameters = request.DeveloperParameters;

            var manifestprops = manifest.GetProperties().ToDictionary(x => x.Key, x => x.Value);
            var accessKey = manifestprops["GoogleClientKey"];
            testProgress += "Access key is " + accessKey + "\n";
            var secretAccessKey = manifestprops["GoogleSecretKey"];
            testProgress += "Secret key is " + secretAccessKey + "\n";
            var email = manifestprops["email"];
            testProgress += "Email is " + email + "\n";
            var projectID = manifestprops["projectID"];
            testProgress += "ProjectID is " + projectID + "\n";
            var bucketName = manifestprops["BucketName"];
            testProgress += "Bucket name is " + bucketName + "\n";

            //test creation
            try
            {
                new Google_Cloud_Addon().Run(projectID, email, bucketName, accessKey, secretAccessKey).Wait();
                testProgress += "Successfully added bucket \n";
            }
            catch
            {
                testProgress += "Failed to add bucket \n";
            }
            testResult.IsSuccess = true;
            testResult.EndUserMessage = testProgress;
            return testResult;
            //throw new NotImplementedException();
        }
    }
}
