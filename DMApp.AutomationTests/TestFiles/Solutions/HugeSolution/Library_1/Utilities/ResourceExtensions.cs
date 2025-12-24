namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources.MetaData;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.SRM.Capabilities;
	using Skyline.DataMiner.Net.Time;

	public static class ResourceExtensions
	{
        public static IEnumerable<IResourceMetaData> GetMetaData(this FunctionResource resource)
		{
            var metadata = new List<IResourceMetaData>();

            if (IpDecoderUrlAndKeyMetaData.TryConstructFromResource(resource, out var ipDecoderMetaData))
			{
                metadata.Add(ipDecoderMetaData);
			}

            if (IpAndPortMetaData.TryConstructFromResource(resource, out var ipAndPortMetaData))
            {
                metadata.Add(ipAndPortMetaData);
            }

            return metadata;
		}

        public static bool HasPrioritizedResourcesDefined(this Resource resource)
		{
            return !string.IsNullOrWhiteSpace(resource.GetResourcePropertyStringValue(ResourcePropertyNames.PrioritizedDestinationResource))
                || !string.IsNullOrWhiteSpace(resource.GetResourcePropertyStringValue(ResourcePropertyNames.PrioritizedMessiLiveRecordingResource))
                || !string.IsNullOrWhiteSpace(resource.GetResourcePropertyStringValue(ResourcePropertyNames.PrioritizedTieLine));
        }

        public static string GetDisplayName(this Resource resource, Guid functionId)
		{
            bool functionIsMatrix = FunctionGuids.IsMatrixFunction(functionId);
            bool functionIsAntenna = functionId == FunctionGuids.Antenna;
            return functionIsMatrix || functionIsAntenna ? resource.Name : resource.Name.Split('.')[0];
		}

        public static string GetDisplayName(string resourceName, Guid functionId)
        {
            bool functionIsMatrix = FunctionGuids.IsMatrixFunction(functionId);
            bool functionIsAntenna = functionId == FunctionGuids.Antenna;

            string displayName = functionIsMatrix || functionIsAntenna ? resourceName : resourceName.Split('.')[0]; ;

            return displayName;
        }

        public static bool IsResourceFromSameElementAs(this FunctionResource resource, FunctionResource otherResource)
        {
            if (resource == null || otherResource == null) return false;

            return resource.MainDVEDmaID == otherResource.MainDVEDmaID && resource.MainDVEElementID == otherResource.MainDVEElementID;
        }

		public static bool MatchesProfileParameters(this Resource resource, Helpers helpers, List<ProfileParameter> capabilities, TimeRangeUtc timeRange = null)
		{
			if (capabilities == null) throw new ArgumentNullException(nameof(capabilities));

			var capabilitiesToEvaluate = capabilities.Where(c => !string.IsNullOrWhiteSpace(c.StringValue)).ToList();
			if (!capabilitiesToEvaluate.Any()) return true;

			foreach (var capability in capabilitiesToEvaluate)
			{
				if (!resource.MatchesProfileParameter(helpers, capability, timeRange)) return false;
			}

			return true;
		}

		public static bool MatchesProfileParameter(this Resource resource, Helpers helpers, ProfileParameter profileParameter, TimeRangeUtc timeRange = null)
		{
			if (resource == null) throw new ArgumentNullException(nameof(resource));
			if (profileParameter == null) throw new ArgumentNullException(nameof(profileParameter));

			if (string.IsNullOrWhiteSpace(profileParameter.StringValue) || resource.Capabilities == null) return false;

			var resourceCapability = resource.Capabilities.FirstOrDefault(c => c.CapabilityProfileID == profileParameter.Id);
			if (resourceCapability == null) return false;

			if (resourceCapability.Value.ProvidedString == profileParameter.StringValue) return true;
            if (resourceCapability.Value.Discreets?.Contains(profileParameter.StringValue) ?? false) return true;
            if (double.TryParse(profileParameter.StringValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var capabilityValue) && resourceCapability.Value.MinRange <= capabilityValue && capabilityValue <= resourceCapability.Value.MaxRange) return true;

            if (resourceCapability.IsTimeDynamic)
            {
				helpers.Log(nameof(ResourceExtensions), nameof(MatchesProfileParameter), $"WARNING: Using this method to match a time dynamic profile parameter in a loop might cause serious performance issues.");

                // verify that the resource is available for the given timerange with the expected capability value

                var eligibleResourceResult = DataMinerInterface.ResourceManager.GetEligibleResources(helpers,
	                new EligibleResourceContext
                {
                    ContextId = Guid.NewGuid(),
                    TimeRange = timeRange,
                    ResourceFilter = ResourceExposers.ID.Equal(resource.ID),
                    RequiredCapabilities = new List<ResourceCapabilityUsage>
                    {
                        new ResourceCapabilityUsage
                        {
                            CapabilityProfileID = profileParameter.Id,
                            RequiredString = profileParameter.StringValue
                        }
                    }
                });

                if (!eligibleResourceResult.EligibleResources.Contains(resource)) return false;

                // verify that this resource is already in use by another booking during this time range
                // if that is the case then the capability matches (and is not empty)
                // we should give priority to this resource so update it's priority to 0 (highest)
                var resourceUsageDetails = eligibleResourceResult.UsageDetails.FirstOrDefault(d => d.ResourceId == resource.ID) ?? throw new NotFoundException($"Unable to find resource usage details for resource {resource.ID}"); ;
                var capabilityUsageDetails = resourceUsageDetails.CapabilityUsageDetails.FirstOrDefault(c => c.CapabilityParameterId == resourceCapability.CapabilityProfileID) ?? throw new NotFoundException($"Unable to find capability usage details for capability {resourceCapability.CapabilityProfileID}");
                if (capabilityUsageDetails.HasExistingBookings.HasValue && capabilityUsageDetails.HasExistingBookings.Value) UpdatePriority(resource, 0);

                return true;
            }

            return false;
        }

		public static bool RequiresSpecificTieLine(this Resource resource)
		{
			return GetResourcePropertyBooleanValue(resource, ResourcePropertyNames.RequiresSpecificTieLine);
		}

        public static bool RequiresFixedNewsRecordingService(this Resource resource)
        {
            return GetResourcePropertyBooleanValue(resource, ResourcePropertyNames.RequiresFixedNewsRecordingService);
        }

        public static bool IsManualResource(this Resource resource)
        {
            return GetResourcePropertyBooleanValue(resource, ResourcePropertyNames.IsManualResourcePropertyName);
        }

        public static void UpdatePriority(this Resource resource, int priority)
        {
            if (resource.Properties == null)
            {
                resource.Properties = new List<ResourceManagerProperty>
                {
                    new ResourceManagerProperty(ResourcePropertyNames.Priority, priority.ToString())
                };

                return;
            }

            var priorityProperty = resource.Properties.FirstOrDefault(p => p.Name == ResourcePropertyNames.Priority);
            if (priorityProperty == null)
            {
                resource.Properties.Add(new ResourceManagerProperty(ResourcePropertyNames.Priority, priority.ToString()));
                return;
            }

            priorityProperty.Value = priority.ToString();
        }

		public static int GetPriority(this Resource resource)
		{
			if (resource.Properties == null) return 99;

			string priority = GetResourcePropertyStringValue(resource, ResourcePropertyNames.Priority);

			return string.IsNullOrWhiteSpace(priority) ? 99 : Convert.ToInt32(priority);
		}

        public static bool GetResourcePropertyBooleanValue(this Resource resource, string propertyName)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            var property = resource.Properties.FirstOrDefault(p => p.Name == propertyName);
            if (property == null) return false;

            return property.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase);
        }

        public static string GetResourcePropertyStringValue(this Resource resource, string propertyName)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            var property = resource.Properties.FirstOrDefault(p => p.Name == propertyName);
            if (property == null) return string.Empty;

            return property.Value;
        }

        public static bool HasCapabilityDiscreetValue(this Resource resource, Guid capabilityId, string capabilityValue)
        {
	        if (resource == null) throw new ArgumentNullException(nameof(resource));
	        if (capabilityValue == null) throw new ArgumentNullException(nameof(capabilityValue));

	        var capability = resource.Capabilities.FirstOrDefault(c => c.CapabilityProfileID == capabilityId);
	        if (capability == null) return false;

	        return capability.Value.Discreets.Contains(capabilityValue);
        }
	}
}