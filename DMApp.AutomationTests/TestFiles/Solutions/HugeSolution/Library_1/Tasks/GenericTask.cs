namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class GenericTask<TResult> : Task
	{
		private readonly string baseDescription;
		private readonly Func<TResult, string> additionalDescription;

		private readonly Func<TResult> function;
		private readonly Func<TResult> rollbackFunction;

		public GenericTask(Helpers helpers, Func<TResult> action, string baseDescription, Func<TResult, string> additionalDescription = null, Func<TResult> rollbackAction = null) : base (helpers)
		{
			this.baseDescription = baseDescription ?? throw new ArgumentNullException(nameof(baseDescription));
			this.function = action ?? throw new ArgumentNullException(nameof(action));
			this.additionalDescription = additionalDescription ?? (result => string.Empty);
			this.rollbackFunction = rollbackAction;
		}

		public override string Description => $"{baseDescription} {additionalDescription(Result)}" ;

		public TResult Result { get; private set; }

		public override Task CreateRollbackTask()
		{
			if (rollbackFunction != null)
			{
				return Task.CreateNew(helpers, rollbackFunction, baseDescription, additionalDescription);
			}
			else
			{
				return null;
			}
		}

		protected override void InternalExecute()
		{
			Result = function.Invoke();
		}
	}
}
