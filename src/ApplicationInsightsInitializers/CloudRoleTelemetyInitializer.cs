// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace ApplicationInsightsInitializers
{
    // This optional class customizes AppInsights behavior (data reported, etc.)
    public class CloudRoleTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string _roleName;

        public CloudRoleTelemetryInitializer(string roleName)
        {
            _roleName = roleName;
        }

        public void Initialize(ITelemetry telemetry)
        {
            // Set the name for this service's role (which will be used to distinguish this component from other services in the application)
            telemetry.Context.Cloud.RoleName = _roleName;

            // Note that telemetry.Context.Cloud.RoleInstance refers to a particular instance of a microservice
            // and will autoamtically populate with the name of the machine/container running the service.
        }

        public static void SetRoleName(string roleName)
        {
            TelemetryConfiguration.Active.TelemetryInitializers.Add(new CloudRoleTelemetryInitializer(roleName));
        }
    }
}
