namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Library_1.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM.Model;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.SRM.Capabilities;

	public class FunctionDefinition : ICloneable
	{
		public FunctionDefinition()
		{

		}

		private FunctionDefinition(FunctionDefinition other)
		{
			InputInterfaces = other.InputInterfaces.Select(o => o.Clone()).Cast<FunctionInterfaceDefinition>().ToList();
			OutputInterfaces = other.OutputInterfaces.Select(o => o.Clone()).Cast<FunctionInterfaceDefinition>().ToList();

			CloneHelper.CloneProperties(other, this);
		}

		public Guid Id { get; set; }

		public string Name { get; set; }

		/// <summary>
		/// The label assigned to this function definition in the service definition.
		/// </summary>
		public string Label { get; set; }

		public int ConfigurationOrder { get; set; }

		public string ResourcePool { get; set; }

        public bool IsHidden { get; set; }

        public ProfileDefinition ProfileDefinition { get; set; }

		public List<FunctionInterfaceDefinition> InputInterfaces { get; set; } = new List<FunctionInterfaceDefinition>();

		public List<FunctionInterfaceDefinition> OutputInterfaces { get; set; } = new List<FunctionInterfaceDefinition>();

        public List<Guid> Children { get; set; }

		public bool IsDummy { get; private set; }

		public static FunctionDefinition DummyFunctionDefinition()
        {
			return new FunctionDefinition
			{
				Id = Guid.Empty,
				Name = "Dummy",
				Label = "Dummy",
				ConfigurationOrder = 0,
				IsHidden = true,
				ProfileDefinition = ProfileDefinition.DummyProfileDefinition(),
				Children = new List<Guid>(),
				IsDummy = true,
			};
        }

        public YleEligibleResourceContext GetEligibleResourceContext(Helpers helpers, DateTime start, DateTime end, Guid? serviceToIgnoreGuid = null, int? functionToIgnoreNodeId = null, IEnumerable<ProfileParameter> capabilities = null)
		{
			capabilities = capabilities ?? new List<ProfileParameter>();

			var resourcePool = helpers.ResourceManager.GetResourcePoolByName(ResourcePool) ?? throw new ResourcePoolNotFoundException(ResourcePool);

			var resourceCapabilityUsages = new List<ResourceCapabilityUsage>();
			foreach (var parameter in capabilities)
			{
				var resourceCapabilityUsage = new ResourceCapabilityUsage { CapabilityProfileID = parameter.Id };
				switch (parameter.Type)
				{
					case ParameterType.Number:
						resourceCapabilityUsage.RequiredRangePoint = Convert.ToDouble(parameter.Value);
						break;
					case ParameterType.Discrete:
						resourceCapabilityUsage.RequiredDiscreet = Convert.ToString(parameter.Value);
						break;
					default:
						resourceCapabilityUsage.RequiredString = Convert.ToString(parameter.Value);
						break;
				}

				resourceCapabilityUsages.Add(resourceCapabilityUsage);
			}

			var context = new YleEligibleResourceContext(Label, resourcePool.GUID)
			{
				ContextId = Guid.NewGuid(),
				TimeRange = new Net.Time.TimeRangeUtc(start.ToUniversalTime(), end.ToUniversalTime()),
				ResourceFilter = ResourceExposers.PoolGUIDs.Contains(resourcePool.GUID),
				RequiredCapabilities = resourceCapabilityUsages
			};

			if (serviceToIgnoreGuid.HasValue && functionToIgnoreNodeId.HasValue)
			{
				context.ReservationIdToIgnore = new Net.ReservationInstanceID(serviceToIgnoreGuid.Value);
				context.NodeIdToIgnore = functionToIgnoreNodeId.Value;
			}

			return context;
		}

		public object Clone()
		{
			return new FunctionDefinition(this);
		}
	}
}