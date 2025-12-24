namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens
{
    using System;
    using System.Linq;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

    public class LoadNonLiveOrderFormDialog : LoadingDialog
    {
        public enum ScriptAction
        {
            AddOrUpdate,
            Duplicate,
            View
        }

        private readonly ScriptAction scriptAction;

        private int ticketId;
        private int dataminerId;
        private NonLiveOrder nonLiveOrder;

        public LoadNonLiveOrderFormDialog(Helpers helpers, ScriptAction scriptAction) : base(helpers)
        {
            this.scriptAction = scriptAction;
        }

        public MainDialog NonLiveOrderFormDialog { get; private set; }

        protected override void GetScriptInput()
        {
            var rawScriptInputParameter = Engine.GetScriptParam("ticketId").Value;

            Helpers.Log(nameof(LoadNonLiveOrderFormDialog), nameof(GetScriptInput), $"Ticket id input parameter value: {rawScriptInputParameter}");

            var cleanInputScriptParameter = rawScriptInputParameter?.Split(new[] { ',' }).Select(item => item.Trim('"', '[', ']')).ToArray();
            if (cleanInputScriptParameter == null || cleanInputScriptParameter.Length == 0)
            {
                PrepareUiForManualErrorMessage("At least one order need to be selected before you can proceed", showExceptionWidgets: false);
            }
            else if (cleanInputScriptParameter.Length > 1)
            {
                PrepareUiForManualErrorMessage("It is only allowed to select one order at a time", showExceptionWidgets: false);
            }
            else
            {
                string[] dmaAndTicketId = cleanInputScriptParameter[0].Split(new[] { '/' });
                if (dmaAndTicketId.Length == 2)
                {
                    if (!int.TryParse(dmaAndTicketId[1], out ticketId)) throw new ArgumentException("Script input is not valid.");
                    if (!int.TryParse(dmaAndTicketId[0], out dataminerId)) throw new ArgumentException("Script input is not valid.");
                }
                else
                {
                    ticketId = -1;
                    dataminerId = -1;
                }
            }
        }

        protected override void CollectActions()
        {
            methodsToExecute.Add(InitializeOrderLogger);
            methodsToExecute.Add(GetNonLiveOrder);
            methodsToExecute.Add(GetUserInfo);
            methodsToExecute.Add(ConstructNonLiveOrderForm);
        }

        protected override void SendReportButton_Pressed(object sender, EventArgs e)
        {
            string title = "Exception while loading Non-Live Order Form [" + DateTime.Now + "]";

            string message = $"Order: '{nonLiveOrder?.OrderDescription}'<br>Order ID: {dataminerId}/{ticketId}<br>User: {Engine.UserLoginName}<br>Timestamp: {DateTime.Now}<br>Exception: {informationMessageLabel.Text.Replace(" at ", "<br>at ")}";

            NotificationManager.SendMailToSkylineDevelopers(Helpers, title, message);

            reportSuccessfullySentLabel.IsVisible = true;
        }

        private void InitializeOrderLogger()
        {
            Helpers.Log(nameof(LoadNonLiveOrderFormDialog), "START SCRIPT", "ADD OR UPDATE NON LIVE");
        }

        private void GetNonLiveOrder()
        {
            if (dataminerId == -1 && ticketId == -1) return;

            var getNonLiveOrderTask = new GetNonLiveOrderTask(Helpers, dataminerId, ticketId);
            Tasks.Add(getNonLiveOrderTask);

            IsSuccessful &= getNonLiveOrderTask.Execute();
            if (!IsSuccessful) return;

            nonLiveOrder = getNonLiveOrderTask.NonLiveOrder;
        }

        private void GetUserInfo()
        {
            var getBaseUserInfoTask = new GetBaseUserInfoTask(Helpers);
            Tasks.Add(getBaseUserInfoTask);

            IsSuccessful &= getBaseUserInfoTask.Execute();
            if (!IsSuccessful) return;

            UserInfo = getBaseUserInfoTask.UserInfo;
        }

        private void ConstructNonLiveOrderForm()
        {
            if (scriptAction == ScriptAction.AddOrUpdate) HandleEditOrder();

            if (scriptAction == ScriptAction.Duplicate) PrepareOrderForDuplication();

            var constructNonLiveOrderFormTask = new ConstructNonLiveOrderFormTask(Helpers, UserInfo, nonLiveOrder, scriptAction);
            Tasks.Add(constructNonLiveOrderFormTask);

            IsSuccessful &= constructNonLiveOrderFormTask.Execute();
            if (!IsSuccessful) return;

            NonLiveOrderFormDialog = constructNonLiveOrderFormTask.NonLiveOrderForm;
        }

        private void HandleEditOrder()
        {
            if (nonLiveOrder != null && nonLiveOrder.State == State.Completed)
                PrepareUiForManualErrorMessage("It is not possible to edit an order with status " + nonLiveOrder.State + ".");
        }

        private void PrepareOrderForDuplication()
        {
            nonLiveOrder.DataMinerId = null;
            nonLiveOrder.TicketId = null;
            nonLiveOrder.OrderDescription = string.Empty;
        }
    }
}
