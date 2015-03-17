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
    public class BucketOperations
    {
        public string message, projectID, email, bucketName, clientID, clientSecret;
        public BucketOperations(string _projectID, string _email, string _clientID, string _clientSecret)
        {
            projectID = _projectID;
            email = _email;
            clientID = _clientID;
            clientSecret = _clientSecret;
            message = "";
        }
        private async Task Run(string newBucketName)
        {
            /*var clientSecrets = new ClientSecrets();
            clientSecrets.ClientId = client;
            clientSecrets.ClientSecret = secret;
            */
            message += "Starting run... \n";

            var clientSecrets = new ClientSecrets();
            clientSecrets.ClientId = clientID;
            clientSecrets.ClientSecret = clientSecret;
            var scopes = new[] { @"https://www.googleapis.com/auth/devstorage.full_control" };

            var cts = new CancellationTokenSource();
            var userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets, scopes, email, cts.Token);

            await userCredential.RefreshTokenAsync(cts.Token);

            var service = new Google.Apis.Storage.v1.StorageService();

            var newBucket = new Google.Apis.Storage.v1.Data.Bucket()
            {
                Name = newBucketName
            };

            var newBucketQuery = service.Buckets.Insert(newBucket, projectID);
            newBucketQuery.OauthToken = userCredential.Token.AccessToken;

            try
            {
                newBucketQuery.Execute();
                message += "Added new bucket " + newBucketName + " to project " + projectID + "\n";

            }
            catch (AggregateException ex)
            {
                foreach (var err in ex.InnerExceptions)
                {
                    message += "Error adding bucket:" + err.Message + " \n";
       
                }
            }
            message += "Finished run \n";
        }
        public void addBucket(string bucketName)
        {
            message += "Attempting to add a bucket \n";

            //verify parameters passed in correctly
            message += "Project ID: " + projectID + "\n";
            message += "Email: " + email + "\n";
            message += "Client ID: " + clientID + "\n";
            message += "Client Secret: " + clientSecret + "\n";
            try
            {
                var newop = new BucketOperations(projectID, email, clientID, clientSecret);
                newop.Run(bucketName).Wait();
                message += newop.message;
               // new BucketOperations(projectID, email, clientID, clientSecret).Run(bucketName).Wait();

                message += "Successfully added bucket \n";
            }
            catch (AggregateException ex)
            {
                message += "Failed to add bucket \n";
                foreach (var err in ex.InnerExceptions)
                {
                    message += "ERROR: " + err.Message + "\n";
                }
            }
        }
    }
    public class Google_Cloud_Addon : AddonBase
    {
        private static readonly ILogger log1 = LogManager.Instance().GetLogger(typeof(Google_Cloud_Addon));

        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            throw new NotImplementedException();
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

    */
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
            try
            {            
                var op = new BucketOperations(projectID, email, accessKey, secretAccessKey);
                op.addBucket(bucketName);
                testProgress += op.message;
                testResult.IsSuccess = true;
            }
            catch(Exception e)
            {
                testProgress += "EXCEPTION: " + e + "\n";
                testProgress += "Failed to add bucket \n";
                testResult.IsSuccess = false;
            }
            testResult.EndUserMessage = testProgress;
            return testResult;
        }
    }
}
