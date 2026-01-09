namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DTR
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration;
	using Skyline.DataMiner.Library.Solutions.SRM.LifecycleServiceOrchestration;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using InterfaceType = Net.ServiceManager.Objects.InterfaceType;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	/// <summary>
	/// Resource Update Handler is used for DTR to trigger on any resource changes and update any linked resource capabilities to make sure the correct resource is selected.
	/// </summary>
	public static class ResourceUpdateHandler
	{
		/// <summary>
		/// Get the resource capability filters for a given resource update.
		/// </summary>
		/// <returns>A collection of resource capability filters that need to be applied based on the updated resource.</returns>
		public static IEnumerable<ResourceCapabilityFilter> GetResourceCapabilityFilters(ResourceUpdateInfo resourceUpdateInfo, YLE.Service.Service service)
		{
			if (resourceUpdateInfo == null) throw new ArgumentNullException(nameof(resourceUpdateInfo));
			if (service == null) throw new ArgumentNullException(nameof(service));

			switch (resourceUpdateInfo.ServiceDefinitionName)
			{
				case "_Fiber RX Limited Capacity":
					return GetFiberRxLimitedCapacityResourceCapabilityFilters(resourceUpdateInfo);
				case "_Fiber TX Limited Capacity":
					return GetFiberTxLimitedCapacityResourceCapabilityFilters(resourceUpdateInfo);
				case "_Satellite RX":
					return GetSatelliteRxResourceCapabilityFilters(resourceUpdateInfo, service);
				case "_Satellite TX":
					return GetSatelliteTxResourceCapabilityFilters(resourceUpdateInfo);
				case "_Fixed Service RX":
					return GetFixedServiceResourceCapabilityFilters(resourceUpdateInfo);
				case "_Audio Processing":
					return GetAudioProcessingResourceCapabilityFilters(resourceUpdateInfo);
				case "_IP RX SRT":
				case "_IP RX RTMP":
					return GetIpRxIntinorResourceCapabilityFilters(resourceUpdateInfo);
				default:
					return new ResourceCapabilityFilter[0];
			}
		}

		public static Dictionary<string, List<ProfileParameter>> GetDownstreamDtrParameters(Function functionToStartFrom, YLE.Service.Service service)
		{
			var downstreamDtrParameters = new Dictionary<string, List<ProfileParameter>>();

			var downstreamFunctions = service.Functions.Where(f => f.ConfigurationOrder > functionToStartFrom.ConfigurationOrder).ToList();

			foreach (var downstreamFunction in downstreamFunctions)
			{
				downstreamDtrParameters.Add(downstreamFunction.Definition.Label, new List<ProfileParameter>());

				foreach (var parameter in downstreamFunction.Parameters.Concat(downstreamFunction.InterfaceParameters))
				{
					if (TryGetFunctionOfWhichDtrSetsParameter(downstreamFunction, parameter, service, out var resultingFunction) && resultingFunction.ConfigurationOrder >= functionToStartFrom.ConfigurationOrder)
					{
						// parameter is set by DTR for a function with higher configuration order

						downstreamDtrParameters[downstreamFunction.Definition.Label].Add(parameter);
					}
				}
			}

			return downstreamDtrParameters;
		}

		public static bool TryGetFunctionOfWhichDtrSetsParameter(Function functionContainingParameter, ProfileParameter parameter, YLE.Service.Service service, out Function resultingFunction)
		{
			try
			{
				resultingFunction = GetFunctionOfWhichDtrSetsParameter(functionContainingParameter, parameter, service);
				return true;
			}
			catch
			{
				resultingFunction = null;
				return false;
			}
		}

		public static Function GetFunctionOfWhichDtrSetsParameter(Function functionContainingParameter, ProfileParameter parameter, YLE.Service.Service service)
		{
			foreach (var function in service.Functions)
			{
				var resourceUpdateInfo = new ResourceUpdateInfo
				{
					ServiceDefinitionName = service.Definition.Name,
					UpdatedResourceFunctionLabel = function.Definition.Label,
					UpdatedResource = new FunctionResource(),
					ConnectedResource = new FunctionResource(),
				};

				var filters = GetResourceCapabilityFilters(resourceUpdateInfo, service);

				var filterSettingTheParameter = filters.SingleOrDefault(f => f.FunctionLabel == functionContainingParameter.Definition.Label && f.CapabilityParameterName == parameter.Name);
				if (filterSettingTheParameter is null) continue;

				return function;
			}

			throw new InvalidOperationException($"Could not find a function for which the DTR sets {functionContainingParameter.Definition.Label} {parameter.Name}");
		}

		/// <summary>
		/// Get the resource capability filters for a give resource assigned in the IP RX SRT service. 
		/// </summary>
		/// <param name="resourceUpdateInfo">The resource update info of the updated resource.</param>
		/// <returns>A collection of resource capability filters that need to be applied based on the updated resource.</returns>
		private static IEnumerable<ResourceCapabilityFilter> GetIpRxIntinorResourceCapabilityFilters(ResourceUpdateInfo resourceUpdateInfo)
		{
			if (resourceUpdateInfo is null) throw new ArgumentNullException(nameof(resourceUpdateInfo));

			if (resourceUpdateInfo.UpdatedResourceFunctionLabel == "IP Decoding")
			{
				string value;
				if (resourceUpdateInfo.UpdatedResource is null)
				{
					value = false.ToString();
				}
				else
				{
					value = bool.TryParse(resourceUpdateInfo.UpdatedResource.GetResourcePropertyStringValue(ResourcePropertyNames.RequiresIpDecodingOutput), out bool requiresIpDecodingOutput) ? (!requiresIpDecodingOutput).ToString() : false.ToString();
				}

				return new[]
				{
					new ResourceCapabilityFilter
					{
						FunctionLabel = "IP Decoding Output",
						CapabilityParameterName = "_Dummy",
						CapabilityParameterValue = value,
					}
				};
			}
			else
			{
				return new ResourceCapabilityFilter[0];
			}
		}

		/// <summary>
		/// Get the resource capability filters for a give resource assigned in the Fiber RX Limited Capacity service. 
		/// </summary>
		/// <param name="resourceUpdateInfo">The resource update info of the updated resource.</param>
		/// <returns>A collection of resource capability filters that need to be applied based on the updated resource.</returns>
		private static IEnumerable<ResourceCapabilityFilter> GetFiberRxLimitedCapacityResourceCapabilityFilters(ResourceUpdateInfo resourceUpdateInfo)
		{
			switch (resourceUpdateInfo.UpdatedResourceFunctionLabel)
			{
				case "Source":
					return new[]
						   {
							   new ResourceCapabilityFilter
							   {
								   FunctionLabel = "ASI Matrix Input",
								   InterfaceType = InterfaceType.In,
								   InterfaceName = "ASI",
								   CapabilityParameterName = "ResourceInputConnections_ASI",
								   CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
							   }
						   };
				case "Decoding":
					return new[]
						   {
							   new ResourceCapabilityFilter
							   {
								   FunctionLabel = "ASI Matrix Output",
								   InterfaceType = InterfaceType.Out,
								   InterfaceName = "ASI",
								   CapabilityParameterName = "ResourceOutputConnections_ASI",
								   CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
							   }
						   };
				default:
					return new ResourceCapabilityFilter[0];
			}
		}

		/// <summary>
		/// Get the resource capability filters for a give resource assigned in the Fiber TX Limited Capacity service. 
		/// </summary>
		/// <param name="resourceUpdateInfo">The resource update info of the updated resource.</param>
		/// <returns>A collection of resource capability filters that need to be applied based on the updated resource.</returns>
		private static IEnumerable<ResourceCapabilityFilter> GetFiberTxLimitedCapacityResourceCapabilityFilters(ResourceUpdateInfo resourceUpdateInfo)
		{
			switch (resourceUpdateInfo.UpdatedResourceFunctionLabel)
			{
				case "Encoding":
					return new[]
						   {
							   new ResourceCapabilityFilter
							   {
								   FunctionLabel = "ASI Matrix Input",
								   InterfaceType = InterfaceType.In,
								   InterfaceName = "ASI",
								   CapabilityParameterName = "ResourceInputConnections_ASI",
								   CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
							   }
						   };
				case "Destination":
					return new[]
						   {
							   new ResourceCapabilityFilter
							   {
								   FunctionLabel = "ASI Matrix Output",
								   InterfaceType = InterfaceType.Out,
								   InterfaceName = "ASI",
								   CapabilityParameterName = "ResourceOutputConnections_ASI",
								   CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
							   }
						   };
				default:
					return new ResourceCapabilityFilter[0];
			}
		}

		/// <summary>
		/// Get the resource capability filters for a give resource assigned in the Satellite RX service. 
		/// </summary>
		/// <param name="resourceUpdateInfo">The resource update info of the updated resource.</param>
		/// <param name="service"></param>
		/// <returns>A collection of resource capability filters that need to be applied based on the updated resource.</returns>
		private static IEnumerable<ResourceCapabilityFilter> GetSatelliteRxResourceCapabilityFilters(ResourceUpdateInfo resourceUpdateInfo, YLE.Service.Service service)
		{
			ArgumentNullCheck.ThrowIfNull(resourceUpdateInfo, nameof(resourceUpdateInfo));

			var resourceCapabilityFilters = new List<ResourceCapabilityFilter>();

			// update the demodulating configuration capability to filter the NS3/4 demodulators correctly
			var demodulatingConfigurationResourceCapabilityFilter = UpdateDemodulatingConfigurationResourceCapabilityFilters(service);
			resourceCapabilityFilters.AddRange(demodulatingConfigurationResourceCapabilityFilter);

			switch (resourceUpdateInfo.UpdatedResourceFunctionLabel)
			{
				case "Satellite":
					{
						var orbitalPositionProperty = resourceUpdateInfo.UpdatedResource != null ? resourceUpdateInfo.UpdatedResource.Properties.FirstOrDefault(p => p.Name == "Orbital Position") : null;
						string orbitalPosition = orbitalPositionProperty?.Value ?? string.Empty;

						resourceCapabilityFilters.Add(new ResourceCapabilityFilter
						{
							FunctionLabel = "Antenna",
							CapabilityParameterName = "_Orbital Position",
							CapabilityParameterValue = orbitalPosition
						});

						break;
					}

				case "Antenna":
					{
						resourceCapabilityFilters.AddRange(new[]
						{
							new ResourceCapabilityFilter
							{
								FunctionLabel = "LBand Matrix Input",
								InterfaceType = InterfaceType.In,
								InterfaceName = "LBAND",
								CapabilityParameterName = "ResourceInputConnections_LBAND",
								CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
							},
							new ResourceCapabilityFilter
							{
								FunctionLabel = "LBand Matrix Output",
								InterfaceType = InterfaceType.In,
								InterfaceName = "LBAND",
								CapabilityParameterName = "ResourceInputConnections_LBAND",
								CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
							}
						});

						break;
					}

				case "LBand Matrix Input" when resourceUpdateInfo.UpdatedResource != null:
					{
						var lnbFrequencyProperty = resourceUpdateInfo.UpdatedResource.Properties.SingleOrDefault(p => p.Name == ResourcePropertyNames.LnbFrequency);
						if (lnbFrequencyProperty is null) break;

						if (!double.TryParse(lnbFrequencyProperty.Value, out double lnbFrequency)) throw new InvalidOperationException($"Unable to parse property value '{lnbFrequencyProperty.Value}' to a double");

						resourceCapabilityFilters.Add(new ResourceCapabilityFilter
						{
							FunctionLabel = "Demodulating",
							CapabilityParameterName = "_LNB Frequency",
							CapabilityParameterValue = lnbFrequencyProperty.Value,
						});

						break;
					}

				case "LBand Matrix Output":
					{
						var matrixProperty = resourceUpdateInfo.UpdatedResource != null ? resourceUpdateInfo.UpdatedResource.Properties.FirstOrDefault(p => p.Name == ResourcePropertyNames.Matrix || p.Name == ResourcePropertyNames._Matrix) : null;

						string matrix = matrixProperty?.Value ?? string.Empty;

						resourceCapabilityFilters.AddRange(new[]
						{
							new ResourceCapabilityFilter
							{
								FunctionLabel = "LBand Matrix Input",
								CapabilityParameterName = "_Matrix",
								CapabilityParameterValue = matrix
							},
							new ResourceCapabilityFilter
							{
								FunctionLabel = "Demodulating",
								InterfaceType = InterfaceType.In,
								InterfaceName = "LBAND",
								CapabilityParameterName = "ResourceInputConnections_LBAND",
								CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
							}
						});

						break;
					}

				case "Demodulating":
					{
						resourceCapabilityFilters.Add(new ResourceCapabilityFilter
						{
							FunctionLabel = "Decoding",
							InterfaceType = InterfaceType.In,
							InterfaceName = "ASI",
							CapabilityParameterName = "ResourceInputConnections_ASI",
							CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
						});

						break;
					}

				case "Decoding":
					{
						var decodingResource = (FunctionResource)resourceUpdateInfo.UpdatedResource;
						var demodulatingResource = (FunctionResource)resourceUpdateInfo.ConnectedResource;

						var input = "None";
						var output = "None";

						if (decodingResource != null && demodulatingResource != null && (decodingResource.MainDVEDmaID != demodulatingResource.MainDVEDmaID || decodingResource.MainDVEElementID != demodulatingResource.MainDVEElementID))
						{
							input = demodulatingResource.Name;
							output = decodingResource.Name;
						}

						resourceCapabilityFilters.AddRange(new[]
						{
							new ResourceCapabilityFilter
							{
								FunctionLabel = "ASI Matrix Input",
								InterfaceType = InterfaceType.In,
								InterfaceName = "ASI",
								CapabilityParameterName = "ResourceInputConnections_ASI",
								CapabilityParameterValue = input
							},
							new ResourceCapabilityFilter
							{
								FunctionLabel = "ASI Matrix Output",
								InterfaceType = InterfaceType.Out,
								InterfaceName = "ASI",
								CapabilityParameterName = "ResourceOutputConnections_ASI",
								CapabilityParameterValue = output
							}
						});

						break;
					}

				default:
					// nothing to do
					break;
			}

			return resourceCapabilityFilters;
		}

		/// <summary>
		/// Get the resource capability filters for a given resource assigned in the Audio Processing service. 
		/// </summary>
		/// <param name="resourceUpdateInfo">The resource update info of the updated resource.</param>
		/// <returns>A collection of resource capability filters that need to be applied based on the updated resource.</returns>
		private static IEnumerable<ResourceCapabilityFilter> GetAudioProcessingResourceCapabilityFilters(ResourceUpdateInfo resourceUpdateInfo)
		{
			if (resourceUpdateInfo == null) throw new ArgumentNullException(nameof(resourceUpdateInfo));

			var resourceCapabilityFilters = new List<ResourceCapabilityFilter>();

			switch (resourceUpdateInfo.UpdatedResourceFunctionLabel)
			{
				case "Audio Deembedding":
					resourceCapabilityFilters.Add(
						new ResourceCapabilityFilter
						{
							FunctionLabel = "Matrix SDI Input Audio Deembedding",
							InterfaceType = InterfaceType.In,
							InterfaceName = "SDI",
							CapabilityParameterName = "ResourceInputConnections_SDI",
							CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
						});
					break;

				case "Audio Dolby Decoding":
					resourceCapabilityFilters.AddRange(new[]
					{
						new ResourceCapabilityFilter
						{
							FunctionLabel = "Matrix SDI Output Audio Dolby Decoding",
							InterfaceType = InterfaceType.Out,
							InterfaceName = "SDI",
							CapabilityParameterName = "ResourceOutputConnections_SDI",
							CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
						},
						new ResourceCapabilityFilter
						{
							FunctionLabel = "Matrix SDI Input Audio Dolby Decoding",
							InterfaceType = InterfaceType.In,
							InterfaceName = "SDI",
							CapabilityParameterName = "ResourceInputConnections_SDI",
							CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
						}
					});
					break;

				case "Audio Shuffling":
					resourceCapabilityFilters.AddRange(new[]
					{
						new ResourceCapabilityFilter
						{
							FunctionLabel = "Matrix SDI Output Audio Shuffling",
							InterfaceType = InterfaceType.Out,
							InterfaceName = "SDI",
							CapabilityParameterName = "ResourceOutputConnections_SDI",
							CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
						},
						new ResourceCapabilityFilter
						{
							FunctionLabel = "Matrix SDI Input Audio Shuffling",
							InterfaceType = InterfaceType.In,
							InterfaceName = "SDI",
							CapabilityParameterName = "ResourceInputConnections_SDI",
							CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
						}
					});
					break;

				case "Audio Embedding":
					resourceCapabilityFilters.Add(
						new ResourceCapabilityFilter
						{
							FunctionLabel = "Matrix SDI Output Audio Embedding",
							InterfaceType = InterfaceType.Out,
							InterfaceName = "SDI",
							CapabilityParameterName = "ResourceOutputConnections_SDI",
							CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
						});
					break;

				default:
					// nothing to do
					break;
			}

			return resourceCapabilityFilters;
		}

		private static ResourceCapabilityFilter[] UpdateDemodulatingConfigurationResourceCapabilityFilters(YLE.Service.Service service)
		{
			var demodulatingFunction = service.Functions.FirstOrDefault(f => f.Id == FunctionGuids.Demodulating);
			if (demodulatingFunction == null) return new ResourceCapabilityFilter[0];

			var modulationStandardProfileParameter = demodulatingFunction.Parameters.FirstOrDefault(p => p.Id == ProfileParameterGuids.ModulationStandard);
			if (modulationStandardProfileParameter == null) return new ResourceCapabilityFilter[0];

			var demodulatingConfiguration = String.Empty;

			if (modulationStandardProfileParameter.StringValue.Contains("NS"))
			{
				var downlinkFrequencyProfileParameter = demodulatingFunction.Parameters.FirstOrDefault(p => p.Id == ProfileParameterGuids.DownlinkFrequency);
				var polarizationProfileParameter = demodulatingFunction.Parameters.FirstOrDefault(p => p.Id == ProfileParameterGuids.Polarization);
				var symbolRateProfileParameter = demodulatingFunction.Parameters.FirstOrDefault(p => p.Id == ProfileParameterGuids.SymbolRate);

				demodulatingConfiguration = String.Join(";",
					downlinkFrequencyProfileParameter?.StringValue,
					modulationStandardProfileParameter.StringValue,
					polarizationProfileParameter?.StringValue,
					symbolRateProfileParameter?.StringValue);
			}

			return new[]
			{
				new ResourceCapabilityFilter
				{
					FunctionLabel = "LBand Matrix Output",
					CapabilityParameterName = "_Demodulating Configuration",
					CapabilityParameterValue = demodulatingConfiguration
				},
				new ResourceCapabilityFilter
				{
					FunctionLabel = "Demodulating",
					CapabilityParameterName = "_Demodulating Configuration",
					CapabilityParameterValue = demodulatingConfiguration
				}
			};
		}

		/// <summary>
		/// Get the resource capability filters for a give resource assigned in the Satellite TX service. 
		/// </summary>
		/// <param name="resourceUpdateInfo">The resource update info of the updated resource.</param>
		/// <returns>A collection of resource capability filters that need to be applied based on the updated resource.</returns>
		private static IEnumerable<ResourceCapabilityFilter> GetSatelliteTxResourceCapabilityFilters(ResourceUpdateInfo resourceUpdateInfo)
		{
			switch (resourceUpdateInfo.UpdatedResourceFunctionLabel)
			{
				case "Satellite":
					var orbitalPosition = String.Empty;
					var orbitalPositionProperty = resourceUpdateInfo.UpdatedResource != null ? resourceUpdateInfo.UpdatedResource.Properties.FirstOrDefault(p => p.Name == "Orbital Position") : null;
					if (orbitalPositionProperty != null) orbitalPosition = orbitalPositionProperty.Value;

					return new[]
					{
						new ResourceCapabilityFilter
						{
							FunctionLabel = "Antenna",
							CapabilityParameterName = "_Orbital Position",
							CapabilityParameterValue = orbitalPosition
						},
						new ResourceCapabilityFilter
						{
							FunctionLabel = "Modulating",
							CapabilityParameterName = "_Orbital Position",
							CapabilityParameterValue = orbitalPosition
						}
					};
				case "Modulating":
					return new[]
						   {
							   new ResourceCapabilityFilter
							   {
								   FunctionLabel = "Antenna",
								   InterfaceType = InterfaceType.In,
								   InterfaceName = "LBAND",
								   CapabilityParameterName = "ResourceInputConnections_LBAND",
								   CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
							   },
							   new ResourceCapabilityFilter
							   {
								   FunctionLabel = "ASI Matrix Output",
								   InterfaceType = InterfaceType.Out,
								   InterfaceName = "ASI",
								   CapabilityParameterName = "ResourceOutputConnections_ASI",
								   CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
							   }
						   };
				case "Encoding":
					return new[]
						   {
							   new ResourceCapabilityFilter
							   {
								   FunctionLabel = "ASI Matrix Input",
								   InterfaceType = InterfaceType.In,
								   InterfaceName = "ASI",
								   CapabilityParameterName = "ResourceInputConnections_ASI",
								   CapabilityParameterValue = resourceUpdateInfo.UpdatedResource?.Name
							   }
						   };
				default:
					return new ResourceCapabilityFilter[0];
			}
		}

		/// <summary>
		/// Get the resource capability filters for a given resource assigned in the Fixed Service service.
		/// </summary>
		/// <param name="resourceUpdateInfo">The resource update info of the updated resource.</param>
		/// <returns>A collection of resource capability filters that need to be applied based on the updated resource.</returns>
		private static IEnumerable<ResourceCapabilityFilter> GetFixedServiceResourceCapabilityFilters(ResourceUpdateInfo resourceUpdateInfo)
		{
			if (resourceUpdateInfo.UpdatedResourceFunctionLabel == "Source")
			{
				var linkedIrd = String.Empty;
				if (resourceUpdateInfo.UpdatedResource != null)
				{
					var linkedIrdProperty = resourceUpdateInfo.UpdatedResource.Properties.FirstOrDefault(p => p.Name == ResourcePropertyNames.LinkedIrdPropertyName);
					if (linkedIrdProperty != null && !String.IsNullOrEmpty(linkedIrdProperty.Value)) linkedIrd = linkedIrdProperty.Value;
				}

				return new[]
				{
					new ResourceCapabilityFilter
					{
						FunctionLabel = "Decoding",
						InterfaceType = InterfaceType.In,
						InterfaceName = "ASI",
						CapabilityParameterName = "ResourceInputConnections_ASI",
						CapabilityParameterValue = String.Format("{0}.Demodulating", linkedIrd)
					}
				};
			}

			return new ResourceCapabilityFilter[0];
		}
	}
}