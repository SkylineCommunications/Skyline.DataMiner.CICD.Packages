namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation.Sets
{
	public class SetToExecute : ISetToExecute
	{
		public string Description { get; set; }

		public int ProtocolReadParameterId { get; set; }

		public object ValueToSet { get; set; }

		public bool ShouldSetValue => true;

		public int NumberOfRetries => 4;
	}
}
