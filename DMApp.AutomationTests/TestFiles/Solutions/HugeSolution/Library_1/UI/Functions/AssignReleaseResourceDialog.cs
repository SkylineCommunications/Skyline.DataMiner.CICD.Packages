namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Functions
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Service = YLE.Service.Service;

	public class AssignReleaseResourceDialog : Dialog
    {
        private readonly Label orderNameLabel = new Label("Order name:");
        private readonly Label resourcesLabel = new Label("Resource:");

        private readonly Guid serviceReservationId;
        private readonly Helpers helpers;
        private readonly Order order;
        private readonly UserInfo userInfo;

        private readonly ResourceScriptAction resourceAction;

        public AssignReleaseResourceDialog(Helpers helpers, ResourceScriptAction resourceAction, Guid serviceToEditId, Order order, LockInfo lockInfo, UserInfo userInfo) : base(helpers.Engine)
        {
            if (serviceToEditId == Guid.Empty) throw new ArgumentException("Guid is empty", nameof(serviceToEditId));
            this.helpers = helpers;
            this.order = order ?? throw new ArgumentNullException(nameof(order));
            if (!order.AllServices.Select(s => s.Id).Contains(serviceToEditId)) throw new ArgumentException($"No Service with ID {serviceToEditId} found in Order", nameof(serviceToEditId));
            this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));

            Title = resourceAction == ResourceScriptAction.Assign ? "Assign recording resource" : "Release recording resource";
            Service = order.AllServices.Single(s => s.Id == serviceToEditId);
            serviceReservationId = serviceToEditId;
            this.resourceAction = resourceAction;

            InitializeWidgets();
            if (!lockInfo.IsLockGranted) DisableAll();
            GenerateUi();
        }

        public Button AssignButton { get; private set; }

        public Button ReleaseButton { get; private set; }

        public DropDown AvailableResourcesDropDown { get; private set; }

        private YleTextBox OrderNameTextBox { get; set; }

        private Service Service { get; set; }

        private DisplayedFunction CurrentlyUsedFunction { get; set; }

        public UpdateResult Finish()
        {
            helpers.Log(nameof(AssignReleaseResourceDialog), nameof(Finish), "Finishing Assigning/Releasing Resource", Service.Name);

            EditResources();

            return order.AddOrUpdate(helpers, userInfo.IsMcrUser);
        }

        private void EditResources()
        {
            switch (Service.Definition.VirtualPlatform)
            {
                case ServiceDefinition.VirtualPlatform.Recording:
                    if (resourceAction == ResourceScriptAction.Assign)
                    {
                        AssignRecordingResource();
                    }
                    else
                    {
                        ReleaseRecordingResource();
                    }
                    break;
                
                default:
                    break;
            }
        }

        private void ReleaseRecordingResource()
        {
            helpers.LogMethodStart(nameof(AssignReleaseResourceDialog), nameof(ReleaseRecordingResource), out var stopwatch);

            try
            {
                Service.ReleaseResources(helpers, order);
                helpers.LogMethodCompleted(nameof(AssignReleaseResourceDialog), nameof(ReleaseRecordingResource), null, stopwatch);
            }
            catch (Exception e)
            {
                helpers.Log(nameof(AssignReleaseResourceDialog), nameof(ReleaseRecordingResource), e.Message);
            }
        }

        private void AssignRecordingResource()
        {
            helpers.LogMethodStart(nameof(AssignReleaseResourceDialog), nameof(AssignRecordingResource), out var stopwatch );

            try
            {
                CurrentlyUsedFunction.Resource = CurrentlyUsedFunction.SelectableResources.Single(resource => AvailableResourcesDropDown.Selected == resource.GetDisplayName(CurrentlyUsedFunction.Id));
                helpers.LogMethodCompleted(nameof(AssignReleaseResourceDialog), nameof(AssignRecordingResource), null, stopwatch);
            }
            catch (Exception e)
            {
                helpers.Log(nameof(AssignReleaseResourceDialog), nameof(AssignRecordingResource), e.Message);
            }
        }

        private void GenerateUi()
        {
            int row = -1;

            AddWidget(new Label(Service.GetShortDescription(order)) { Style = TextStyle.Title }, ++row, 0, 1, 2);

            AddWidget(new WhiteSpace(), ++row, 0);

            AddWidget(orderNameLabel, ++row, 0);
            AddWidget(OrderNameTextBox, row, 1);

            AddWidget(new WhiteSpace(), ++row, 0);

            AddWidget(resourcesLabel, ++row, 0);
            AddWidget(AvailableResourcesDropDown, row, 1);

            AddWidget(new WhiteSpace(), ++row, 0);

            AddWidget(AssignButton, ++row, 0);
            AddWidget(ReleaseButton, ++row, 0);
        }

        private void InitializeWidgets()
        {
            OrderNameTextBox = new YleTextBox(order.Name) { Width = 200, IsEnabled = false };

            resourcesLabel.IsVisible = resourceAction == ResourceScriptAction.Assign;

            AvailableResourcesDropDown = new DropDown() { IsVisible = resourceAction == ResourceScriptAction.Assign, Width = 200 };

            if (resourceAction == ResourceScriptAction.Assign)
            {
                CurrentlyUsedFunction = (DisplayedFunction)Service.Functions.FirstOrDefault() ?? throw new ServiceFunctionNotFoundException(Service.Id, Service.Name);

                var context = CurrentlyUsedFunction.Definition.GetEligibleResourceContext(helpers, Service.StartWithPreRoll.ToUniversalTime(), Service.EndWithPostRoll.ToUniversalTime(), serviceReservationId, CurrentlyUsedFunction.NodeId, CurrentlyUsedFunction.Parameters.Where(p => p.IsCapability).ToList());

                CurrentlyUsedFunction.SelectableResources = helpers.ResourceManager.GetAvailableResources(context.Yield(), false)[context.FunctionDefinitionLabel];

                AvailableResourcesDropDown.Options = CurrentlyUsedFunction.DisplayedResourceNames.OrderBy(name => name);

                if (!AvailableResourcesDropDown.Options.Contains(CurrentlyUsedFunction.ResourceName)) throw new Exception("The current active resource isn't inside the options");

                AvailableResourcesDropDown.Selected = CurrentlyUsedFunction.ResourceName;

                helpers.Log(nameof(AssignReleaseResourceDialog), nameof(InitializeWidgets), "Available resource DropDown is correctly initialized with: " + String.Join(",", AvailableResourcesDropDown.Options));
            }

            AssignButton = new Button("Assign Resource") { IsVisible = resourceAction == ResourceScriptAction.Assign };
            ReleaseButton = new Button("Release Resource") { IsVisible = resourceAction == ResourceScriptAction.Release };
        }

        private void DisableAll()
        {
            AssignButton.IsEnabled = false;
            ReleaseButton.IsEnabled = false;
            AvailableResourcesDropDown.IsEnabled = false;
        }
    }
}
