namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
	using System.Collections.Generic;
	using System.Linq;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public abstract class RoutingRequiringService : ProcessingRelatedService
	{
		protected RoutingRequiringService(Helpers helpers, Service service, LiveVideoOrder liveVideoOrder)
			: base(helpers, service, liveVideoOrder)
		{
		}

		public RoutingServiceChain RoutingServiceChain { get; set; }

		protected SourceService Source => LiveVideoOrder.GetSource(this);

		public abstract bool ProcessesSignalSameAs(RoutingRequiringService service);

		public void InitializeCurrentRoutingServiceChain()
		{
			RoutingService outputRoutingService = null;
			RoutingService inputRoutingService = null;
			RoutingService connectingRoutingService = null;

			var routingParents = LiveVideoOrder.GetRoutingParents(this);

			outputRoutingService = routingParents.FirstOrDefault();

			if (routingParents.Count == 2)
			{
				inputRoutingService = routingParents[1];
			}
			else if(routingParents.Count == 3)
			{
				connectingRoutingService = routingParents[1];
				inputRoutingService = routingParents[2];
			}
			else
			{
				// nothing
			}		

			var inputService = GetYoungestNonRoutingParent();

			RoutingServiceChain = new RoutingServiceChain(Helpers, inputService, inputRoutingService, connectingRoutingService, outputRoutingService, this, LiveVideoOrder);

			Log(nameof(InitializeCurrentRoutingServiceChain), $"Currently applied routing service chain is {RoutingServiceChain}");
		}

		/// <summary>
		/// Add or update the routing configuration.
		/// </summary>
		public void AddOrUpdateRoutingConfiguration(out List<RoutingService> removedServices)
		{
			LogMethodStart(nameof(AddOrUpdateRoutingConfiguration), out var stopwatch, Service.Name);

			Log(nameof(AddOrUpdateRoutingConfiguration), $"The service {Service.Name} requires routing: {Service.RequiresRouting}");

            if (!Service.RequiresRouting)
            {
				LiveVideoOrder.SetParent(this, RoutingServiceChain.InputService);
				removedServices = RoutingServiceChain.AllRoutingServices;
				LogMethodCompleted(nameof(AddOrUpdateRoutingConfiguration), stopwatch);
				return;
            }

			RoutingServiceChain.InitializeSelectableRoutingResourceChains();

			RoutingServiceChain.RoutingResourceChain.IsValid = RoutingServiceChain.SelectableRoutingResourceChains.Contains(RoutingServiceChain.RoutingResourceChain);
			// current chain is valid if it is selectable

			RoutingResourceChain selectedRoutingResourceChain = null;
			RoutingServiceChain routingServiceChainToShare = null;

			var singleHopChainAvailable = RoutingServiceChain.SelectableRoutingResourceChains.Any(x => x.AmountOfHops == 1 && x.IsValid);

			if (singleHopChainAvailable)
			{
				bool currentChainIsSelectable = RoutingServiceChain.SelectableRoutingResourceChains.Contains(RoutingServiceChain.RoutingResourceChain);

				if (currentChainIsSelectable)
				{
					selectedRoutingResourceChain = RoutingServiceChain.RoutingResourceChain;

					Log(nameof(AddOrUpdateRoutingConfiguration), $"Selected current routing chain {selectedRoutingResourceChain}");
				}
				else
				{
					var otherRoutingServiceChainsForSameInputService = LiveVideoOrder.GetRoutingServiceChainsWithSameInputServiceAs(Service.Id).Except(new[] { RoutingServiceChain }).ToList();

					selectedRoutingResourceChain = RoutingServiceChain.SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains(otherRoutingServiceChainsForSameInputService, out routingServiceChainToShare);

					Log(nameof(AddOrUpdateRoutingConfiguration), $"Selected new routing chain {selectedRoutingResourceChain}");
				}
			}
			else
			{
				var otherRoutingServiceChainsForSameInputService = LiveVideoOrder.GetRoutingServiceChainsWithSameInputServiceAs(Service.Id).Except(new[] {RoutingServiceChain}).ToList();

				selectedRoutingResourceChain = RoutingServiceChain.SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains(otherRoutingServiceChainsForSameInputService, out routingServiceChainToShare);
			}

			RoutingServiceChain.Apply(selectedRoutingResourceChain, out var removedServicesPart1);

			RoutingServiceChain.TryShareServicesWith(routingServiceChainToShare, out var removedServicePart2);

			removedServices = removedServicesPart1.Concat(removedServicePart2).ToList();

			Log(nameof(AddOrUpdateRoutingConfiguration), $"Updated Routing Service Chain: {RoutingServiceChain}");

			LogMethodCompleted(nameof(AddOrUpdateRoutingConfiguration), stopwatch);
		}
	}
}
