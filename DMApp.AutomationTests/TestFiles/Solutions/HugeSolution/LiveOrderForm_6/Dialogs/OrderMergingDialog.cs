namespace LiveOrderForm_6.Dialogs
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;

	public class OrderMergingDialog : Dialog
	{
		private readonly Label titleLabel = new Label("MERGE ORDERS") { Style = TextStyle.Bold };
		private readonly Label primaryOrderLabel = new Label("Primary Order");
		private readonly Label nameForMergedOrderLabel = new Label("Name for merged order");
		private readonly Label sourcesLabel = new Label("Sources") { Style = TextStyle.Bold };
		private readonly Label sourceServiceLabel = new Label("Source Service");
		private readonly Label backupSourceServiceLabel = new Label("Backup Source Service");
		private readonly Label destinationsLabel = new Label("Destinations") { Style = TextStyle.Bold };
		private readonly Label recordingsLabel = new Label("Recordings") { Style = TextStyle.Bold };
		private readonly Label transmissionsLabel = new Label("Transmissions") { Style = TextStyle.Bold };
		private readonly Label validationLabel = new Label(String.Empty);
		private readonly Label editMergedOrdersExplanationLabel = new Label("NOTE: the actual merging of the Orders happens after saving or booking");

		private readonly List<CheckedService> checkedServices = new List<CheckedService>();
		private readonly List<CheckBox> destinationCheckBoxes = new List<CheckBox>();
		private readonly List<CheckBox> recordingCheckBoxes = new List<CheckBox>();
		private readonly List<CheckBox> transmissionCheckBoxes = new List<CheckBox>();
		private readonly Helpers helpers;
		private readonly IEnumerable<Order> orders;

		private DropDown primaryOrderDropDown;
		private DropDown nameForMergedOrderDropDown;
		private Label sourceServiceValueLabel;
		private Label backupSourceServiceValueLabel;

		public OrderMergingDialog(Helpers helpers, IEnumerable<Order> orders) : base(helpers.Engine)
		{
			this.Title = "Merge Orders";
			this.helpers = helpers;
			this.orders = orders ?? throw new ArgumentNullException(nameof(orders));

			if(!ValidOrderCollectionForMerge()) throw new ArgumentException("Invalid collection of orders to merge. Only certain combinations of orders created by integrations are allowed.", nameof(orders));

			foreach (var order in orders)
			{
				order.AcceptChanges(null);
			}

			InitializePrimaryOrderWidgets();
			InitCheckBoxes();
			EditMergedOrdersButton = new Button("Edit Merged Order") { Width = 200, Style = ButtonStyle.CallToAction };

			GenerateUI();
		}

		private bool ValidOrderCollectionForMerge()
		{
			int plasmaOrderCount = orders.Count(o => o.IntegrationType == IntegrationType.Plasma);
			int ceitonOrderCount = orders.Count(o => o.IntegrationType == IntegrationType.Ceiton);
			int ebuOrderCount = orders.Count(o => o.IntegrationType == IntegrationType.Eurovision);
			int feenixOrderCount = orders.Count(o => o.IntegrationType == IntegrationType.Feenix);
			int pebbleBeachOrderCount = orders.Count(o => o.IntegrationType == IntegrationType.PebbleBeach);

			bool multipleOrdersOfNonMatchingIntegrations = plasmaOrderCount + ceitonOrderCount + feenixOrderCount + pebbleBeachOrderCount > 1;
			bool multipleEbuOrders = ebuOrderCount > 1;

			if (multipleEbuOrders || multipleOrdersOfNonMatchingIntegrations) return false;

			return true;
		}

		public Button EditMergedOrdersButton { get; set; }

		public bool IsValid()
		{
			helpers.LogMethodStart(nameof(OrderMergingDialog), nameof(IsValid), out var stopwatch);

			bool isValid = false;

			if (!AllNecessaryServicesToRecordAreSelected())
			{
				helpers.Log(nameof(OrderMergingDialog), nameof(IsValid), "Dialog is not valid");
				helpers.LogMethodCompleted(nameof(OrderMergingDialog), nameof(IsValid));
				return false;
			}

			bool atLeastOneDestination = destinationCheckBoxes.Any(x => x.IsChecked);
			isValid |= atLeastOneDestination;

			bool atLeastOneRecording = recordingCheckBoxes.Any(x => x.IsChecked);
			isValid |= atLeastOneRecording;

			bool atLeastOneTransmission = transmissionCheckBoxes.Any(x => x.IsChecked);
			isValid |= atLeastOneTransmission;

			if (!atLeastOneDestination && !atLeastOneRecording && !atLeastOneTransmission)
			{
				validationLabel.Text = "At least 1 Destination, Recording or Transmission should be selected";
			}

			helpers.Log(nameof(OrderMergingDialog), nameof(IsValid), "Dialog is "+(isValid ? string.Empty : "not")+" valid");
			helpers.LogMethodCompleted(nameof(OrderMergingDialog), nameof(IsValid));

			return isValid;
		}

		private bool AllNecessaryServicesToRecordAreSelected()
		{
			helpers.LogMethodStart(nameof(OrderMergingDialog), nameof(AllNecessaryServicesToRecordAreSelected), out var stopwatch);

			bool allServicesToRecordAreSelected = true;
			var checkedRecordings = checkedServices.Where(s => s.Service.Definition.VirtualPlatform == VirtualPlatform.Recording && s.CheckBox.IsChecked);
			foreach (var checkedRecording in checkedRecordings)
			{
				var serviceToRecord = checkedRecording.Order.AllServices.Single(s => s.Name == checkedRecording.Service.RecordingConfiguration.NameOfServiceToRecord);

				if (serviceToRecord.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception)
				{
					var primaryOrder = orders.Single(x => x.Name == primaryOrderDropDown.Selected);

					bool serviceToRecordIsPartOfPrimaryOrder = primaryOrder.Sources.Exists(s => s.Id == serviceToRecord.Id);
					allServicesToRecordAreSelected &= serviceToRecordIsPartOfPrimaryOrder;
				}
				else
				{
					bool serviceToRecordIsChecked = checkedServices.Any(s => s.Service.Id == serviceToRecord.Id && s.CheckBox.IsChecked);
					allServicesToRecordAreSelected &= serviceToRecordIsChecked;
				}

				if (!allServicesToRecordAreSelected)
				{
					validationLabel.Text = "One or more Recordings are selected without the service to record being selected.";
					helpers.Log(nameof(OrderMergingDialog), nameof(AllNecessaryServicesToRecordAreSelected), validationLabel.Text);
					helpers.LogMethodCompleted(nameof(OrderMergingDialog), nameof(AllNecessaryServicesToRecordAreSelected));

					return false;
				}
			}

			helpers.LogMethodCompleted(nameof(OrderMergingDialog), nameof(AllNecessaryServicesToRecordAreSelected));

			return true;
		}

		public Order GetMergedOrder()
		{
			helpers.LogMethodStart(nameof(OrderMergingDialog), nameof(GetMergedOrder), out var stopwatch);

			var primaryOrder = orders.Single(x => x.Name == primaryOrderDropDown.Selected);

			primaryOrder.AcceptChanges();
			foreach (var primaryOrderService in primaryOrder.AllServices) primaryOrderService.AcceptChanges();

			// Plasma ID should not get lost when merging orders
			var plasmaIdFromMergingOrder = orders.Select(o => o.PlasmaId).FirstOrDefault(plasmaId => !string.IsNullOrWhiteSpace(plasmaId));
			if (!string.IsNullOrWhiteSpace(plasmaIdFromMergingOrder)) primaryOrder.PlasmaId = plasmaIdFromMergingOrder;

			helpers.Log(nameof(OrderMergingDialog), nameof(GetMergedOrder), "Selected primary order: " + primaryOrder.Name + "(id:" + primaryOrder.Id + ")");

			var primaryOrderMainServiceGuids = primaryOrder.GetAllMainServices().Select(x => x.Id).ToList();

			var selectedChildServices = checkedServices.Where(x => x.CheckBox.IsChecked).Select(x => x.Service).ToList();

			helpers.Log(nameof(OrderMergingDialog),nameof(GetMergedOrder), "Selected child services: " + string.Join(", ", selectedChildServices.Select(s => s.Name + "(id:" + s.Id + ")")));

			var mergedOrderChildServices = new ObservableCollection<Service>();
			foreach (var childService in selectedChildServices)
			{
				bool isServiceFromNonPrimaryOrder = !primaryOrderMainServiceGuids.Contains(childService.Id);
				if (isServiceFromNonPrimaryOrder)
				{
					childService.AcceptChanges();

					helpers.Log(nameof(OrderMergingDialog), nameof(GetMergedOrder), $"Service {childService.Name} is not part of the primary order, setting Node ID to 0");

					childService.NodeId = 0;

					foreach (var nonPrimaryOrder in orders.Where(o => o.Id != primaryOrder.Id))
					{
						childService.OrderReferences.Remove(nonPrimaryOrder.Id);
						if (childService.ReservationInstance != null) childService.UpdateOrderReferencesProperty(helpers);
					}
				}

				mergedOrderChildServices.Add(childService);
			}

			var mergedOrder = primaryOrder;

			mergedOrder.Sources.Single(x => x.BackupType == BackupType.None).SetChildren(mergedOrderChildServices);

			foreach (var service in mergedOrder.AllServices)
			{
				service.UserTasks = new List<LiveUserTask>();
			}

			mergedOrder.ManualName = nameForMergedOrderDropDown.Selected;
			mergedOrder.Start = mergedOrder.AllServices.OrderBy(s => s.StartWithPreRoll).First().StartWithPreRoll;
			mergedOrder.End = mergedOrder.AllServices.OrderByDescending(s => s.EndWithPostRoll).First().EndWithPostRoll;

			helpers.LogMethodCompleted(nameof(OrderMergingDialog), nameof(GetMergedOrder));

			return mergedOrder;
		}

		public List<Order> GetNonPrimaryMergingOrders()
		{
			return orders.Where(x => x.Name != primaryOrderDropDown.Selected).ToList();
		}

		private void InitializePrimaryOrderWidgets()
		{
			var orderNames = orders.Select(x => x.Name).OrderBy(x => x).ToArray();

			var orderFromIntegration = orders.FirstOrDefault(x => x.IntegrationType != IntegrationType.None);

			var primaryOrder = orderFromIntegration ?? orders.First(x => x.Name == orderNames[0]);

			primaryOrderDropDown = new DropDown(orderNames, primaryOrder.Name) { IsEnabled = true, MinWidth = 500 };
			primaryOrderDropDown.Changed += PrimaryOrderDropDown_Changed;

			nameForMergedOrderDropDown = new DropDown(orderNames, primaryOrder.Name);

			InitializeSourceServiceLabels(primaryOrder);
		}

		private void InitializeSourceServiceLabels(Order primaryOrder)
		{
			var primaryOrderMainSourceService = primaryOrder.Sources.Single(x => x.BackupType == BackupType.None);
			sourceServiceValueLabel = new Label(primaryOrderMainSourceService.GetShortDescription());

			var primaryOrderBackupSourceService = primaryOrder.Sources.SingleOrDefault(x => x.BackupType != BackupType.None);
			backupSourceServiceValueLabel = new Label(primaryOrderBackupSourceService != null ? primaryOrderBackupSourceService.GetShortDescription() : "N/A");
		}

		private void PrimaryOrderDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			var primaryOrder = orders.FirstOrDefault(x => x.Name.Equals(primaryOrderDropDown.Selected)) ?? throw new NotFoundException($"Unable to find order {primaryOrderDropDown.Selected}");

			sourceServiceValueLabel.Text = primaryOrder.MainSourceService.GetShortDescription();

			backupSourceServiceValueLabel.Text = primaryOrder.BackupSourceService != null ? primaryOrder.BackupSourceService.GetShortDescription() : "N/A";
		}

		private void InitCheckBoxes()
		{
			if (orders == null) return;

			foreach (Order order in orders)
			{
				if (order.Sources == null || !order.Sources.Any())
				{
					continue;
				}

				foreach (Service service in OrderManager.FlattenServices(new[] { order.Sources[0] }))
				{
					if (service?.Name == null || service.Definition == null) continue;

					CheckedService checkedService;
					switch (service.Definition.VirtualPlatformServiceType)
					{
						case VirtualPlatformType.Destination:
							checkedService = new CheckedService(order, service);
							checkedServices.Add(checkedService);
							destinationCheckBoxes.Add(checkedService.CheckBox);
							break;
						case VirtualPlatformType.Recording:
							checkedService = new CheckedService(order, service);
							checkedServices.Add(checkedService);
							recordingCheckBoxes.Add(checkedService.CheckBox);
							break;
						case VirtualPlatformType.Transmission:
							checkedService = new CheckedService(order, service);
							checkedServices.Add(checkedService);
							transmissionCheckBoxes.Add(checkedService.CheckBox);
							break;
						default:
							break;
					}
				}
			}
		}

		private void GenerateUI()
		{
			int row = 0;

			AddWidget(titleLabel, new WidgetLayout(row, 0));

			AddWidget(primaryOrderLabel, new WidgetLayout(++row, 0));
			AddWidget(primaryOrderDropDown, new WidgetLayout(row, 1));

			AddWidget(nameForMergedOrderLabel, new WidgetLayout(++row, 0));
			AddWidget(nameForMergedOrderDropDown, new WidgetLayout(row, 1));

			AddWidget(sourcesLabel, new WidgetLayout(++row, 0));

			AddWidget(sourceServiceLabel, new WidgetLayout(++row, 0));
			AddWidget(sourceServiceValueLabel, new WidgetLayout(row, 1));

			AddWidget(backupSourceServiceLabel, new WidgetLayout(++row, 0));
			AddWidget(backupSourceServiceValueLabel, new WidgetLayout(row, 1));

			if (destinationCheckBoxes.Any())
			{
				AddWidget(destinationsLabel, new WidgetLayout(++row, 0));
				foreach (CheckBox checkBox in destinationCheckBoxes)
				{
					AddWidget(checkBox, new WidgetLayout(++row, 0));
				}
			}

			if (recordingCheckBoxes.Any())
			{
				AddWidget(recordingsLabel, new WidgetLayout(++row, 0));
				foreach (CheckBox checkBox in recordingCheckBoxes)
				{
					AddWidget(checkBox, new WidgetLayout(++row, 0));
				}
			}

			if (transmissionCheckBoxes.Any())
			{
				AddWidget(transmissionsLabel, new WidgetLayout(++row, 0));
				foreach (CheckBox checkBox in transmissionCheckBoxes)
				{
					AddWidget(checkBox, new WidgetLayout(++row, 0));
				}
			}

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));

			AddWidget(EditMergedOrdersButton, new WidgetLayout(++row, 0));
			AddWidget(validationLabel, new WidgetLayout(row, 1));

			AddWidget(editMergedOrdersExplanationLabel, new WidgetLayout(row + 1, 0, 1, 2));
		}

		private class CheckedService
		{
			private const string DisplayedServiceFormat = "{0} from {1}";

			public CheckedService(Order order, Service service)
			{
				Service = service;
				Order = order;
				CheckBox = new CheckBox(String.Format(DisplayedServiceFormat, service.GetShortDescription(order), order.Name));
			}

			public CheckBox CheckBox { get; private set; }

			public Service Service { get; private set; }

			public Order Order { get; private set; }
		}
	}
}