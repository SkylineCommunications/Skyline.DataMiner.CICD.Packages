namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DTR
{
	using Skyline.DataMiner.Net.Messages;

	public class ResourceUpdateInfo
	{
		public string ServiceDefinitionName { get; set; }

		public string UpdatedResourceFunctionLabel { get; set; }

		public Resource UpdatedResource { get; set; }

		/// <summary>
		/// In case we also need to take a connected resource into account then make sure that resource is referenced.
		/// </summary>
		public Resource ConnectedResource { get; set; }
	}
}