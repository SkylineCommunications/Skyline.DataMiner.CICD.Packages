namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Library_1.Utilities;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library;
	using Skyline.DataMiner.Net.ServiceManager.Objects;
	using FunctionDefinition = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.FunctionDefinition;

	public class ServiceDefinition : ICloneable
    {
        private readonly VirtualPlatform virtualPlatform;

        public ServiceDefinition()
        {

        }

        public ServiceDefinition(string virtualPlatform)
        {
            if (!EnumExtensions.GetEnumDescriptions<VirtualPlatform>().Contains(virtualPlatform))
                throw new ArgumentException(String.Format("Unknown Virtual Platform {0}", virtualPlatform), "virtualPlatform");

            this.virtualPlatform = virtualPlatform.GetEnumValue<VirtualPlatform>();
        }

        public ServiceDefinition(VirtualPlatform virtualPlatform)
        {
            this.virtualPlatform = virtualPlatform;
        }

		private ServiceDefinition(ServiceDefinition other)
		{
			FunctionDefinitions = other.FunctionDefinitions.Select(fd => (FunctionDefinition)fd.Clone()).ToList();
			this.virtualPlatform = other.virtualPlatform;

			CloneHelper.CloneProperties(other, this);
		}

        /// <summary>
        /// The id (GUID) of the service definition
        /// Only required when action is "NEW" or "EDIT"
        /// </summary>
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsDefault { get; set; }

        /// <summary>
        /// Indicates if an order with a service using this service definition only allows a source (no destinations can be added).
        /// </summary>
        public bool IsSourceOnly { get; set; }

        /// <summary>
        /// Indicates if this service definition can only be selected by MCR users.
        /// </summary>
        public bool IsMcrOnly { get; set; }

        /// <summary>
        /// Indicates if this service definition can only be used by integration orders.
        /// </summary>
        public bool IsIntegrationOnly { get; set; }

        /// <summary>
        /// The booking manager element name that will be used for the order bookings
        /// Always required
        /// </summary>
        public string BookingManagerElementName { get; set; }

        [JsonIgnore]
        public ContributingConfig ContributingConfig { get; set; }

        public VirtualPlatform VirtualPlatform => virtualPlatform;

        [JsonIgnore]
        public VirtualPlatformType VirtualPlatformServiceType => virtualPlatform.GetDescription().Split('.')[0].GetEnumValue<VirtualPlatformType>();

        [JsonIgnore]
        public VirtualPlatformName VirtualPlatformServiceName => virtualPlatform.GetDescription().Split('.').Last().GetEnumValue<VirtualPlatformName>();

        [JsonIgnore]
		public IEnumerable<FunctionDefinition> FunctionDefinitions { get; set; } = new List<FunctionDefinition>();

        [JsonIgnore]
        public Graph Diagram { get; set; }

        [JsonIgnore]
		public bool IsDummy => VirtualPlatformServiceName == VirtualPlatformName.None || VirtualPlatformServiceName == VirtualPlatformName.Unknown; // TODO: what about Eurovision services?

		[JsonIgnore]
		public bool IsEndPointService => VirtualPlatformServiceType == VirtualPlatformType.Recording || VirtualPlatformServiceType == VirtualPlatformType.Transmission || VirtualPlatformServiceType == VirtualPlatformType.Destination;

        public static ServiceDefinition GenerateDummyServiceDefinition(VirtualPlatform virtualPlatform)
		{
            return new ServiceDefinition(virtualPlatform)
            {
                Name = "Dummy",
                Id = Guid.Empty,
                ContributingConfig = new ContributingConfig { ParentSystemFunction = ServiceManager.SourceServiceSystemFunctionId, ResourcePool = "Dummy" },
                FunctionDefinitions = new List<FunctionDefinition> { FunctionDefinition.DummyFunctionDefinition() },
                IsDefault = true,
                IsSourceOnly = false,
                IsMcrOnly = false,
                IsIntegrationOnly = false,
                Description = String.Empty,
                Diagram = new Graph
                {
                    Nodes = new List<Node>
                    {
                        new Node
                        {
                            Label = "Dummy",
                            Position = new Position(0, 0),
                            ID = 1,
                            Configuration = new NodeConfiguration
                            {
                                FunctionID = Guid.Empty
                            }
                        }
                    },
                    Edges = new List<Edge>()
                }
            };
        }

        public static ServiceDefinition GenerateDummyReceptionServiceDefinition()
        {
			return new ServiceDefinition(VirtualPlatform.ReceptionNone)
            {
                Name = "Dummy Reception",
                Id = Guid.Empty,
                ContributingConfig = new ContributingConfig { ParentSystemFunction = ServiceManager.SourceServiceSystemFunctionId, ResourcePool = "Reception" },
                FunctionDefinitions = new List<FunctionDefinition> { FunctionDefinition.DummyFunctionDefinition() },
                IsDefault = true,
                IsSourceOnly = false,
                IsMcrOnly = false,
                IsIntegrationOnly = false,
				Description = String.Empty,
				Diagram = new Graph
                {
					Nodes = new List<Node>
                    {
                        new Node
                        {
                            Label = "Dummy",
                            Position = new Position(0, 0),
                            ID = 1,
                            Configuration = new NodeConfiguration
                            {
                                FunctionID = Guid.Empty
                            }
                        }
                    },
					Edges = new List<Edge>()
                }
            };
        }

        public static ServiceDefinition GenerateDummyUnknownReceptionServiceDefinition()
        {
            return new ServiceDefinition(VirtualPlatform.ReceptionUnknown)
            {
                Name = "Unknown Reception",
                Id = Guid.Empty,
                ContributingConfig = new ContributingConfig { ParentSystemFunction = ServiceManager.SourceServiceSystemFunctionId, ResourcePool = "Reception" },
                FunctionDefinitions = new List<FunctionDefinition> { FunctionDefinition.DummyFunctionDefinition() },
                IsDefault = true,
                IsSourceOnly = false,
                IsMcrOnly = false,
                IsIntegrationOnly = false,
				Description = String.Empty,
				Diagram = new Graph
				{
					Nodes = new List<Node>
                    {
                        new Node
                        {
                            Label = "Dummy",
                            Position = new Position(0, 0),
                            ID = 1,
                            Configuration = new NodeConfiguration
                            {
                                FunctionID = Guid.Empty
                            }
                        }
                    },
					Edges = new List<Edge>()
				}
			};
        }

        public static ServiceDefinition GenerateDummyTransmissionServiceDefinition()
        {
			return new ServiceDefinition(VirtualPlatform.TransmissionNone)
            {
                Name = "Dummy Transmission",
                Id = Guid.Empty,
                ContributingConfig = new ContributingConfig { ParentSystemFunction = ServiceManager.TransmissionServiceSystemFunctionId, ResourcePool = "Transmission" },
                FunctionDefinitions = new List<FunctionDefinition>(),
                IsDefault = true,
                IsSourceOnly = false,
                IsMcrOnly = false,
                IsIntegrationOnly = false,
				Description = String.Empty,
				Diagram = new Graph
				{
					Nodes = new List<Node>(),
					Edges = new List<Edge>()
				}
			};
        }

        public static ServiceDefinition GenerateEurovisionReceptionServiceDefinition()
        {
            return new ServiceDefinition(VirtualPlatform.ReceptionEurovision)
            {
                Name = "Eurovision Reception",
                Id = Guid.Empty,
                ContributingConfig = new ContributingConfig { ParentSystemFunction = ServiceManager.SourceServiceSystemFunctionId, ResourcePool = "Reception" },
                FunctionDefinitions = new List<FunctionDefinition> { FunctionDefinition.DummyFunctionDefinition() },
                IsDefault = true,
                IsSourceOnly = false,
                IsMcrOnly = false,
                IsIntegrationOnly = false,
				Description = String.Empty,
				Diagram = new Graph
				{
                    Nodes = new List<Node>
                    {
                        new Node
                        {
                            Label = "Dummy",
                            Position = new Position(0, 0),
                            ID = 1,
                            Configuration = new NodeConfiguration
                            {
                                FunctionID = Guid.Empty
                            }
                        }
                    },
                    Edges = new List<Edge>()
				}
			};
        }

        public static ServiceDefinition GenerateEurovisionTransmissionServiceDefinition()
        {
            return new ServiceDefinition(VirtualPlatform.TransmissionEurovision)
            {
                Name = "Eurovision Transmission",
                Id = Guid.Empty,
                ContributingConfig = new ContributingConfig { ParentSystemFunction = ServiceManager.TransmissionServiceSystemFunctionId, ResourcePool = "Transmission" },
                FunctionDefinitions = new List<FunctionDefinition>(),
                IsDefault = true,
                IsSourceOnly = false,
                IsMcrOnly = false,
                IsIntegrationOnly = false,
				Description = String.Empty,
				Diagram = new Graph
				{
					Nodes = new List<Node>(),
					Edges = new List<Edge>()
				}
			};
        }

        public bool IsValid(Helpers helpers)
        {
            bool isValid = true;

            isValid &= Id != Guid.Empty;

            isValid &= Diagram != null && Diagram.IsValid(helpers);

            return isValid;
        }

        public override string ToString()
        {
            if (Diagram == null) return "Diagram is null";

            var sb = new StringBuilder($"Definition {Id}: ");
            foreach (var node in Diagram.Nodes)
            {
                var parentEdge = Diagram.Edges.SingleOrDefault(e => e.ToNode.ID == node.ID);
                var parentNode = parentEdge != null ? parentEdge.FromNodeID.ToString() : string.Empty;

                var childEdges = Diagram.Edges.Where(e => e.FromNode.ID == node.ID);
                var childNodes = string.Join(",", childEdges.Select(e => e.ToNode.ID));

                var nodeInfo = $"Node {node.Label} {node.ID}  (parent={parentNode}, children={childNodes})";

                sb.Append($"{nodeInfo} ; ");
            }

            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            ServiceDefinition other = obj as ServiceDefinition;
            if (other == null) return false;
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Gets the 0-based position of the function in the service definition.
        /// </summary>
        public int GetFunctionPosition(Function function)
        {
            return Diagram.GetFunctionPosition(function);
        }

        public bool FunctionIsFirst(Function function)
        {
            return Diagram.GetFunctionPosition(function) == 0;
        }

        public bool FunctionIsFirst(string functionLabel)
        {
            return Diagram.GetFunctionPosition(functionLabel) == 0;
        }

        public bool FunctionIsLast(string functionLabel)
        {
            return Diagram.GetFunctionPosition(functionLabel) == Diagram.Nodes.Count - 1;
        }

        public bool FunctionIsLast(Function function)
        {
            return Diagram.GetFunctionPosition(function) == Diagram.Nodes.Count - 1;
        }

        public bool FunctionsAreConnected(Function FunctionA, Function FunctionB)
        {
            if (FunctionA == null || FunctionB == null) return false;

            var functionPositionA = Diagram.GetFunctionPosition(FunctionA);
            var functionPositionB = Diagram.GetFunctionPosition(FunctionB);

            return Math.Abs(functionPositionA - functionPositionB) == 1;
        }

		public object Clone()
		{
			return new ServiceDefinition(this);
		}
	}
}