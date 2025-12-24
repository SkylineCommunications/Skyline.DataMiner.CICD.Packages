namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Reservations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Debug_2.Debug.Reservations;
	using Skyline.DataMiner.Library.Solutions.SRM.Model;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.AssignProfilesAndResources;
	using Skyline.DataMiner.Net.History;
	using Skyline.DataMiner.Net.History.ReservationInstances;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.SRM.Capabilities;
	using SLDataGateway.API.Querying;
	using Parameter = Library.Solutions.SRM.Model.Parameter;

	public class FindQuarantinedOrdersDialog : Dialog
	{
		private readonly Helpers helpers;

		private readonly Label findQuarantinedOrdersLabel = new Label("Find Quarantined Orders") { Style = TextStyle.Heading };
		private readonly Label noQuarantinedOrdersFoundLabel = new Label("No Quarantined Orders Found");
		private readonly Label sortByLabel = new Label("Sort By");
		private readonly List<QuarantinedReservationSection> quarantinedReservationSections = new List<QuarantinedReservationSection>();

		private bool findQuarantinedOrdersButtonWasPressed;

		public FindQuarantinedOrdersDialog(Helpers helpers) : base(helpers.Engine)
		{
			Title = "Find Quarantined Orders";

			this.helpers = helpers;

			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			FindQuarantinedOrdersButton.Pressed += FindQuarantinedReservationsButton_Pressed;
		}

		private void FindQuarantinedReservationsButton_Pressed(object sender, EventArgs e)
		{
			findQuarantinedOrdersButtonWasPressed = true;

			FilterElement<ReservationInstance> filter = new ANDFilterElement<ReservationInstance>(ReservationInstanceExposers.IsQuarantined.Equal(true))
				.AND(ReservationInstanceExposers.Properties.DictStringField("Booking Manager").Equal("Order Booking Manager"));

			if (!IncludeOrdersInThePastCheckBox.IsChecked)
			{
				filter = filter.AND(ReservationInstanceExposers.End.GreaterThanOrEqual(DateTime.Now));
			}

			var quarantinedReservations = SrmManagers.ResourceManager.GetReservationInstances(filter);

			quarantinedReservationSections.Clear();
			ReservationInstance[] orderedQuarantinedReservations;
			switch (SortByRadioButtonList.Selected)
			{
				case DateString:
					orderedQuarantinedReservations = quarantinedReservations.OrderBy(x => x.Start).ToArray();
					break;
				case NameString:
					orderedQuarantinedReservations = quarantinedReservations.OrderBy(x => x.Name).ToArray();
					break;
				default:
					orderedQuarantinedReservations = quarantinedReservations; // No sorting specified
					break;
			}

			foreach (var reservation in orderedQuarantinedReservations)
			{
				QuarantinedReservationSection quarantinedReservationSection = new QuarantinedReservationSection(helpers, reservation);
				quarantinedReservationSection.OnUiUpdateRequired += (s, args) => GenerateUi();
				quarantinedReservationSections.Add(quarantinedReservationSection);
			}

			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 5);

			AddWidget(findQuarantinedOrdersLabel, ++row, 0, 1, 5);

			AddWidget(IncludeOrdersInThePastCheckBox, ++row, 0, 1, 5);

			AddWidget(sortByLabel, ++row, 0, 1, 2, HorizontalAlignment.Left, VerticalAlignment.Top);
			AddWidget(SortByRadioButtonList, row, 2, 1, 3);

			AddWidget(FindQuarantinedOrdersButton, ++row, 0, 1, 5);

			AddWidget(new WhiteSpace(), ++row, 1);

			if (findQuarantinedOrdersButtonWasPressed && !quarantinedReservationSections.Any())
			{
				AddWidget(noQuarantinedOrdersFoundLabel, ++row, 0, 1, 5);
			}
			else
			{
				foreach (var section in quarantinedReservationSections)
				{
					AddSection(section, ++row, 0);
					row += section.RowCount;

				}
			}

			SetColumnWidth(0, 45);
			SetColumnWidth(1, 45);
		}

		public Button BackButton { get; private set; } = new Button("Back...") { Width = 150 };

		public Button FindQuarantinedOrdersButton { get; private set; } = new Button("Find Quarantined Orders") { Width = 250 };

		public CheckBox IncludeOrdersInThePastCheckBox { get; private set; } = new CheckBox("Include Orders in the Past") { IsChecked = false };

		private const string DateString = "Date";
		private const string NameString = "Name";

		public RadioButtonList SortByRadioButtonList { get; private set; } = new RadioButtonList(new [] { DateString, NameString }) { Selected = DateString };

		private sealed class QuarantinedReservationSection : Section
		{
			private readonly Helpers helpers;
			private readonly List<HistorySection> reservationHistorySections = new List<HistorySection>();
			private readonly List<HistorySection> resourceHistorySections = new List<HistorySection>();
			private readonly List<QuarantinedReservationSection> quarantinedServiceSections = new List<QuarantinedReservationSection>();
			private readonly ResourceAssignmentSection resourceAssignmentSection;

			private readonly CollapseButton collapseButton = new CollapseButton { Width = 44, CollapseText = "-", ExpandText = "+", IsCollapsed = true };

			private readonly Label reservationIdTitle = new Label("ID");
			private readonly Label reservationNameTitle = new Label("Name");
			private readonly Label reservationStartTitle = new Label("Start");
			private readonly Label reservationEndTitle = new Label("End");
			private readonly Label quarantinedServicesTitle = new Label("Quarantined Services") { Style = TextStyle.Bold };

			private readonly Label reservationTitleLabel = new Label { Style = TextStyle.Heading };
			private readonly Label reservationNameLabel = new Label();
			private readonly Label reservationIdLabel = new Label();
			private readonly Label reservationStartLabel = new Label();
			private readonly Label reservationEndLabel = new Label();
			private readonly Label reservationStateLabel = new Label();
			private readonly Label reservationQuarantinedLabel = new Label();

			private readonly WhiteSpace whiteSpace1 = new WhiteSpace();
			private readonly WhiteSpace whiteSpace2 = new WhiteSpace();
			private readonly WhiteSpace whiteSpace3 = new WhiteSpace();

			private readonly Label noReservationHistoryAvailableLabel = new Label("No Reservation Instance History Available");
			private readonly Label noContributingResourceHistoryAvailableLabel = new Label("No Contributing Resource History Available");

			private readonly TextBox leaveQuarantinedStateExceptionTextBox = new TextBox { IsMultiline = true, Height = 300 };

			private readonly bool isOrder;

			private bool servicesHaveBeenRetrieved;

			public QuarantinedReservationSection(Helpers helpers, ReservationInstance reservationInstance)
			{
				this.helpers = helpers;
				ReservationInstance = reservationInstance;

				isOrder = reservationInstance.Properties.Any(x => x.Key.Equals("Booking Manager") && x.Value != null && x.Value.Equals("Order Booking Manager"));
				if (isOrder) reservationTitleLabel.Style = TextStyle.Bold;

				reservationTitleLabel.Text = $"{reservationInstance.Name} ({reservationInstance.Start.ToUniversalTime()} - {reservationInstance.End.ToUniversalTime()} [UTC])";
				reservationNameLabel.Text = reservationInstance.Name;
				reservationIdLabel.Text = reservationInstance.ID.ToString();
				reservationStartLabel.Text = $"{reservationInstance.Start.ToUniversalTime()} [UTC]";
				reservationEndLabel.Text = $"{reservationInstance.End.ToUniversalTime()} [UTC]";
				reservationStateLabel.Text = reservationInstance.Status.GetDescription();
				reservationQuarantinedLabel.Text = (reservationInstance.IsQuarantined) ? "Quarantined" : "Not Quarantined";


				if (!isOrder) resourceAssignmentSection = new ResourceAssignmentSection(helpers, reservationInstance as ServiceReservationInstance);

				Initialize();
				GenerateUi();
			}

			private void InitializeServiceSections()
			{
				string serializedConfig = helpers.OrderManagerElement.GetSerializedServiceConfigurations(ReservationInstance.ID);
				if (string.IsNullOrWhiteSpace(serializedConfig) || serializedConfig == "-1" || serializedConfig.Equals("Not found", StringComparison.InvariantCultureIgnoreCase)) return;

				var serviceConfigurations = JsonConvert.DeserializeObject<Dictionary<int, ServiceConfiguration>>(serializedConfig);

				quarantinedServiceSections.Clear();
				foreach (var serviceConfig in serviceConfigurations.Values)
				{
					var serviceReservation = helpers.ResourceManager.GetReservationInstance(serviceConfig.Id);
					if (!serviceReservation.IsQuarantined) continue;

					helpers.Log(nameof(QuarantinedReservationSection), nameof(InitializeServiceSections), "JsonConvert.SerializeObject(serviceReservation)");

					var quarantinedServiceSection = new QuarantinedReservationSection(helpers, serviceReservation);
					collapseButton.LinkedWidgets.Add(quarantinedServiceSection.collapseButton);
					collapseButton.LinkedWidgets.Add(quarantinedServiceSection.reservationTitleLabel);

					quarantinedServiceSection.OnUiUpdateRequired += (sender, args) =>
					{
						GenerateUi();
						OnUiUpdateRequired(this, new EventArgs());
					};

					quarantinedServiceSections.Add(quarantinedServiceSection);
				}
			}

			public event EventHandler OnUiUpdateRequired;

			private void Initialize()
			{
				collapseButton.LinkedWidgets.AddRange(new Widget[]
				{
					reservationIdTitle,
					reservationNameTitle,
					reservationNameLabel,
					reservationIdLabel,
					reservationStartTitle,
					reservationStartLabel,
					reservationEndTitle,
					reservationEndLabel,
					ReservationHistoryCollapseButton,
					ResourceHistoryCollapseButton,
					LeaveQuarantinedStateButton,
					whiteSpace1,
					whiteSpace2,
					whiteSpace3,
					quarantinedServicesTitle
				});

				if (!isOrder && resourceAssignmentSection != null)
				{
					collapseButton.LinkedWidgets.AddRange(resourceAssignmentSection.Widgets);
				}

				collapseButton.Collapse();
				collapseButton.Pressed += CollapseButton_Pressed;
				ReservationHistoryCollapseButton.Pressed += ReservationHistoryButton_Pressed;
				ResourceHistoryCollapseButton.Pressed += ResourceHistoryCollapseButton_Pressed;
				LeaveQuarantinedStateButton.Pressed += TryLeaveQuarantineButton_Pressed;
			}

			private void CollapseButton_Pressed(object sender, EventArgs e)
			{
				if (!isOrder || servicesHaveBeenRetrieved) return;

				InitializeServiceSections();
				servicesHaveBeenRetrieved = true;

				GenerateUi();
				OnUiUpdateRequired(this, new EventArgs());
			}

			private void ReservationHistoryButton_Pressed(object sender, EventArgs e)
			{
				if (ReservationInstance == null || ReservationHistoryCollapseButton.IsCollapsed) return;

				try
				{
					string subjectId = $"ReservationInstanceID_{ReservationInstance.ID}";
					var filter = new ANDFilterElement<HistoryChange>(HistoryChangeExposers.SubjectID.Equal(subjectId, StringComparison.OrdinalIgnoreCase));
					var response = (ManagerStorePagingResponse<HistoryChange>)helpers.Engine.SendSLNetSingleResponseMessage(new ManagerStoreStartPagingRequest<HistoryChange>(filter.ToQuery(), 100) { ExtraTypeIdentifier = "ReservationInstance" });

					ClearReservationHistorySections();
					foreach (var historyChange in response.Objects)
					{
						foreach (var change in historyChange.Changes)
						{
							var historySection = new HistorySection(change, historyChange.Time, historyChange.FullUsername, historyChange.DmaId, isOrder);
							ReservationHistoryCollapseButton.LinkedWidgets.AddRange(historySection.Widgets);
							reservationHistorySections.Add(historySection);
						}
					}

					ReservationHistoryCollapseButton.LinkedWidgets.Add(noReservationHistoryAvailableLabel);
				}
				catch (Exception exception)
				{
					helpers.Engine.Log($"{nameof(QuarantinedReservationSection)}|{nameof(ReservationHistoryButton_Pressed)}|{exception}");
				}

				GenerateUi();
				OnUiUpdateRequired(this, new EventArgs());
			}

			private void ResourceHistoryCollapseButton_Pressed(object sender, EventArgs e)
			{
				if (ReservationInstance == null || ResourceHistoryCollapseButton.IsCollapsed) return;

				try
				{
					string subjectId = $"ResourceID_{ReservationInstance.ID}";
					var filter = new ANDFilterElement<HistoryChange>(HistoryChangeExposers.SubjectID.Equal(subjectId, StringComparison.OrdinalIgnoreCase));
					var response = (ManagerStorePagingResponse<HistoryChange>)helpers.Engine.SendSLNetSingleResponseMessage(new ManagerStoreStartPagingRequest<HistoryChange>(filter.ToQuery(), 100) { ExtraTypeIdentifier = "Resource" });

					ClearResourceHistorySections();
					foreach (var historyChange in response.Objects)
					{
						helpers.Engine.Log(JsonConvert.SerializeObject(historyChange));
						foreach (var change in historyChange.Changes)
						{
							var historySection = new HistorySection(change, historyChange.Time, historyChange.FullUsername, historyChange.DmaId, isOrder);
							ResourceHistoryCollapseButton.LinkedWidgets.AddRange(historySection.Widgets);
							resourceHistorySections.Add(historySection);
						}
					}

					ResourceHistoryCollapseButton.LinkedWidgets.Add(noContributingResourceHistoryAvailableLabel);
				}
				catch (Exception exception)
				{
					helpers.Engine.Log($"{nameof(QuarantinedReservationSection)}|{nameof(ResourceHistoryCollapseButton_Pressed)}|{exception}");
				}

				GenerateUi();
				OnUiUpdateRequired(this, new EventArgs());
			}

			private void TryLeaveQuarantineButton_Pressed(object sender, EventArgs e)
			{
				if (!ReservationInstance.Properties.Dictionary.TryGetValue("Booking Manager", out object bookingManagerElementName)) return;
				try
				{
					var bookingManager = new BookingManager((Engine)helpers.Engine, helpers.Engine.FindElement(Convert.ToString(bookingManagerElementName))) { CustomProperties = true, CustomEvents = true };
					ReservationInstance = bookingManager.LeaveQuarantineState((Engine)helpers.Engine, ReservationInstance);
					leaveQuarantinedStateExceptionTextBox.Text = String.Empty;
				}
				catch (Exception exception)
				{
					helpers.Engine.Log($"{nameof(QuarantinedReservationSection)}|{nameof(TryLeaveQuarantineButton_Pressed)}|{exception}");
					leaveQuarantinedStateExceptionTextBox.Text = $"[{DateTime.Now}] {exception}";
				}

				GenerateUi();
				OnUiUpdateRequired(this, new EventArgs());
			}

			private void ClearReservationHistorySections()
			{
				ReservationHistoryCollapseButton.LinkedWidgets.Clear();
				reservationHistorySections.Clear();
			}

			private void ClearResourceHistorySections()
			{
				ResourceHistoryCollapseButton.LinkedWidgets.Clear();
				resourceHistorySections.Clear();
			}

			private void GenerateUi()
			{
				Clear();

				int row = -1;
				int labelColumnSpan = isOrder ? 2 : 1;
				int valueColumnSpan = isOrder ? 2 : 1;
				int valueColumnIdx = isOrder ? 3 : 2;

				AddWidget(collapseButton, ++row, 0);
				AddWidget(reservationTitleLabel, row, 1, 1, 3);

				AddReservationInfoWidgets(ref row, labelColumnSpan, valueColumnIdx, valueColumnSpan);

				if (!isOrder)
				{
					// Display functions and their resource here
					AddWidget(whiteSpace1, ++row, 0, 1, valueColumnSpan + 1);
					AddSection(resourceAssignmentSection, new SectionLayout(++row, 1));
					row += resourceAssignmentSection.RowCount;
				}

				AddWidget(whiteSpace2, ++row, 0, 1, valueColumnSpan + 1);

				AddWidget(LeaveQuarantinedStateButton, ++row, 1, 1, valueColumnSpan + 1);
				if (!String.IsNullOrWhiteSpace(leaveQuarantinedStateExceptionTextBox.Text))
				{
					AddWidget(leaveQuarantinedStateExceptionTextBox, ++row, 1, 1, valueColumnSpan + 1);
				}

				AddWidget(ReservationHistoryCollapseButton, ++row, 1, 1, valueColumnSpan + 1);
				if (!ReservationHistoryCollapseButton.IsCollapsed && !reservationHistorySections.Any())
				{
					AddWidget(noReservationHistoryAvailableLabel, ++row, 1, 1, valueColumnSpan + 1);
				}
				else
				{
					foreach (var historySection in reservationHistorySections.OrderBy(x => x.Time))
					{
						AddSection(historySection, new SectionLayout(++row, 1));
						row += historySection.RowCount;
					}
				}

				if (!isOrder)
				{
					AddWidget(ResourceHistoryCollapseButton, ++row, 1, 1, valueColumnSpan + 1);
					if (!ResourceHistoryCollapseButton.IsCollapsed && !resourceHistorySections.Any())
					{
						AddWidget(noContributingResourceHistoryAvailableLabel, ++row, 1, 1, valueColumnSpan + 1);
					}
					else
					{
						foreach (var historySection in resourceHistorySections.OrderBy(x => x.Time))
						{
							AddSection(historySection, new SectionLayout(++row, 1));
							row += historySection.RowCount;
						}
					}
				}

				if (quarantinedServiceSections.Any())
				{
					AddWidget(whiteSpace3, ++row, 0, 1, valueColumnSpan + 1);
					AddWidget(quarantinedServicesTitle, ++row, 1, 1, valueColumnSpan + 1);
				}

				foreach (var serviceSection in quarantinedServiceSections)
				{
					AddSection(serviceSection, new SectionLayout(++row, 1));
					row += serviceSection.RowCount;
				}
			}

			private void AddReservationInfoWidgets(ref int row, int labelColumnSpan, int valueColumnIdx, int valueColumnSpan)
			{
				AddWidget(reservationNameTitle, ++row, 1, 1, labelColumnSpan);
				AddWidget(reservationNameLabel, row, valueColumnIdx, 1, valueColumnSpan);

				AddWidget(reservationIdTitle, ++row, 1, 1, labelColumnSpan);
				AddWidget(reservationIdLabel, row, valueColumnIdx, 1, valueColumnSpan);

				AddWidget(reservationStartTitle, ++row, 1, 1, labelColumnSpan);
				AddWidget(reservationStartLabel, row, valueColumnIdx, 1, valueColumnSpan);

				AddWidget(reservationEndTitle, ++row, 1, 1, labelColumnSpan);
				AddWidget(reservationEndLabel, row, valueColumnIdx, 1, valueColumnSpan);
			}

			public ReservationInstance ReservationInstance { get; private set; }

			public CollapseButton ReservationHistoryCollapseButton { get; private set; } = new CollapseButton { IsCollapsed = true, CollapseText = "Hide Reservation History", ExpandText = "Show Reservation History" };

			public CollapseButton ResourceHistoryCollapseButton { get; private set; } = new CollapseButton { IsCollapsed = true, CollapseText = "Hide Contributing Resource History", ExpandText = "Show Contributing Resource History" };

			public Button LeaveQuarantinedStateButton { get; private set; } = new Button("Leave Quarantined State");

			/// <summary>
			/// Displays a line for each node in the service representing a function and the resource assigned to it.
			/// </summary>
			private sealed class ResourceAssignmentSection : Section
			{
				private readonly List<FunctionSection> functionSections = new List<FunctionSection>();
				private readonly Helpers helpers;

				private ServiceReservationInstance serviceReservationInstance;

				public ResourceAssignmentSection(Helpers helpers, ServiceReservationInstance serviceReservationInstance)
				{
					this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
					this.serviceReservationInstance = serviceReservationInstance ?? throw new ArgumentNullException(nameof(serviceReservationInstance));

					Initialize();
					GenerateUi();
				}

				private void Initialize()
				{
					var serviceDefinition = SrmManagers.ServiceManager.GetServiceDefinition(serviceReservationInstance.ServiceDefinitionID);
					var functions = serviceReservationInstance.GetFunctionData();
					var resourceUsageDefinitions = serviceReservationInstance.GetAllServiceResourceUsageDefinitions();
					var quarantinedResourceUsageDefinitions = serviceReservationInstance.QuarantinedResources;
					foreach (var node in serviceDefinition.Diagram.Nodes)
					{
						string resourcePoolName = node.Properties.FirstOrDefault(x => x.Name.Equals("Resource Pool"))?.Value;

						Resource functionResource;
						var resourceUsageDefinition = resourceUsageDefinitions.FirstOrDefault(x => x.ServiceDefinitionNodeID.Equals(node.ID));
						var quarantinedResourceUsage = quarantinedResourceUsageDefinitions.FirstOrDefault(x => (x.QuarantinedResourceUsage is ServiceResourceUsageDefinition srud && srud.ServiceDefinitionNodeID.Equals(node.ID)));
						if (resourceUsageDefinition != null)
						{
							functionResource = SrmManagers.ResourceManager.GetResource(resourceUsageDefinition.GUID);
						}
						else if (quarantinedResourceUsage != null)
						{
							functionResource = SrmManagers.ResourceManager.GetResource(quarantinedResourceUsage.QuarantinedResourceUsage.GUID);
						}
						else
						{
							functionResource = null;
						}

						var resourcePool = SrmManagers.ResourceManager.GetResourcePools(new ResourcePool { Name = resourcePoolName }).FirstOrDefault();
						var resourceCapabilityUsages = GetResourceCapabilityUsages(functions.FirstOrDefault(x => x.Id == node.ID));

						var eligbleResourceResult = SrmManagers.ResourceManager.GetEligibleResources(new EligibleResourceContext
						{
							ContextId = node.Configuration.FunctionID,
							TimeRange = new Net.Time.TimeRangeUtc(serviceReservationInstance.Start.ToUniversalTime(), serviceReservationInstance.End.ToUniversalTime()),
							ResourceFilter = ResourceExposers.PoolGUIDs.Contains(resourcePool.GUID),
							RequiredCapabilities = resourceCapabilityUsages
						});

						var functionSection = new FunctionSection(node.Label, node.Configuration.FunctionID, functionResource, eligbleResourceResult.EligibleResources);
						functionSection.UpdateResourceButton.Pressed += UpdateResourceButton_Pressed;
						functionSections.Add(functionSection);
					}
				}

				private void UpdateResourceButton_Pressed(object sender, EventArgs e)
				{
					var functionSection = functionSections.FirstOrDefault(x => x.UpdateResourceButton.Equals(sender));
					if (functionSection == null) return;

					var request = new AssignResourceRequest
					{
						TargetNodeLabel = functionSection.NodeLabel,
						NewResourceId = (functionSection.SelectedResource == null) ? Guid.Empty : functionSection.SelectedResource.ID
					};

					helpers.Engine.Log($"{nameof(ResourceAssignmentSection)}|{nameof(UpdateResourceButton_Pressed)}|Request: {JsonConvert.SerializeObject(request)}");

					try
					{
						helpers.Engine.Log($"{nameof(ResourceAssignmentSection)}|{nameof(UpdateResourceButton_Pressed)}|Assigning resource {functionSection.SelectedResource?.Name} with ID {request.NewResourceId} to node {request.TargetNodeLabel}");
						serviceReservationInstance = serviceReservationInstance.AssignResources((Engine)helpers.Engine, true, new[] { request });
						
						functionSection.Update(functionSection.SelectedResource);
					}
					catch (Exception exception)
					{
						functionSection.Exception = exception;
						helpers.Engine.Log($"{nameof(ResourceAssignmentSection)}|{nameof(UpdateResourceButton_Pressed)}|{exception}");
					}
				}

				private List<ResourceCapabilityUsage> GetResourceCapabilityUsages(Function function)
				{
					List<ResourceCapabilityUsage> resourceCapabilityUsages = new List<ResourceCapabilityUsage>();
					if (function == null) return resourceCapabilityUsages;

					foreach (var profileParameter in function.Parameters)
					{
						if (!profileParameter.Capability) continue;

						helpers.Engine.Log($"{nameof(ResourceAssignmentSection)}|{nameof(ResourceAssignmentSection)}|Function {function.Name}, Profile Parameter {profileParameter.Name} has value {profileParameter.Value}");

						var resourceCapabilityUsage = new ResourceCapabilityUsage { CapabilityProfileID = profileParameter.Id };
						switch (profileParameter.Type)
						{
							case ParameterType.Number:
								resourceCapabilityUsage.RequiredRangePoint = Convert.ToDouble(profileParameter.Value);
								break;
							case ParameterType.Discrete:
								resourceCapabilityUsage.RequiredDiscreet = Convert.ToString(profileParameter.Value);
								break;
							default:
								resourceCapabilityUsage.RequiredString = Convert.ToString(profileParameter.Value);
								break;
						}

						resourceCapabilityUsages.Add(resourceCapabilityUsage);
					}

					return resourceCapabilityUsages;
				}

				private void GenerateUi()
				{
					int row = -1;

					foreach (var functionSection in functionSections)
					{
						AddSection(functionSection, new SectionLayout(++row, 0));
						row += functionSection.RowCount;
					}
				}

				private sealed class FunctionSection : Section
				{
					private const string None = "None";

					private readonly List<Resource> availableResources;
					private readonly Resource originalResource;

					private readonly Label functionNameTitle = new Label { Style = TextStyle.Heading };
					private readonly Label currentResourceTitle = new Label("Current Resource");
					private readonly Label currentResourceLabel = new Label();
					private readonly Label availableResourcesTitle = new Label("Available Resource(s)");
					private readonly DropDown availableResourcesDropDown = new DropDown { IsDisplayFilterShown = true };
					private readonly TextBox exceptionTextBox = new TextBox { IsMultiline = true, IsVisible = false, Height = 300 };

					public FunctionSection(string nodeLabel, Guid functionId, Resource resource, IEnumerable<Resource> availableResources)
					{
						this.availableResources = availableResources.ToList();
						originalResource = resource;
						NodeLabel = nodeLabel;
						FunctionId = functionId;

						functionNameTitle.Text = NodeLabel;
						currentResourceLabel.Text = (resource == null) ? None : resource.Name;

						Initialize();
						GenerateUi();
					}

					public void Update(Resource resource)
					{
						currentResourceLabel.Text = (resource == null) ? None : resource.Name;
					}

					private void Initialize()
					{
						List<string> options = new List<string>(new [] { None });
						options.AddRange(availableResources.Select(x => x.Name).OrderBy(x => x));
						availableResourcesDropDown.Options = options;

						availableResourcesDropDown.Selected = (availableResources.Contains(originalResource)) ? originalResource.Name : None;
					}

					private void GenerateUi()
					{
						int row = -1;

						AddWidget(functionNameTitle, ++row, 0, 1, 2);

						AddWidget(currentResourceTitle, ++row, 0);
						AddWidget(currentResourceLabel, row, 1);

						AddWidget(availableResourcesTitle, ++row, 0);
						AddWidget(availableResourcesDropDown, row, 1);

						AddWidget(UpdateResourceButton, ++row, 1);

						AddWidget(exceptionTextBox, row, 1);
					}

					public Button UpdateResourceButton { get; private set; } = new Button("Edit Resource");

					public string NodeLabel { get; private set; }

					public Guid FunctionId { get; private set; }

					public Resource SelectedResource
					{
						get
						{
							if (availableResourcesDropDown.Selected == None) return null;
							return availableResources.FirstOrDefault(x => x.Name.Equals(availableResourcesDropDown.Selected));
						}
					}

					public Exception Exception
					{
						set
						{
							exceptionTextBox.Text = (value == null) ? String.Empty : value.ToString();
							exceptionTextBox.IsVisible = !String.IsNullOrWhiteSpace(exceptionTextBox.Text);
						}
					}
				}
			}
		}
	}
}