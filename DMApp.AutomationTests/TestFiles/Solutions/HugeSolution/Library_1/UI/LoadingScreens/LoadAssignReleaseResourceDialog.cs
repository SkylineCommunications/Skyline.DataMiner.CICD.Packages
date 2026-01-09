namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens
{
    using System;
    using System.Linq;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Functions;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

    public class LoadAssignReleaseResourceDialog : LoadingDialog
    {
        private string serviceId;
        private Service service;
        private Order order;
        private LockInfo lockInfo;
        private UserInfo userInfo;

        private readonly ResourceScriptAction resourceAction;

        public LoadAssignReleaseResourceDialog(Helpers helpers, ResourceScriptAction resourceAction) : base(helpers)
        {
            this.resourceAction = resourceAction;
        }

        public AssignReleaseResourceDialog AssignReleaseResourceDialog { get; private set; }

        protected override void GetScriptInput()
        {
            serviceId = Engine.GetScriptParam("BookingID").Value;
        }

        protected override void CollectActions()
        {
            methodsToExecute.Add(GetService);
            methodsToExecute.Add(VerifyService);
            methodsToExecute.Add(InitializeOrderLogger);
            methodsToExecute.Add(GetOrder);
            methodsToExecute.Add(GetLockInfo);
            methodsToExecute.Add(GetUserInfo);
            methodsToExecute.Add(ConstructAssignReleaseResourceDialog);
        }

        protected override void SendReportButton_Pressed(object sender, EventArgs e)
        {
            string title = "Exception while loading Assign/Release Recording Resource [" + DateTime.Now + "]";

            string message = $"Order: '{order?.Name}'<br>Order ID: {order?.Id}<br>Service: '{service?.Name}'<br>Service ID: {serviceId}<br>User: {Engine.UserLoginName}<br>Timestamp: {DateTime.Now}<br>Exception: {informationMessageLabel.Text.Replace(" at ", "<br>at ")}";

            NotificationManager.SendMailToSkylineDevelopers(Helpers, title, message);

            reportSuccessfullySentLabel.IsVisible = true;
        }

        private void GetService()
        {
            var getServiceTask = new GetServiceTask(Helpers, serviceId);
            Tasks.Add(getServiceTask);

            IsSuccessful &= getServiceTask.Execute();

            if (!IsSuccessful) return;

            service = getServiceTask.Service;
        }

        private void VerifyService()
        {
            if (service.IsSharedSource)
            {
                PrepareUiForManualErrorMessage("This service is an Shared Source. Use the dedicated button to edit Shared Sources.");
            }
        }

        private void InitializeOrderLogger()
        {
            Helpers.AddOrderReferencesForLogging(service.OrderReferences.ToArray());
        }

        private void GetOrder()
        {
            var getOrderTask = new GetOrderTask(Helpers, service.OrderReferences.Single());
            Tasks.Add(getOrderTask);

            IsSuccessful &= getOrderTask.Execute();

            if (!IsSuccessful) return;

            order = getOrderTask.Order;
        }

        private void GetLockInfo()
        {
            var getLockInfoTask = new GetOrderLockTask(Helpers, order.Id);
            Tasks.Add(getLockInfoTask);

            IsSuccessful &= getLockInfoTask.Execute();

            if (!IsSuccessful) return;

            lockInfo = getLockInfoTask.LockInfo;
        }

        private void GetUserInfo()
        {
            var getUserInfoTask = new GetUserInfoTask(Helpers, order.Event);
            Tasks.Add(getUserInfoTask);

            IsSuccessful &= getUserInfoTask.Execute();

            if (!IsSuccessful) return;

            this.userInfo = getUserInfoTask.UserInfo;
        }

        private void ConstructAssignReleaseResourceDialog()
        {
            var constructAssignReleaseResourceDialogTask = new ConstructAssignReleaseResourceDialogTask(Helpers, resourceAction, service.Id, order, lockInfo, userInfo);
            Tasks.Add(constructAssignReleaseResourceDialogTask);

            IsSuccessful &= constructAssignReleaseResourceDialogTask.Execute();

            if (!IsSuccessful) return;

            AssignReleaseResourceDialog = constructAssignReleaseResourceDialogTask.AssignReleaseResourceDialog;
        }
    }
}
