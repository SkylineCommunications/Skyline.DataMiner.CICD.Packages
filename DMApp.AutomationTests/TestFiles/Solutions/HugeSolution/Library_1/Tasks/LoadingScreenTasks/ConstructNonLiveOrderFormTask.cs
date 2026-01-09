using System;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens.LoadNonLiveOrderFormDialog;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks
{
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class ConstructNonLiveOrderFormTask : Task
	{
		private readonly UserInfo userInfo;
		private readonly NonLiveOrder nonLiveOrder;
        private readonly ScriptAction scriptAction;

		public ConstructNonLiveOrderFormTask(Helpers helpers, UserInfo userInfo, NonLiveOrder nonLiveOrder, ScriptAction scriptAction) : base(helpers)
		{
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			this.nonLiveOrder = nonLiveOrder;
            this.scriptAction = scriptAction;

			IsBlocking = true;
		}

		public MainDialog NonLiveOrderForm { get; private set; }

		protected override void InternalExecute()
		{
			NonLiveOrderForm = new MainDialog(helpers, userInfo, scriptAction, nonLiveOrder);
		}
		public override Task CreateRollbackTask()
		{
			return null;
		}

		public override string Description => "Building UI";
	}
}
