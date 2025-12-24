namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation.Sets
{
	public interface ISetToExecute
	{
		string Description { get; }

		int ProtocolReadParameterId { get; }

		object ValueToSet { get; }

		bool ShouldSetValue { get; }

		int NumberOfRetries { get; }

	}
}
