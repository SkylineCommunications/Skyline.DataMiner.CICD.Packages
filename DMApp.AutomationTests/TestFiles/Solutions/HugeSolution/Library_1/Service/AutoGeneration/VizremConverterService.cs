namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using System;
    using System.Linq;
    using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public class VizremConverterService : LiveVideoService
	{
		public VizremConverterService(Helpers helpers, Service service, LiveVideoOrder liveVideoOrder)
			: base(helpers, service, liveVideoOrder)
		{
		}

		/// <summary>
		/// A boolean indicating whether this service is already configured correctly.
		/// </summary>
		public bool IsAlreadyUpdated { get; set; } = false;

		/// <summary>
		/// Generate a new vizrem converter service.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="parent">The parent of the vizrem converter service.</param>
		/// <param name="child">The vizrem farm of the vizrem converter service.</param>
		/// <param name="liveVideoOrder">Linked video order of this service.</param>
		/// <returns></returns>
		public static VizremConverterService GenerateNewVizremConverterService(Helpers helpers, LiveVideoService parent, LiveVideoService child, LiveVideoOrder liveVideoOrder)
		{
			bool parentIsStudio = parent.Service?.Definition?.Description != null && parent.Service.Definition.Description.Contains("Studio");
			var converterServiceReference = parentIsStudio ? parent : child; // When vizrem farm is parent, child will always be pointed to the studio again

			helpers.Log(nameof(VizremConverterService), nameof(GenerateNewVizremConverterService), $"Converter service reference: '{converterServiceReference.Service.Definition.Description}'");

			ServiceDefinition.ServiceDefinition vizremConverterServiceDefinition = null;
			if (converterServiceReference.Service.Definition.Description != null && converterServiceReference.Service.Definition.Description.Contains("Studio Mediapolis"))
            {
				vizremConverterServiceDefinition = helpers.ServiceDefinitionManager.VizremConverterMediapolisServiceDefinition ?? throw new ArgumentException($"SD Manager cannot find SD", nameof(helpers));
			}
			else if (converterServiceReference.Service.Definition.Description != null && converterServiceReference.Service.Definition.Description.Contains("Studio Helsinki"))
            {
				vizremConverterServiceDefinition = helpers.ServiceDefinitionManager.VizremConverterHelsinkiServiceDefinition ?? throw new ArgumentException($"SD Manager cannot find SD", nameof(helpers));
			}
            else
            {
				throw new InvalidOperationException($"Unknown SD Description {converterServiceReference.Service.Definition.Description}");
            }
			
			var service = new DisplayedService
			{
				Start = parent.Service.Start,
				End = parent.Service.End,
				PreRoll = GetPreRoll(vizremConverterServiceDefinition, child.Service),
				PostRoll = ServiceManager.GetPostRollDuration(vizremConverterServiceDefinition),
				Definition = vizremConverterServiceDefinition,
				BackupType = parent.Service.BackupType
			};

			var functionNode = vizremConverterServiceDefinition.Diagram.Nodes.Single();
			var functionDefinition = vizremConverterServiceDefinition.FunctionDefinitions.Single();

			var function = new Function.DisplayedFunction(helpers, functionNode, functionDefinition);

            if (parent is VizremStudioService)
            {
				function.InterfaceParameters.Single(p => p.Id == ProfileParameterGuids.ResourceInputConnectionsSdi).Value = parent.OutputResource.Name;
				function.InterfaceParameters.Single(p => p.Id == ProfileParameterGuids.ResourceOutputConnectionsNdi).Value = child.InputResource.Name;
			}
            else if (parent is VizremFarmService)
            {
				function.InterfaceParameters.Single(p => p.Id == ProfileParameterGuids.ResourceInputConnectionsNdi).Value = parent.OutputResource.Name;
				function.InterfaceParameters.Single(p => p.Id == ProfileParameterGuids.ResourceOutputConnectionsSdi).Value = child.InputResource.Name;
			}

			service.Functions.Add(function);

			helpers.Log(nameof(VizremConverterService), nameof(GenerateNewVizremConverterService), $"Generated new vizrem converter service: '{service.GetConfiguration().Serialize()}'");

			var vizremConverterService = new VizremConverterService(helpers, service, liveVideoOrder) { IsNew = true };

			vizremConverterService.Service.AcceptChanges();

			return vizremConverterService;
		}

		public void UpdateValuesBasedOn(VizremConverterRequiringService parent, LiveVideoService child)
		{
			UpdateFunctionsBasedOn(parent, child);

			if (!LiveVideoOrder.Order.ConvertedFromRunningToStartNow)
			{
				Service.Start = child.Service.Start;
				Service.End = child.Service.End;
			}
		}

		public void UpdateFunctionsBasedOn(VizremConverterRequiringService parent, LiveVideoService child)
		{
			var function = Service.Functions[0];

			if (parent is VizremStudioService)
			{
				function.InterfaceParameters.Single(p => p.Id == ProfileParameterGuids.ResourceInputConnectionsSdi).Value = parent.OutputResource?.Name;
				function.InterfaceParameters.Single(p => p.Id == ProfileParameterGuids.ResourceOutputConnectionsNdi).Value = child.InputResource?.Name;
			}
			else if (parent is VizremFarmService)
			{
				function.InterfaceParameters.Single(p => p.Id == ProfileParameterGuids.ResourceInputConnectionsNdi).Value = parent.OutputResource?.Name;
				function.InterfaceParameters.Single(p => p.Id == ProfileParameterGuids.ResourceOutputConnectionsSdi).Value = child.InputResource?.Name;
			}

			IsAlreadyUpdated = true;
		}
	}
}