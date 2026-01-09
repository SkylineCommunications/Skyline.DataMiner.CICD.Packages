namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;

	public class GetUserInfoTask : Task
	{
		private readonly string userLoginName;
		private readonly Event _event;

		public GetUserInfoTask(Helpers helpers) : this(helpers, null)
		{

		}

		public GetUserInfoTask(Helpers helpers, Event @event) : base(helpers)
		{
			this.userLoginName = base.helpers.Engine.UserLoginName;
			this._event = @event;
		}

		protected override void InternalExecute()
		{
			UserInfo = helpers.ContractManager.GetUserInfo(userLoginName, _event);

			Log(nameof(InternalExecute), $"Retrieved user info for user='{UserInfo.User.Name}', Contract='{UserInfo.Contract.Name}'");
		}

		public override Task CreateRollbackTask()
		{
			return null;
		}

		public override string Description => "Getting user info for " + userLoginName;

		public UserInfo UserInfo { get; private set; }
	}
}
