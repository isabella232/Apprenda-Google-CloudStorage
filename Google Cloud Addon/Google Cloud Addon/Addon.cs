using System;
using Apprenda.Services.Logging;

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
            var testProgress = "";
            //var provisionResult = new ProvisionAddOnResult("") { IsSuccess = false };
            var testResult = new OperationResult { IsSuccess = false };

            var manifest = request.Manifest;
            
            var developerParameters = request.DeveloperParameters;

            try
            {            
                //var op = new BucketOperations(projectId, email, accessKey, secretAccessKey);
                var op = new BucketOperations(manifest, developerParameters);
                op.AddBucket();
                testProgress += op.Message;
                testResult.IsSuccess = true;
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
