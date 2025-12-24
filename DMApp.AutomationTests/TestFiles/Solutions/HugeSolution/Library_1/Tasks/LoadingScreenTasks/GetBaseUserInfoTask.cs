using Skyline.DataMiner.Automation;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks
{
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class GetBaseUserInfoTask : Task
	{
		private readonly string userLoginName;

		public GetBaseUserInfoTask(Helpers helpers) : base(helpers)
		{
			this.userLoginName = base.helpers.Engine.UserLoginName;
			IsBlocking = true;
		}

		public UserInfo UserInfo { get; private set; }

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			UserInfo = helpers.ContractManager.GetBaseUserInfo(userLoginName);
		}

		public override string Description => "Getting user info for " + userLoginName;
	}
}
