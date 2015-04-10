using System;
using Apprenda.Services.Logging;
using Google.Apis;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Services;
using Google.Apis.Storage.v1;
using Google.Apis.Storage.v1.Data;
using Google.Apis.Util.Store;

namespace Apprenda.SaaSGrid.Addons.Google.Storage
{
    public class GoogleCloudAddon : AddonBase
    {
        private static readonly ILogger Log = LogManager.Instance().GetLogger(typeof(GoogleCloudAddon));

        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            string connectionData = request.ConnectionData;
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            AddonManifest manifest = request.Manifest;
            var developerParameters = request.DeveloperParameters;
            var developerOptions = GoogleStorageDeveloperOptions.Parse(developerParameters);

            try
            {
                var conInfo = ConnectionInfo.Parse(connectionData);
                developerOptions.BucketName = conInfo.BucketName;
                var op = new BucketOperations(manifest, developerOptions);
                op.RemoveBucket();
                deprovisionResult.IsSuccess = true;
                deprovisionResult.EndUserMessage = "Successfully deleted bucket: " + conInfo.BucketName;
            }
            catch (AggregateException err)
            {
                foreach (var e in err.InnerExceptions)
                {
                    deprovisionResult.EndUserMessage = e.Message;
                    deprovisionResult.IsSuccess = false;
                }
            }
            return deprovisionResult;
        }

        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            var provisionResult = new ProvisionAddOnResult("") { IsSuccess = false };
            var manifest = request.Manifest;
            var developerParameters = request.DeveloperParameters;
            var developerOptions = GoogleStorageDeveloperOptions.Parse(developerParameters);

            try
            {
                //add a bucket
                var op = new BucketOperations(manifest, developerOptions);
                op.AddBucket();
                provisionResult.IsSuccess = true;
                provisionResult.ConnectionData = "BucketName=" + developerOptions.BucketName;
                provisionResult.EndUserMessage = "Successfully added bucket " + developerOptions.BucketName + "\n";
            }
            catch (AggregateException err)
            {
                foreach (var e in err.InnerExceptions)
                {
                    provisionResult.EndUserMessage = e.Message;
                    provisionResult.IsSuccess = false;
                }
            }
            return provisionResult;
        }

        public override OperationResult Test(AddonTestRequest request)
        {
            var testProgress = "";
            var testResult = new OperationResult { IsSuccess = false };

            var manifest = request.Manifest;
            
            var developerParameters = request.DeveloperParameters;
            var developerOptions = GoogleStorageDeveloperOptions.Parse(developerParameters);
            testProgress += "Attempting to add a bucket...";
            try
            {
                //add a bucket
                var op = new BucketOperations(manifest, developerOptions);
                op.AddBucket();
                testProgress += "Successfully added a bucket.\n";

                testProgress += "Attempting to remove a bucket...";
                try
                {
                    //remove a bucket
                    op.RemoveBucket();
                    testProgress += "Sucessfully removed a bucket.\n";
                    testResult.IsSuccess = true;
                }
                catch (AggregateException err)
                {
                    foreach (var e in err.InnerExceptions)
                    {
                        Log.Error("Error occurred during test of Google Cloud Addon", e);
                        testProgress += "EXCEPTION: " + e + "\n";
                        testProgress += "Failed to remove bucket \n";
                    }
                }
            }
            catch (AggregateException err)
            {
                foreach (var e in err.InnerExceptions)
                {
                    Log.Error("Error occurred during test of Google Cloud Addon", e);
                    testProgress += "EXCEPTION: " + e + "\n";
                    testProgress += "Failed to add bucket \n";
                }
            }
            testResult.EndUserMessage = testProgress;
            return testResult;
        }
    }
}
