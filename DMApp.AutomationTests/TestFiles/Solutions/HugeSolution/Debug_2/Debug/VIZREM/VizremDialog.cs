namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Debug.VIZREM
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Order = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates;

	public class VizremDialog : Dialog
	{
		private readonly Helpers helpers;

		private readonly Label triggerVizremBookLabel = new Label("Trigger VIZREM Book") { Style = TextStyle.Heading };

		public VizremDialog(Helpers helpers) : base(helpers.Engine)
		{
			Title = "Vizrem";

			this.helpers = helpers;

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		public Button TriggerBookFlowButton { get; } = new Button("Trigger Book...") { Width = 150 };

		private TextBox DataTextBox { get; set; }

		public void CreateVizremOrder(InteractiveController app)
		{
			//var createdOrder = CreateNewOrder();
			var adaptedOrder = GetOrderAndChangeResources();

			AddOrUpdateOrder(adaptedOrder, app);
		}

		private void Initialize()
		{
			DataTextBox = new TextBox { PlaceHolder = "Data", Width = 400 };
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddWidget(triggerVizremBookLabel, ++row, 0, 1, 5);

			AddWidget(DataTextBox, ++row, 2);
			AddWidget(TriggerBookFlowButton, ++row, 2);
		}

		private Order CreateNewOrder()
		{
			DateTime start = DateTime.Now.AddDays(1).RoundToMinutes();
			DateTime end = start.AddMinutes(60).RoundToMinutes();

			var allServiceDefinitions = helpers.ServiceDefinitionManager.ServiceDefinitionsForLiveOrderForm;

			var studioService = new DisplayedService(helpers, allServiceDefinitions.VizremStudios.First(x => x.Description.Contains("NDI Router")));
			studioService.AcceptChanges();

			var vizremService = new DisplayedService(helpers, allServiceDefinitions.VizremFarms.First());
			vizremService.AcceptChanges();

			var studioDestinationService = new DisplayedService(helpers, allServiceDefinitions.VizremStudios.First(x => x.Description.Contains("NDI Router")));
			studioDestinationService.AcceptChanges();

			studioService.Start = start;
			studioService.End = end;
			var studioFunction = studioService.Functions[0];
			var studioResource = helpers.ResourceManager.GetResourcesByName("ST26 NDI1 to NDI1").First() as FunctionResource;
			studioFunction.Resource = studioResource;

			vizremService.Start = start;
			vizremService.End = end;
			var vizremFunction = vizremService.Functions[0];
			vizremFunction.Resource = helpers.ResourceManager.GetResourcesByName("VIZREM 06").First() as FunctionResource;

			studioDestinationService.Start = start;
			studioDestinationService.End = end;
			var studioDestinationFunction = studioDestinationService.Functions[0];
			studioDestinationFunction.Resource = helpers.ResourceManager.GetResourcesByName("ST26 NDI5 to NDI5").First() as FunctionResource;


			Order newOrder = new Order
			{
				ManualName = $"Vizrem Order {Guid.NewGuid()}",
				Start = start,
				End = end,
				Subtype = OrderSubType.Vizrem,
				Event = null,
				IsInternal = false,
				Contract = "YLE Base",
				Company = "YLE",
				CreatedByUserName = helpers.Engine.UserLoginName,
				CreatedByEmail =  String.Empty,
				CreatedByPhone = String.Empty,
				LastUpdatedBy = helpers.Engine.UserLoginName,
				LastUpdatedByEmail = String.Empty,
				LastUpdatedByPhone = String.Empty,
				BillingInfo = new BillingInfo
				{
					BillableCompany = "YLE",
					CustomerCompany = String.Empty
				},
				Status = YLE.Order.Status.Confirmed,
			};

			newOrder.SetUserGroupIds(new HashSet<int>() { 0 });
			
			newOrder.SetSecurityViewIds(new HashSet<int>() { 10102 });

			newOrder.AcceptChanges();
			newOrder.SourceService = studioService;
			newOrder.SourceService.Children.Add(vizremService);
			vizremService.Children.Add(studioDestinationService);

			DataTextBox.Text = JsonConvert.SerializeObject(newOrder.AllServices);

			return newOrder;
		}

		private Order GetOrderAndChangeResources()
		{
			var order = helpers.OrderManager.GetOrder(new Guid("359555fe-6a44-499b-a4d0-4e075d76f6b8"));
			var orderServices = order.AllServices;

			var studioService = order.Sources[0];
			var studioFunction = studioService.Functions[0];
			studioFunction.Resource = helpers.ResourceManager.GetResourcesByName("T2.Studio Mediapolis").First() as FunctionResource;

			var vizremFarm = orderServices.First(s => s.Definition.VirtualPlatform == VirtualPlatform.VizremFarm);
			var vizremFunction = vizremFarm.Functions[0];
			vizremFunction.Resource = helpers.ResourceManager.GetResourcesByName("VIZREM 05").First() as FunctionResource;

			var studioDestination = orderServices.Last(s => s.Definition.VirtualPlatform == VirtualPlatform.VizremStudio);
			var studioDestinationFunction = studioDestination.Functions[0];
			studioDestinationFunction.Resource = helpers.ResourceManager.GetResourcesByName("T2.Studio Mediapolis").First() as FunctionResource;

			return order;
		}

		private void AddOrUpdateOrder(Order order, InteractiveController app)
		{
			var reportDialog = new AddOrUpdateReportDialog(helpers, false);

			reportDialog.OkButton.Pressed += (o, e) =>
			{
				app.ShowDialog(this);
			};

			app.ShowDialog(reportDialog);

			var tasks = order.AddOrUpdate(helpers, false).Tasks;

			if (tasks.Any())
			{
				reportDialog.Finish(tasks);

				// release the locks if all tasks were successful
				//if (tasks.All(t => t.Status == TaskStatus.Ok)) helpers.LockManager.ReleaseLocks();

				app.ShowDialog(reportDialog);
			}

			helpers.Log(nameof(VizremDialog), nameof(AddOrUpdateOrder), $"Vizrem Order Updated");
		}
	}
}
