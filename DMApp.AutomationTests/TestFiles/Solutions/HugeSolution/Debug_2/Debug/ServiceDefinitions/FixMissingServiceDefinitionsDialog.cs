namespace Debug_2.Debug.ServiceDefinitions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.ServiceManager.Objects;
	using SrmServiceDefinition = Skyline.DataMiner.Net.ServiceManager.Objects.ServiceDefinition;

	public class FixMissingServiceDefinitionsDialog : DebugDialog
	{
		private MisingServiceDefinitionSection section;
		private int missingServiceDefinitions = 0;
		private readonly Button findReservationsButton = new Button("Find Reservations without Service Definition");
		private bool initialized = false;

		public FixMissingServiceDefinitionsDialog(Helpers helpers) : base(helpers)
		{
			Title = "Fix Missing Service Definitions";

			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			findReservationsButton.Pressed += (s, e) =>
			{
				FindReservationsWithoutServiceDefinitions();
				GenerateUi();
			};
		}

		private void FindReservationsWithoutServiceDefinitions()
		{
			var allServiceDefinitionIds = SrmManagers.ServiceManager.GetServiceDefinitions(ServiceDefinitionExposers.ID.NotEqual(Guid.Empty)).Select(x => x.ID).ToList();

			List<FilterElement<ReservationInstance>> filters = new List<FilterElement<ReservationInstance>>
			{
				//ReservationInstanceExposers.Start.GreaterThan(new DateTime(2023, 1, 1))
			};

			filters.AddRange(allServiceDefinitionIds.Select(x => ServiceReservationInstanceExposers.ServiceDefinitionID.NotEqual(x)));

			FilterElement<ReservationInstance> filter = new ANDFilterElement<ReservationInstance>(filters.ToArray());

			var affectedReservations = SrmManagers.ResourceManager.GetReservationInstances(filter).Cast<ServiceReservationInstance>();

			Dictionary<Guid, List<ServiceReservationInstance>> groupedReservations = new Dictionary<Guid, List<ServiceReservationInstance>>();
			foreach (var affectedReservation in affectedReservations)
			{
				if (groupedReservations.TryGetValue(affectedReservation.ServiceDefinitionID, out List<ServiceReservationInstance> reservations))
				{
					reservations.Add(affectedReservation);
				}
				else
				{
					groupedReservations.Add(affectedReservation.ServiceDefinitionID, new List<ServiceReservationInstance> { affectedReservation });
				}
			}

			missingServiceDefinitions = groupedReservations.Count;
			section = null;
			initialized = true;

			if (!groupedReservations.Any()) return;

			var mostInfluentialServiceDefinition = groupedReservations.OrderByDescending(x => x.Value.Count).FirstOrDefault();
			section = new MisingServiceDefinitionSection(helpers, mostInfluentialServiceDefinition.Key, mostInfluentialServiceDefinition.Value);
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0);

			AddWidget(new Label("Find Reservations Without Valid Service Definition") { Style = TextStyle.Heading }, ++row, 0, 1, 2);
			AddWidget(new Label("Searches for Reservations that either don't have a Service Definition assigned to them or for which the Service Definition can not be retrieved."), ++row, 0, 1, 2);

			AddWidget(findReservationsButton, ++row, 0, 1, 2);

			if (initialized)
			{
				if (section == null)
				{
					AddWidget(new Label("No invalid reservations found"), ++row, 0, 1, 2);
				}
				else
				{
					AddWidget(new Label("Missing Service Definitions:"), ++row, 0);
					AddWidget(new Label(missingServiceDefinitions.ToString()), row, 1, horizontalAlignment: HorizontalAlignment.Right);

					AddWidget(new WhiteSpace(), ++row, 0);

					AddSection(section, new SectionLayout(++row, 0));
					row += section.RowCount;
					AddWidget(new WhiteSpace(), ++row, 0);
				}
			}
		}

		private class MisingServiceDefinitionSection : Section
		{
			private readonly Helpers helpers;
			private readonly Guid id;
			private readonly ICollection<ServiceReservationInstance> affectedReservationInstances;
			private readonly TextBox serviceDefinitionTextBox = new TextBox() { IsMultiline = true, Height = 400 };
			private readonly TextBox uploadServiceDefinitionTextBox = new TextBox() { IsMultiline = true, Height = 200 };

			private ConnectionsSection connectionsSection;
			private Dictionary<int, ServiceConfiguration> serviceConfigurations = new Dictionary<int, ServiceConfiguration>();
			private List<FixedNode> fixedNodes;

			public readonly Button UploadButton = new Button("Upload Service Definition");
			public readonly Button GenerateServiceDefinitionButton = new Button("Generate Service Definition");
			public readonly Button DeleteServiceDefinitionButton = new Button("Delete Service Definition");

			public MisingServiceDefinitionSection(Helpers helpers, Guid id, ICollection<ServiceReservationInstance> affectedReservationInstances)
			{
				this.helpers = helpers;
				this.id = id;
				this.affectedReservationInstances = affectedReservationInstances;

				Initialize();
				GenerateUi();
			}

			public Guid ID => id;

			public int AffectedReservationsCount => affectedReservationInstances.Count;

			public int NodeCount => serviceConfigurations.Count;

			private void Initialize()
			{
				serviceConfigurations = GetMostCompleteServiceConfiguration();
				if (serviceConfigurations == null) return;

				fixedNodes = ConvertToFixedNodes(serviceConfigurations);

				connectionsSection = new ConnectionsSection(fixedNodes);

				GenerateServiceDefinitionButton.Pressed += (s, e) => GenerateServiceDefinition();

				UploadButton.Pressed += (s, e) => UploadServiceDefinition();

				DeleteServiceDefinitionButton.Pressed += (s, e) => DeleteServiceDefinition();
			}

			private void DeleteServiceDefinition()
			{
				try
				{
					var srmServiceDefinition = SrmManagers.ServiceManager.GetServiceDefinition(id);
					SrmManagers.ServiceManager.RemoveServiceDefinitions(out string error, srmServiceDefinition);

					if (!String.IsNullOrWhiteSpace(error))
					{
						uploadServiceDefinitionTextBox.Text = $"Unable to delete service definition due to {error}";
					}
					else
					{
						uploadServiceDefinitionTextBox.Text = $"Deleting service definition {id} was successful";
					}
				}
				catch (Exception e)
				{
					uploadServiceDefinitionTextBox.Text = $"Unable to delete service definition due to {e}";
				}
			}

			private void UploadServiceDefinition()
			{
				if (String.IsNullOrWhiteSpace(serviceDefinitionTextBox.Text)) return;
				try
				{
					var serviceDefinition = JsonConvert.DeserializeObject<SrmServiceDefinition>(serviceDefinitionTextBox.Text);
					SrmManagers.ServiceManager.AddOrUpdateServiceDefinition(serviceDefinition);
					uploadServiceDefinitionTextBox.Text = $"Uploading service definition {serviceDefinition.ID} was successful";
				}
				catch (Exception e)
				{
					uploadServiceDefinitionTextBox.Text = $"Unable to upload service definition due to {e}";
				}
			}

			private void GenerateServiceDefinition()
			{
				var serviceDefinition = new SrmServiceDefinition(id)
				{
					Name = id.ToString(),
					IsTemplate = true,
					Description = String.Empty,
					Properties = new List<Property>
					{
						new Property("Virtual Platform", "Order")
					},
					Scripts = new List<Skyline.DataMiner.Net.Profiles.ScriptEntry>
					{
						new Skyline.DataMiner.Net.Profiles.ScriptEntry
						{
							Name = "START",
							Description = null,
							Script = "HandleOrderAction"
						},
						new Skyline.DataMiner.Net.Profiles.ScriptEntry
						{
							Name = "STOP",
							Description = null,
							Script = "HandleOrderAction"
						},
					},
					Diagram = new Graph
					{
						Nodes = new List<Node>(),
						Edges = new List<Edge>()
					}
				};

				DeterminePositions();

				foreach (var node in fixedNodes)
				{
					string resourcePool;
					SystemFunctionDefinition systemFunctionDefinition; 

					bool isDummyReception = node.ServiceDefinitionId == Guid.Empty;
					if (isDummyReception)
					{
						var randomReceptionServiceDefinition = SrmManagers.ServiceManager.GetServiceDefinition(ServiceDefinitionGuids.FixedLineYleHelsinkiReceptionServiceDefinitionId) ?? throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ServiceDefinition.ServiceDefinitionNotFoundException($"Unable to find service definition {ServiceDefinitionGuids.FixedLineYleHelsinkiReceptionServiceDefinitionId}");

						var contributingConfig = GetContributingConfig(randomReceptionServiceDefinition) ?? throw new InvalidOperationException($"Unable to get contributing config for SD {randomReceptionServiceDefinition.ID}");

						systemFunctionDefinition = SrmManagers.ProtocolFunctionManager.GetFunctionDefinition(new Skyline.DataMiner.Net.FunctionDefinitionID(Guid.Parse(contributingConfig.ParentSystemFunction))) ?? throw new FunctionNotFoundException(contributingConfig.ParentSystemFunction);
						resourcePool = "not applicable";
					}
					else
					{
						var nodeServiceDefinition = SrmManagers.ServiceManager.GetServiceDefinition(node.ServiceDefinitionId) ?? throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ServiceDefinition.ServiceDefinitionNotFoundException($"Unable to find service definition {node.ServiceDefinitionId}");
						var contributingConfig = GetContributingConfig(nodeServiceDefinition) ?? throw new InvalidOperationException($"Unable to get contributing config for SD {nodeServiceDefinition.ID}");

						systemFunctionDefinition = SrmManagers.ProtocolFunctionManager.GetFunctionDefinition(new Skyline.DataMiner.Net.FunctionDefinitionID(Guid.Parse(contributingConfig.ParentSystemFunction))) ?? throw new FunctionNotFoundException(contributingConfig.ParentSystemFunction);

						resourcePool = contributingConfig.ResourcePool;
					}

					serviceDefinition.Diagram.Nodes.Add(new Node
					{
						ID = node.Id,
						Label = $"{systemFunctionDefinition.Name} [{node.Id}]",
						Position = new Position(node.Row, node.Column),
						Properties = new[]
						{
							new Property { Name = "Options", Value = "Optional" },
							new Property { Name = "Resource Pool", Value = resourcePool },
							new Property { Name = "IsContributing", Value = "TRUE" },
							new Property { Name= "IsProfileInstanceOptional", Value = "TRUE" }
						}.ToList(),
						Configuration = new NodeConfiguration
						{
							FunctionID = systemFunctionDefinition.GUID,
						},
						InterfaceConfigurations = GetNodeInterfaceConfiguration(systemFunctionDefinition).ToArray()
					});
				}

				serviceDefinition.Diagram.Edges = GetEdges(serviceDefinition.Diagram.Nodes).ToList();

				serviceDefinitionTextBox.Text = JsonConvert.SerializeObject(serviceDefinition, Formatting.Indented);
			}

			private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ContributingConfig GetContributingConfig(SrmServiceDefinition serviceDefinition)
			{
				Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ContributingConfig contributingConfig = null;
				try
				{
					var contributingConfigProperty = serviceDefinition.Properties.FirstOrDefault(p => String.Equals(p.Name, "Contributing Config", StringComparison.InvariantCultureIgnoreCase));

					contributingConfig = JsonConvert.DeserializeObject<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ContributingConfig>(contributingConfigProperty.Value);
				}
				catch (Exception)
				{
				}

				return contributingConfig;
			}

			private IEnumerable<InterfaceConfiguration> GetNodeInterfaceConfiguration(Skyline.DataMiner.Net.Messages.SystemFunctionDefinition functionDefinition)
			{
				var nodeInterfaces = new List<InterfaceConfiguration>();

				foreach (var inputInterface in functionDefinition.InputInterfaces)
				{
					nodeInterfaces.Add(new InterfaceConfiguration
					{
						ID = inputInterface.Id,
						Type = InterfaceType.In,
						ProfileDefinitionID = inputInterface.ProfileDefinition,
						Properties = new List<Property>
						{
							new Property { Name= "IsProfileInstanceOptional", Value = "TRUE" }
						}
					});
				}

				foreach (var outputInterface in functionDefinition.OutputInterfaces)
				{
					nodeInterfaces.Add(new InterfaceConfiguration
					{
						ID = outputInterface.Id,
						Type = InterfaceType.Out,
						ProfileDefinitionID = outputInterface.ProfileDefinition,
						Properties = new List<Property>
						{
							new Property { Name= "IsProfileInstanceOptional", Value = "TRUE" }
						}
					});
				}

				foreach (var inputOutputInterface in functionDefinition.InputOutputInterfaces)
				{
					nodeInterfaces.Add(new InterfaceConfiguration
					{
						ID = inputOutputInterface.Id,
						Type = InterfaceType.InOut,
						ProfileDefinitionID = inputOutputInterface.ProfileDefinition,
						Properties = new List<Property>
						{
							new Property { Name= "IsProfileInstanceOptional", Value = "TRUE" }
						}
					});
				}

				return nodeInterfaces;
			}

			private void DeterminePositions()
			{
				var edges = connectionsSection.Edges;

				// Get source connections
				var sourceNodes = fixedNodes.Where(x => !x.HasInputInfo && x.HasOutputInfo);

				int row = 0;
				int column = 0;
				SetPositions(edges, sourceNodes, ref row, column);
			}

			private void SetPositions(IEnumerable<FixedEdge> edges, IEnumerable<FixedNode> nodes, ref int row, int column)
			{
				if (!nodes.Any())
				{
					row += 1;
					return;
				}

				foreach (var node in nodes)
				{
					node.Row = row;
					node.Column = column;

					var childNodes = edges.Where(x => x.From.Equals(node)).Select(x => x.To).ToList();

					SetPositions(edges, childNodes, ref row, column + 1);
				}
			}

			private IEnumerable<Edge> GetEdges(IEnumerable<Node> configuredNodes)
			{
				List<Edge> edges = new List<Edge>();
				foreach (var fixedEdge in connectionsSection.Edges)
				{
					var outputNode = configuredNodes.FirstOrDefault(x => x.ID.Equals(fixedEdge.From.Id));
					var inputNode = configuredNodes.FirstOrDefault(x => x.ID.Equals(fixedEdge.To.Id));

					edges.Add(new Edge
					{
						FromNodeID = fixedEdge.From.Id,
						FromNodeInterfaceID = outputNode.InterfaceConfigurations.FirstOrDefault(x => x.Type == InterfaceType.Out).ID,
						ToNodeID = fixedEdge.To.Id,
						ToNodeInterfaceID = inputNode.InterfaceConfigurations.FirstOrDefault(x => x.Type == InterfaceType.In).ID,
					});
				}

				return edges;
			}

			private Dictionary<int, ServiceConfiguration> GetMostCompleteServiceConfiguration()
			{
				int maxAssignedResources = -1;
				Dictionary<int, ServiceConfiguration> serviceConfigurations = null;
				ReservationInstance orderReservation = null;
				foreach (var reservation in affectedReservationInstances)
				{
					if (!helpers.OrderManagerElement.TryGetServiceConfigurations(reservation.ID, out var configs)) continue;

					int assignedResources = CountAssignedResources(configs);
					if (maxAssignedResources < assignedResources)
					{
						maxAssignedResources = assignedResources;
						serviceConfigurations = configs;
						orderReservation = reservation;
					}
				}

				helpers.Log(nameof(FixMissingServiceDefinitionsDialog), nameof(GetMostCompleteServiceConfiguration), $"Found service configs for order {orderReservation.Name} ({orderReservation.ID})");

				return serviceConfigurations;
			}

			private int CountAssignedResources(Dictionary<int, ServiceConfiguration> serviceConfigurations)
			{
				int count = 0;
				foreach (var kvp in serviceConfigurations)
				{
					foreach (var function in kvp.Value.Functions)
					{
						if (String.IsNullOrEmpty(function.Value.ResourceName) || function.Value.ResourceName.Equals("None")) continue;
						if (function.Value.ResourceId.Equals(Guid.Empty)) continue;

						count += 1;
					}
				}

				return count;
			}

			private List<FixedNode> ConvertToFixedNodes(Dictionary<int, ServiceConfiguration> serviceConfigurations)
			{
				if (serviceConfigurations.Values.Any(x => x.Name.Contains("VIZREM")))
				{
					return ConvertVizremConfigurationToFixedNodes(serviceConfigurations);
				}
				else
				{
					return ConvertLiveVideoConfigurationToFixedNodes(serviceConfigurations);
				}
			}

			private List<FixedNode> ConvertLiveVideoConfigurationToFixedNodes(Dictionary<int, ServiceConfiguration> serviceConfigurations)
			{
				List<FixedNode> fixedNodes = new List<FixedNode>();
				var matrixInputFunctionGuids = FunctionGuids.AllMatrixInputGuids.ToList();
				var matrixOutputFunctionGuids = FunctionGuids.AllMatrixOutputGuids.ToList();

				foreach (var kvp in serviceConfigurations)
				{			
					FixedNode node = new FixedNode
					{
						Id = kvp.Key,
						DisplayName = kvp.Value.Name,
						ServiceDefinitionId = kvp.Value.ServiceDefinitionId,
						IntegrationType = kvp.Value.IntegrationType
					};

					var functionIds = kvp.Value.Functions.Select(x => x.Value.Id).ToList();

					bool hasRoutingInputFunction = functionIds.Any(x => matrixInputFunctionGuids.Contains(x)) || kvp.Value.Functions.Values.Any(x => x.Name.Contains("Input"));
					bool hasRoutingOutputFunction = functionIds.Any(x => matrixOutputFunctionGuids.Contains(x)) || kvp.Value.Functions.Values.Any(x => x.Name.Contains("Output"));

					if (kvp.Value.Name.Contains("Reception"))
					{
						// Input service
						var outputFunction = kvp.Value.Functions.Values.LastOrDefault();
						node.OutputFunctionName = outputFunction?.Name ?? "dummy source output function";
						node.OutputResource = outputFunction?.ResourceName ?? "dummy source output resource";
					}
					else if (hasRoutingInputFunction && hasRoutingOutputFunction)
					{
						// Routing service
						var inputFunction = kvp.Value.Functions.Values.FirstOrDefault();
						node.InputFunctionName = inputFunction?.Name ?? "dummy routing input function";
						node.InputResource = inputFunction?.ResourceName ?? "dummy routing input resource";

						var outputFunction = kvp.Value.Functions.Values.LastOrDefault();
						node.OutputFunctionName = outputFunction?.Name ?? "dummy routing output function";
						node.OutputResource = outputFunction?.ResourceName ?? "dummy routing output resource";
					}
					else if (kvp.Value.Name.Contains("Destination") && kvp.Value.IntegrationType == Skyline.DataMiner.Utils.YLE.Integrations.IntegrationType.Plasma)
					{
						// Plasma Destination service -> could be used as input for recordings after destinations
						var inputFunction = kvp.Value.Functions.Values.FirstOrDefault();
						node.InputFunctionName = inputFunction?.Name ?? "dummy plasma destination input function";
						node.InputResource = inputFunction?.ResourceName ?? "dummy plasma destination input resource";

						var outputFunction = kvp.Value.Functions.Values.LastOrDefault();
						node.OutputFunctionName = outputFunction?.Name ?? "dummy plasma destination output function";
						node.OutputResource = outputFunction?.ResourceName ?? "dummy plasma destination output resource";
					}
					else if (kvp.Value.Name.Contains("Processing"))
					{
						// Processing service should be handled as routing
						var inputFunction = kvp.Value.Functions.Values.FirstOrDefault();
						node.InputFunctionName = inputFunction?.Name ?? "dummy processing input function";
						node.InputResource = inputFunction?.ResourceName ?? "dummy processing input resource";

						var outputFunction = kvp.Value.Functions.Values.LastOrDefault();
						node.OutputFunctionName = outputFunction?.Name ?? "dummy processing output function";
						node.OutputResource = outputFunction?.ResourceName ?? "dummy processing output resource";
					}
					else
					{
						// Output service
						var inputFunction = kvp.Value.Functions.Values.FirstOrDefault();
						node.InputFunctionName = inputFunction?.Name ?? "dummy destination/transmission/recording input function";
						node.InputResource = inputFunction?.ResourceName ?? "dummy destination/transmission/recording input resource";
					}

					fixedNodes.Add(node);
				}

				return fixedNodes;
			}

			private List<FixedNode> ConvertVizremConfigurationToFixedNodes(Dictionary<int, ServiceConfiguration> serviceConfigurations)
			{
				List<FixedNode> fixedNodes = new List<FixedNode>();
				foreach(var kvp in serviceConfigurations)
				{
					if (kvp.Key == 1)
					{
						// First node
						fixedNodes.Add(new FixedNode
						{
							Id = kvp.Key,
							DisplayName = kvp.Value.Name,
							ServiceDefinitionId = kvp.Value.ServiceDefinitionId,
							IntegrationType = kvp.Value.IntegrationType,
							OutputFunctionName = kvp.Value.Functions.Last().Value.Name,
							OutputResource = kvp.Value.Functions.Last().Value.ResourceName
						});
					}
					else if (kvp.Key == serviceConfigurations.Last().Key)
					{
						// Last node
						fixedNodes.Add(new FixedNode
						{
							Id = kvp.Key,
							DisplayName = kvp.Value.Name,
							ServiceDefinitionId = kvp.Value.ServiceDefinitionId,
							IntegrationType = kvp.Value.IntegrationType,
							InputFunctionName = kvp.Value.Functions.First().Value.Name,
							InputResource = kvp.Value.Functions.First().Value.ResourceName
						});
					}
					else
					{
						fixedNodes.Add(new FixedNode
						{
							Id = kvp.Key,
							DisplayName = kvp.Value.Name,
							ServiceDefinitionId = kvp.Value.ServiceDefinitionId,
							IntegrationType = kvp.Value.IntegrationType,
							InputFunctionName = kvp.Value.Functions.First().Value.Name,
							InputResource = kvp.Value.Functions.First().Value.ResourceName,
							OutputFunctionName = kvp.Value.Functions.Last().Value.Name,
							OutputResource = kvp.Value.Functions.Last().Value.ResourceName
						});
					}
				}

				return fixedNodes;
			}

			private void GenerateUi()
			{
				Clear();

				int row = -1;

				AddWidget(new Label(id.ToString()) { Style = TextStyle.Heading }, ++row, 0, 1, 2);

				AddWidget(new Label("Affected Reservations"), ++row, 0);
				AddWidget(new Label(AffectedReservationsCount.ToString()), row, 1, horizontalAlignment: HorizontalAlignment.Right);

				AddWidget(new TextBox(String.Join(Environment.NewLine.ToString(), affectedReservationInstances.OrderByDescending(x => x.Start).Select(x => $"{x.Name} [{x.ID}] ({x.Start} - {x.End})"))) { IsMultiline = true, Height = 200 }, ++row, 0, 1, 2);

				if (serviceConfigurations == null)
				{
					AddWidget(new Label("No Valid Service Configuration Available"), ++row, 0, 1, 2);
				}
				else
				{
					AddWidget(new Label("Amount of Nodes"), ++row, 0);
					AddWidget(new Label(NodeCount.ToString()), row, 1, horizontalAlignment: HorizontalAlignment.Right);

					AddWidget(new Label("Valid Service Configuration"), ++row, 0, 1, 2);
					AddWidget(new TextBox(JsonConvert.SerializeObject(serviceConfigurations, Formatting.Indented)) { Height = 400, IsMultiline = true }, ++row, 0, 1, 2);

					AddSection(connectionsSection, new SectionLayout(++row, 0));
					row += connectionsSection.RowCount;

					AddWidget(GenerateServiceDefinitionButton, ++row, 0, 1, 2);
					AddWidget(serviceDefinitionTextBox, ++row, 0, 1, 2);
					AddWidget(new WhiteSpace(), ++row, 0);
					AddWidget(UploadButton, ++row, 0, 1, 2);
					AddWidget(DeleteServiceDefinitionButton, ++row, 0, 1, 2);
					AddWidget(uploadServiceDefinitionTextBox, ++row, 0, 1, 2);
				}
			}
		}

		private class ConnectionsSection : Section
		{
			private readonly Dictionary<string, FixedNode> inputNodes = new Dictionary<string, FixedNode>();
			private readonly Dictionary<string, FixedNode> outputNodes = new Dictionary<string, FixedNode>();
			private readonly List<Tuple<DropDown, Label>> connectionDropDowns = new List<Tuple<DropDown, Label>>();

			public ConnectionsSection(ICollection<FixedNode> nodes)
			{
				Initialize(nodes);
				GenerateUi();
			}

			public ICollection<FixedEdge> Edges
			{
				get
				{
					List<FixedEdge> edges = new List<FixedEdge>();
					foreach (var tuple in connectionDropDowns)
					{
						edges.Add(new FixedEdge
						{
							From = outputNodes[tuple.Item1.Selected],
							To = inputNodes[tuple.Item2.Text]
						});
					}

					return edges;
				}
			}

			private void Initialize(ICollection<FixedNode> nodes)
			{
				foreach (var node in nodes)
				{
					if (node.HasInputInfo) inputNodes.Add($"Node {node.Id} ({node.DisplayName}) [{node.InputResource}] {(node.IntegrationType == Skyline.DataMiner.Utils.YLE.Integrations.IntegrationType.None ? String.Empty : node.IntegrationType.ToString())}", node);
					if (node.HasOutputInfo) outputNodes.Add($"Node {node.Id} ({node.DisplayName}) [{node.OutputResource}] {(node.IntegrationType == Skyline.DataMiner.Utils.YLE.Integrations.IntegrationType.None ? String.Empty : node.IntegrationType.ToString())}", node);
				}

				foreach (var inputNode in inputNodes)
				{
					DropDown outputDropDown = new DropDown(outputNodes.Keys.Where(x => outputNodes[x] != inputNode.Value)); // Node cannot be connected to itself
					Label label = new Label(inputNode.Key);
					connectionDropDowns.Add(new Tuple<DropDown, Label>(outputDropDown, label));
				}
			}

			private void GenerateUi()
			{
				Clear();

				int row = -1;

				AddWidget(new Label("Connections") { Style = TextStyle.Heading }, ++row, 0, 1, 2);
				for (int i = 0; i < connectionDropDowns.Count; i++)
				{
					AddWidget(new Label($"Connection {i + 1}"), ++row, 0, 1, 2);
					AddWidget(new Label("From"), ++row, 0);
					AddWidget(connectionDropDowns[i].Item1, row, 1);
					AddWidget(new Label("To"), ++row, 0);
					AddWidget(connectionDropDowns[i].Item2, row, 1);
					AddWidget(new WhiteSpace(), ++row, 0);
				}
			}
		}

		private class FixedNode
		{
			public int Id { get; set; } = 0;

			public string DisplayName { get; set; }

			public string InputFunctionName { get; set; } = String.Empty;

			public string InputResource { get; set; } = null;

			public string OutputFunctionName { get; set; } = String.Empty;

			public string OutputResource { get; set; } = null;

			public Guid ServiceDefinitionId { get; set; } = Guid.Empty;

			public int Column { get; set; } = 0;

			public int Row { get; set; } = 0;

			public Skyline.DataMiner.Utils.YLE.Integrations.IntegrationType IntegrationType { get; set; } = Skyline.DataMiner.Utils.YLE.Integrations.IntegrationType.None;

			public bool HasInputInfo => !String.IsNullOrEmpty(InputFunctionName) && !String.IsNullOrEmpty(InputResource);

			public bool HasOutputInfo => !String.IsNullOrEmpty(OutputFunctionName) && !String.IsNullOrEmpty(OutputResource);

			public override bool Equals(object obj)
			{
				if (!(obj is FixedNode otherNode)) return false;
				return Id.Equals(otherNode.Id);
			}

			public override int GetHashCode()
			{
				return Id.GetHashCode();
			}
		}

		private class FixedEdge
		{
			public FixedNode From { get; set; }

			public FixedNode To { get; set; }
		}
	}
}
