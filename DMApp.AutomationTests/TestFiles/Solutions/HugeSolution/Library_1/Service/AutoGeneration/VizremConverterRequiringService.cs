namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using System;
    using System.Collections.Generic;
    using System.Linq;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public class VizremConverterRequiringService : LiveVideoService
    {
		public VizremConverterRequiringService(Helpers helpers, Service service, LiveVideoOrder liveVideoOrder)
			: base(helpers, service, liveVideoOrder)
        {

        }

		public VizremConverterService VizremConverterService { get; set; }

		private bool IsVizremConverterRequired
		{
			get
			{
				if (Service.Definition.Description.Contains("Studio Helsinki") || Service.Definition.Description.Contains("Studio Mediapolis"))
				{
					// only studios Helsinki and Mediapolis as source need a Vizrem Converter child

					return Service.Children.Any();
				}
				else if(Service.Definition.Description.Contains("VIZREM Farm"))
				{
					// Vizrem farms to studios Helsinki and Mediapolis need a Vizrem Converter child

					return AllChildrenAndGrandChildren.Any(s => s.Service.Definition.Description.Contains("Studio Helsinki") || s.Service.Definition.Description.Contains("Studio Mediapolis"));
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Indicates if the current Graphics Processing service is valid.
		/// </summary>
		private bool HasValidVizremConverterService => IsVizremConverterRequired && VizremConverterService.Service.Start.ToUniversalTime() <= Service.Start.ToUniversalTime() && VizremConverterService.Service.End.ToUniversalTime() >= Service.End.ToUniversalTime();

		/// <summary>
		/// Indicates if the current Graphics Processing configuration is valid.
		/// </summary>
		public bool HasValidVizremConverterConfiguration
		{
			get
			{
				if (IsVizremConverterRequired)
				{
					if (VizremConverterService == null) return false;

					return HasValidVizremConverterService;
				}
				else
				{
					return VizremConverterService == null;
				}
			}
		}

		/// <summary>
		/// Add or update the vizrem converter configuration.
		/// </summary>
		public void AddOrUpdateVizremConverterConfiguration(List<VizremConverterService> unusedExistingVizremConverterServices = null)
		{
			LogMethodStart(nameof(AddOrUpdateVizremConverterConfiguration), out var stopwatch);

            if (IsVizremConverterRequired)
			{
				Log(nameof(AddOrUpdateVizremConverterConfiguration), $"Vizrem Converter required");

				var vizremConverterServiceToUpdate = VizremConverterService;

				if (vizremConverterServiceToUpdate is null)
				{
					AddVizremConverterService();
				}
                else
                {
					UpdateVizremConverter(vizremConverterServiceToUpdate);
                }
			}
			else if (VizremConverterService != null)
			{
				Log(nameof(AddOrUpdateVizremConverterConfiguration), $"No vizrem converter required, but vizrem converter {VizremConverterService.Service.Name} is present");

				RemoveVizremConverterService();
			}
			else
			{
				Log(nameof(AddOrUpdateVizremConverterConfiguration), $"No vizrem converter required");
			}

			LogMethodCompleted(nameof(AddOrUpdateVizremConverterConfiguration), stopwatch);
		}

		private void AddVizremConverterService()
		{
			if (!this.Service.Children.Any())
			{
				Log(nameof(AddVizremConverterService), $"No need to create a vizrem converter service, as the studio has no children it means this service is declared as end point of the vizrem chain.");
				return;
			}

			var desiredVizremConverterParent = this;
			Log(nameof(AddVizremConverterService), $"Desired converter parent is {desiredVizremConverterParent.Service?.Name}");

			var desiredVizremConverterChild = GetDirectNonRoutingChildren(exludeProcessingRelatedService: true)[0];
			Log(nameof(AddVizremConverterService), $"Desired vizrem converter child is {desiredVizremConverterChild.Service?.Name}");

			VizremConverterService = VizremConverterService.GenerateNewVizremConverterService(Helpers, desiredVizremConverterParent, desiredVizremConverterChild, LiveVideoOrder);

			LiveVideoOrder.LiveVideoServices.Add(VizremConverterService);

			Log(nameof(AddVizremConverterService), $"Created new vizrem converter service {VizremConverterService.Service?.Name}");

			LiveVideoOrder.InsertServiceBetween(VizremConverterService, desiredVizremConverterParent, desiredVizremConverterChild);
			// Note: the order of vizrem orders including converter is the following: 1.Studio 2.Converter 3.Vizrem Farm 4.Converter 5.Studio.
		}

		private void UpdateVizremConverter(VizremConverterService vizremConverterServiceToUpdate)
		{
			VizremConverterService = vizremConverterServiceToUpdate ?? throw new ArgumentNullException(nameof(vizremConverterServiceToUpdate));

			Log(nameof(UpdateVizremConverter), $"Updating vizrem converter service {VizremConverterService.Service.Name}");

			var desiredVizremConverterChild = GetDirectNonRoutingChildren(exludeProcessingRelatedService: true)[0];
			Log(nameof(UpdateVizremConverter), $"Desired vizrem converter child is {desiredVizremConverterChild.Service?.Name}");

			VizremConverterService.UpdateValuesBasedOn(this, desiredVizremConverterChild);
		}

		private void RemoveVizremConverterService()
		{
			LiveVideoService child = null;
			if (Service.Definition.VirtualPlatform == ServiceDefinition.VirtualPlatform.VizremFarm)
			{
				child = LiveVideoOrder.GetVizremStudioServices().Single(s => !s.Children.Any());
			}
			else if(Service.Definition.VirtualPlatform == ServiceDefinition.VirtualPlatform.VizremStudio)
			{
				child = LiveVideoOrder.GetVizremFarmServices().Single();
			}

			LiveVideoOrder.SetParent(child, this);

			VizremConverterService = null;
		}
	}
}
