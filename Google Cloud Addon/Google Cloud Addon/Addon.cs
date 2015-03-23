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
            throw new NotImplementedException();
        }

        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            throw new NotImplementedException();
        }

        public override OperationResult Test(AddonTestRequest request)
        {
            var testProgress = "";
            var testResult = new OperationResult { IsSuccess = false };

            var manifest = request.Manifest;
            
            var developerParameters = request.DeveloperParameters;
            testProgress += "Attempting to add a bucket...\n";
            try
            {
                //add a bucket
                var op = new BucketOperations(manifest, developerParameters);
                op.AddBucket();
                testProgress += "Successfully added a bucket.\n";

                testProgress += "Attempting to remove a bucket...\n";
                try
                {
                    //remove a bucket
                    op.RemoveBucket();
                    testProgress += "Sucessfully removed a bucket.\n";
                    testResult.IsSuccess = true;
                }
                catch(Exception e)
                {
                    Log.Error("Error occurred during test of Google Cloud Addon", e);
                    testProgress += "EXCEPTION: " + e + "\n";
                    testProgress += "Failed to remove bucket \n";
                    testResult.IsSuccess = false;
                }
            }
            catch(Exception e)
            {
                Log.Error("Error occurred during test of Google Cloud Addon", e);
                testProgress += "EXCEPTION: " + e + "\n";
                testProgress += "Failed to add bucket \n";
                testResult.IsSuccess = false;

            }
            testResult.EndUserMessage = testProgress;
            return testResult;
        }
    }
}
