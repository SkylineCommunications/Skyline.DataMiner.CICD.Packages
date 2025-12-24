namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Functions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using DataMiner.DeveloperCommunityLibrary.YLE.UI.ProfileParameters;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Resource;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Contexts;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class FunctionSectionConfiguration : ISectionConfiguration
	{
		public FunctionSectionConfiguration(Helpers helpers, Function function, ServiceDefinition serviceDefinition, UserInfo userInfo, Guid serviceId)
		{
			DisplayedFunctionLabel = function.Name;

			ResourceSectionConfiguration = new ResourceSectionConfiguration(helpers, function, userInfo);

			foreach (var profileParameter in function.NonDtrNonAudioProfileParameters)
			{
				var allValues = profileParameter.Discreets.Select(d => d.DisplayValue).ToList();

				var allowedValues = userInfo.Contract?.FilterProfileParameterValues(serviceDefinition, profileParameter, userInfo) ?? new List<string>();

				ProfileParameterSectionConfigurations.Add(profileParameter.Id, new ProfileParameterSectionConfiguration
				{
					DisallowedValues = allValues.Except(allowedValues).ToList()
				});
			}

			ProfileParametersInDisplayOrderAboveResourceSelection = new LinkedList<Guid>(function.NonDtrNonAudioProfileParameters.Select(x => x.Id));

			switch (serviceDefinition.VirtualPlatform)
			{
				case VirtualPlatform.AudioProcessing:
				case VirtualPlatform.VideoProcessing:
				case VirtualPlatform.GraphicsProcessing:
					if (helpers.Context.Script == Scripts.LiveOrderForm || (helpers.Context is UpdateServiceContext updateServiceContext && updateServiceContext.ServiceId != serviceId))
					{
						bool isMatrixFunction = FunctionGuids.AllMatrixGuids.Contains(function.Id);
						IsVisible = !isMatrixFunction;
						HideAllProfileParameters(function);
					}

					break;
				case VirtualPlatform.ReceptionSatellite:
				case VirtualPlatform.TransmissionSatellite:
					ResourceSelectionPosition = HorizontalAlignment.Right;
					FunctionSectionDividerCharacter = '░';
					break;

				default:
					// no extra config required
					break;
			}
		}

		[IsIsVisibleProperty]
		public bool IsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool IsEnabled { get; set; } = true;

		public ResourceSectionConfiguration ResourceSectionConfiguration { get; set; }

		public HorizontalAlignment ResourceSelectionPosition { get; set; } = HorizontalAlignment.Left;

		public char FunctionSectionDividerCharacter { get; set; }

		public string DisplayedFunctionLabel { get; set; }

		public Dictionary<Guid, ProfileParameterSectionConfiguration> ProfileParameterSectionConfigurations { get; set; } = new Dictionary<Guid, ProfileParameterSectionConfiguration>();

		/// <summary>
		/// Required for setting properties using reflection.
		/// </summary>
		public IEnumerable<ProfileParameterSectionConfiguration> AllProfileParameterSections => ProfileParameterSectionConfigurations.Values;

		public LinkedList<Guid> ProfileParametersInDisplayOrderAboveResourceSelection { get; set; } = new LinkedList<Guid>();

		public LinkedList<Guid> ProfileParametersInDisplayOrderBelowResourceSelection { get; set; } = new LinkedList<Guid>();

		public int LabelSpan { get; set; } = 2;

		public int InputSpan { get; set; } = 2;

		[JsonIgnore]
		public Dictionary<string, string> ToolTip { get; set; } = ReflectionHandler.ReadTooltipFile();

		public int InputColumn { get; set; } = 2;

		public void SetIsEnabledPropertyValues(bool valueToSet)
		{
			ConfigurationHelper.SetIsEnabledPropertyValues(this, valueToSet);
		}

		public void SetIsVisiblePropertyValues(bool valueToSet)
		{
			ConfigurationHelper.SetIsVisiblePropertyValues(this, valueToSet);
		}

		public void DisableProfileParameter(Guid profileParameterId)
		{
			if (!ProfileParameterSectionConfigurations.TryGetValue(profileParameterId, out var parameterSectionConfiguration))
			{
				parameterSectionConfiguration = new ProfileParameterSectionConfiguration();
				ProfileParameterSectionConfigurations.Add(profileParameterId, parameterSectionConfiguration);
			}

			parameterSectionConfiguration.IsEnabled = false;
		}

		public void DisableAllProfileParameters(Function function)
		{
			foreach (var profileParameter in function.Parameters)
			{
				if (!ProfileParameterSectionConfigurations.TryGetValue(profileParameter.Id, out var parameterSectionConfiguration))
				{
					parameterSectionConfiguration = new ProfileParameterSectionConfiguration();
					ProfileParameterSectionConfigurations.Add(profileParameter.Id, parameterSectionConfiguration);
				}

				parameterSectionConfiguration.SetIsEnabledPropertyValues(false);
			}
		}

		public void HideProfileParameter(Guid profileParameterId)
		{
			if (!ProfileParameterSectionConfigurations.TryGetValue(profileParameterId, out var parameterSectionConfiguration))
			{
				parameterSectionConfiguration = new ProfileParameterSectionConfiguration();
				ProfileParameterSectionConfigurations.Add(profileParameterId, parameterSectionConfiguration);
			}

			parameterSectionConfiguration.IsVisible = false;
		}

		public void HideAllProfileParameters(Function function)
		{
			foreach (var profileParameter in function.Parameters)
			{
				if (!ProfileParameterSectionConfigurations.TryGetValue(profileParameter.Id, out var parameterSectionConfiguration))
				{
					parameterSectionConfiguration = new ProfileParameterSectionConfiguration();
					ProfileParameterSectionConfigurations.Add(profileParameter.Id, parameterSectionConfiguration);
				}

				parameterSectionConfiguration.SetIsVisiblePropertyValues(false);
			}
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}