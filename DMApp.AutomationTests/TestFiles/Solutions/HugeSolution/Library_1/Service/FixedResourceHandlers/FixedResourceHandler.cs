namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.FixedResourceHandlers
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Net.Messages;

	public abstract class FixedResourceHandler<T> where T : IFixedResourceConfiguration
	{
		private readonly Helpers helpers;

		protected FixedResourceHandler(Helpers helpers)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
		}

		/// <summary>
		/// Checks if the service matches any of the linked resource configurations.
		/// If it does, the resource defined in the matching resource configuration will be assigned to the function defined in the resource configuration.
		/// </summary>
		/// <param name="service">Service to check and to assign the resource to.</param>
		public void AssignFixedResource(Service service)
		{
			if (ResourceConfigurations == null || !ResourceConfigurations.Any())
			{
				helpers.Log(nameof(FixedResourceHandler<T>), nameof(AssignFixedResource), "No configurations defined");
				return;
			}

			if (service?.Definition?.VirtualPlatform != VirtualPlatform)
			{
				helpers.Log(nameof(FixedResourceHandler<T>), nameof(AssignFixedResource), $"Virtual Platform {VirtualPlatform} does not match the Virtual Platform of the service {service?.Definition?.VirtualPlatform}");
				return;
			}

			foreach (T configuration in ResourceConfigurations)
			{
				var function = service.Functions.FirstOrDefault(x => x.Id.Equals(configuration.FunctionId));
				if (function == null)
				{
					helpers.Log(nameof(FixedResourceHandler<T>), nameof(AssignFixedResource), $"The service does not a function with id: {configuration.FunctionId} on which the fixed resource should be assigned");
					continue;
				}

				if (!ServiceMatchesConfiguration(service, configuration))
				{
					helpers.Log(nameof(FixedResourceHandler<T>), nameof(AssignFixedResource), "The service doesn't match the configuration.");
					continue;
				}

				FunctionResource resource = (FunctionResource)helpers.ResourceManager.GetResources(ResourceExposers.Name.Equal(configuration.ResourceName)).FirstOrDefault();
				if (resource == null)
				{
					helpers.Log(nameof(FixedResourceHandler<T>), nameof(AssignFixedResource), $"Unable to find a resource with name {configuration.ResourceName}, the resource on the function was not updated");
				}
				else
				{
					helpers.Log(nameof(FixedResourceHandler<T>), nameof(AssignFixedResource), $"Service matches configuration, assigning resource {configuration.ResourceName} to function {function.Name} of service {service.Name}");
					function.Resource = resource;
					function.EnforceSelectedResource = true;
					return;
				}
			}
		}

		private bool ServiceMatchesConfiguration(Service service, T configuration)
		{
			IReadOnlyDictionary<Guid, object> profileParameterValues = GetProfileParameterValuesFromConfiguration(configuration);
			IReadOnlyDictionary<Guid, string> functionResourceNames = GetFunctionResourceFromConfiguration(configuration);

			foreach (var function in service.Functions)
			{
				string resourceName;
				if (functionResourceNames.TryGetValue(function.Id, out resourceName) && function.Resource?.Name != resourceName)
				{
					helpers?.Log(nameof(FixedResourceHandler<T>), nameof(AssignFixedResource), $"Service doesn't match configuration|Function {function.Name} has resource {function.Resource?.Name} assigned to it instead of {resourceName}");
					return false;
				}

				foreach (var profileParameter in function.Parameters)
				{
					object profileParameterValue;
					if (profileParameterValues.TryGetValue(profileParameter.Id, out profileParameterValue) && !profileParameterValue.Equals(profileParameter.Value))
					{
						helpers?.Log(nameof(FixedResourceHandler<T>), nameof(AssignFixedResource), $"Service doesn't match configuration|Profile parameter {profileParameter.Name} has value {profileParameter.Value} assigned to it instead of {profileParameterValue}");
						return false;
					}
				}
			}

			return true;
		}

		private static IReadOnlyDictionary<Guid, object> GetProfileParameterValuesFromConfiguration(T configuration)
		{
			Dictionary<Guid, object> profileParameterValues = new Dictionary<Guid, object>();
			foreach (var property in typeof(T).GetProperties())
			{
				object[] attributes = property.GetCustomAttributes(false);
				var profileParameterValueAttribute = attributes.FirstOrDefault(x => x is MatchingProfileParameterAttribute);
				if (profileParameterValueAttribute == null) continue;

				MatchingProfileParameterAttribute attribute = (MatchingProfileParameterAttribute)profileParameterValueAttribute;
				object profileParameterValue = property.GetValue(configuration);
				Guid profileParameterGuid = attribute.MatchingProfileParameterId;

				profileParameterValues.Add(profileParameterGuid, profileParameterValue);
			}

			return profileParameterValues;
		}

		private static IReadOnlyDictionary<Guid, string> GetFunctionResourceFromConfiguration(T configuration)
		{
			Dictionary<Guid, string> functionResourceNames = new Dictionary<Guid, string>();
			foreach (var property in typeof(T).GetProperties())
			{
				object[] attributes = property.GetCustomAttributes(false);
				var functionResourceAttribute = attributes.FirstOrDefault(x => x is MatchingFunctionResourceAttribute);
				if (functionResourceAttribute == null) continue;

				MatchingFunctionResourceAttribute attribute = (MatchingFunctionResourceAttribute)functionResourceAttribute;
				string resourceName = (string)property.GetValue(configuration);
				Guid functionGuid = attribute.MatchingFunctionId;

				functionResourceNames.Add(functionGuid, resourceName);
			}

			return functionResourceNames;
		}

		protected abstract List<T> ResourceConfigurations { get; }

		protected abstract ServiceDefinition.VirtualPlatform VirtualPlatform { get; }
	}
}
