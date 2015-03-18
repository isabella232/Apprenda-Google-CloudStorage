using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.Google.Storage
{
    internal class GoogleStorageDeveloperOptions
    {
        internal string ProjectId { get; private set; }
        internal string BucketName { get; private set; }

        // parses the developer options into a usable model - these are the ones that come from the web form.
        internal static GoogleStorageDeveloperOptions Parse(IEnumerable<AddonParameter> parameters)
        {
            // TODO - bring in ProjectID and BucketName as options
            // we have a neat way of parsing in the developer options

            var options = new GoogleStorageDeveloperOptions();

            foreach (var parameter in parameters)
            {
                MapToOption(options, parameter.Key.ToLowerInvariant(), parameter.Value);
            }
            return options;
        }

        private static void MapToOption(GoogleStorageDeveloperOptions options, string key, string value)
        {
            if (key.Equals("projectid"))
            {
                options.ProjectId = value;
                return;
            }
            if (key.Equals("bucketname"))
            {
                options.BucketName = value;
                return;
            }
            throw new ArgumentException(string.Format("Developer parameter {0} is either not readable, or not supported at this time.", key));
        }
    }
}
