namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.ExternalJson
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using PropertyInfo = System.Reflection.PropertyInfo;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;

	public static class ExternalJsonServiceCreator
	{
		public static Service CreateSourceService(Helpers helpers, Source source)
		{
			ArgumentNullCheck.ThrowIfNull(helpers, nameof(helpers));
			ArgumentNullCheck.ThrowIfNull(source, nameof(source));

			var sourceServiceDefinition = helpers.ServiceDefinitionManager.GetServiceDefinition($"{source.Type} RX");

			helpers.Log(nameof(Service), nameof(CreateSourceService), $"Found service def {sourceServiceDefinition.Name} based on Type {source.Type}");

			var sourceService = new DisplayedService(helpers, sourceServiceDefinition);

			var serviceProperties = typeof(Service).GetProperties();
			var serviceProfileParameters = sourceService.Functions.SelectMany(f => f.Parameters).ToList();

			foreach (var jsonSourceProperty in typeof(Source).GetProperties())
			{
				var jsonPropertyValue = jsonSourceProperty.GetValue(source);
				if (jsonSourceProperty.PropertyType == typeof(string))
				{
					// Temporary workaround to avoid that reservation properties don't include any escaped characters.
					string jsonPropertyStringValue = Convert.ToString(jsonPropertyValue);
					jsonPropertyStringValue = Regex.Unescape(jsonPropertyStringValue);
					jsonPropertyStringValue = jsonPropertyStringValue.Clean(allowSiteContent: true);
					jsonPropertyValue = jsonPropertyStringValue;
				}

				var attributes = jsonSourceProperty.GetCustomAttributes(true);

				var matchingFunctionIdAttribute = attributes.SingleOrDefault(a => a is MatchingFunctionAttribute) as MatchingFunctionAttribute;
				var matchingServicePropertyAttribute = attributes.SingleOrDefault(a => a is MatchingServicePropertyAttribute) as MatchingServicePropertyAttribute;
				var containsProfileParametersAttribute = attributes.SingleOrDefault(a => a is ContainsProfileParametersAttribute) as ContainsProfileParametersAttribute;

				helpers.Log(nameof(Service), nameof(CreateSourceService), $"External JSON Service property {jsonSourceProperty.Name} has value '{jsonPropertyValue.ToString()}, Type {jsonSourceProperty.PropertyType.ToString()}' and attribute values:  MatchingFunctionID='{matchingFunctionIdAttribute?.MatchingFunctionId}', MatchingServiceProperty='{matchingServicePropertyAttribute?.MatchingServicePropertyName}', ContainsProfileParameters={containsProfileParametersAttribute != null}");

				bool propertyRepresentsFunction = matchingFunctionIdAttribute != null;
				bool propertyRepresentsServiceProperty = matchingServicePropertyAttribute != null;
				bool propertyContainsProfileParameters = containsProfileParametersAttribute != null;

				if (propertyContainsProfileParameters || propertyRepresentsFunction)
				{
					FillProfileParameters(helpers, serviceProfileParameters, jsonSourceProperty, jsonPropertyValue);
				}

				if (propertyRepresentsFunction)
				{
					var matchingFunctions = sourceService.Functions.Where(f => f.Id == matchingFunctionIdAttribute.MatchingFunctionId).ToList();

					helpers.Log(nameof(Service), nameof(CreateSourceService), $"Found matching functions {string.Join(";", matchingFunctions.Select(f => f.Name))}");

					foreach (var matchingFunction in matchingFunctions)
					{
						TrySetFunctionResource(helpers, matchingFunction, jsonSourceProperty, jsonPropertyValue);
					}
				}

				if (propertyRepresentsServiceProperty)
				{
					var matchingServiceProperty = serviceProperties.FirstOrDefault(p => p.Name == matchingServicePropertyAttribute.MatchingServicePropertyName);

					helpers.Log(nameof(Service), nameof(CreateSourceService), $"Found matching service property '{matchingServiceProperty?.Name}'");

					matchingServiceProperty?.SetValue(sourceService, jsonPropertyValue);
				}
			}

			helpers.Log(nameof(Service), nameof(CreateSourceService), $"Service object properties summary: Start={sourceService.Start.ToString("o")}, End={sourceService.End.ToString("o")}");

			helpers.Log(nameof(Service), nameof(CreateSourceService), $"Service object profile parameters summary: {string.Join(";", sourceService.Functions.SelectMany(f => f.Parameters).Select(p => $"{p.Name}='{p.StringValue}'"))}");

			return sourceService;
		}

		private static void FillProfileParameters(Helpers helpers, List<ProfileParameter> allProfileParameters, PropertyInfo propertyContainingProfileParameters, object propertyContainingProfileParametersValue)
		{
			if (allProfileParameters == null) throw new ArgumentNullException(nameof(allProfileParameters));
			if (propertyContainingProfileParameters == null) throw new ArgumentNullException(nameof(propertyContainingProfileParameters));
			if (propertyContainingProfileParametersValue == null) throw new ArgumentNullException(nameof(propertyContainingProfileParametersValue));

			var jsonProperties = propertyContainingProfileParameters.PropertyType.GetProperties();

			foreach (var jsonProperty in jsonProperties)
			{
				var jsonPropertyValue = jsonProperty.GetValue(propertyContainingProfileParametersValue);
				var attributes = jsonProperty.GetCustomAttributes(true);

				var matchingProfileParamIdAttribute = attributes.SingleOrDefault(a => a is MatchingProfileParameterAttribute) as MatchingProfileParameterAttribute;

				helpers.Log(nameof(Service), nameof(TrySetFunctionResource), $"External JSON property {jsonProperty.Name} has value '{jsonPropertyValue?.ToString()}' and {attributes.Length} attributes with values '{matchingProfileParamIdAttribute?.MatchingProfileParameterId}'");

				bool propertyRepresentsProfileParameter = matchingProfileParamIdAttribute != null;

				if (propertyRepresentsProfileParameter)
				{
					var matchingProfileParameters = allProfileParameters.Where(p => p.Id == matchingProfileParamIdAttribute.MatchingProfileParameterId).ToList();

					foreach (var matchingProfileParameter in matchingProfileParameters)
					{
						matchingProfileParameter.Value = jsonPropertyValue;
					}
				}
			}
		}

		private static void TrySetFunctionResource(Helpers helpers, Function functionToFill, PropertyInfo functionPropertyInfo, object functionPropertyValue)
		{
			if (functionToFill == null) throw new ArgumentNullException(nameof(functionToFill));
			if (functionPropertyInfo == null) throw new ArgumentNullException(nameof(functionPropertyInfo));
			if (functionPropertyValue == null) throw new ArgumentNullException(nameof(functionPropertyValue));

			var jsonFunctionProperties = functionPropertyInfo.PropertyType.GetProperties();

			helpers.Log(nameof(Service), nameof(TrySetFunctionResource), $"Filling function {functionToFill.Name} with JSON Property {functionPropertyInfo.Name} containing properties {string.Join(";", jsonFunctionProperties.Select(p => p.Name))}");

			foreach (var jsonFunctionProperty in jsonFunctionProperties)
			{
				var jsonPropertyValue = jsonFunctionProperty.GetValue(functionPropertyValue);
				var attributes = jsonFunctionProperty.GetCustomAttributes(true);

				var isResourceNameAttribute = attributes.SingleOrDefault(a => a is IsResourceNameAttribute) as IsResourceNameAttribute;

				helpers.Log(nameof(Service), nameof(TrySetFunctionResource), $"External JSON property {jsonFunctionProperty.Name} has value '{jsonPropertyValue?.ToString()}' and attribute IsResourceName={isResourceNameAttribute != null}");

				bool propertyRepresentsResource = isResourceNameAttribute != null;
				if (!propertyRepresentsResource) continue;

				functionToFill.Resource = (FunctionResource)DataMinerInterface.ResourceManager.GetResources(helpers, ResourceExposers.Name.Equal(jsonPropertyValue?.ToString())).FirstOrDefault();

				helpers.Log(nameof(Service), nameof(TrySetFunctionResource), $"Set function {functionToFill.Name} resource to {functionToFill.ResourceName}");
			}
		}
	}
}
