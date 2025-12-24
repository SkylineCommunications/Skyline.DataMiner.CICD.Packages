namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources
{
	using System;
	using System.Collections.Generic;
    using System.Linq;
    using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public sealed class OccupiedResource : FunctionResource
	{
		private List<OccupyingService> occupyingServices = new List<OccupyingService>();

		public OccupiedResource(FunctionResource resource) : base(resource)
		{
			
		}

		public bool OccupyingServicesAlreadyRetrieved { get; private set; }

		public List<OccupyingService> OccupyingServices
		{
			get => occupyingServices;
			set
			{
				occupyingServices = value;
				OccupyingServicesAlreadyRetrieved = true;
			}
		}

		public bool IsFullyOccupied => OccupyingServices.Count >= MaxConcurrency;

		public static FunctionResource WrapIfOccupied(Helpers helpers, FunctionResource resource, DateTime start, DateTime end, Guid ignoreOrderId, string ignoreServiceName)
		{
			if (resource is null) return resource;

			var occupyingServicess = helpers.ResourceManager.GetOccupyingServices(resource, start, end, ignoreOrderId, ignoreServiceName);

			helpers.Log(nameof(OccupiedResource), nameof(WrapIfOccupied), $"Resource {resource.Name} has occupying services: '{string.Join(", ", occupyingServicess.Select(os => os.ToString()))}' over {start.ToFullDetailString()} until {end.ToFullDetailString()}");

			if (occupyingServicess.Any())
			{
				helpers.Log(nameof(OccupiedResource), nameof(WrapIfOccupied), $"{occupyingServicess.Count} is more than or equal to concurrency {resource.MaxConcurrency} of the resource. Considering this resource as occupied.");

				return new OccupiedResource(resource) { OccupyingServices = occupyingServicess };
			}
			else
			{
				helpers.Log(nameof(OccupiedResource), nameof(WrapIfOccupied), $"{occupyingServicess.Count} is less than concurrency {resource.MaxConcurrency} of the resource. Considering this resource as available.");

				return resource;
			}
		}
	}
}
