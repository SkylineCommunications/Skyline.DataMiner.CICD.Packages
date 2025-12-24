namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DTR
{
	using InterfaceType = Net.ServiceManager.Objects.InterfaceType;

	public class ResourceCapabilityFilter
	{
		public InterfaceType InterfaceType { get; set; }

		public string InterfaceName { get; set; }

		public string CapabilityParameterName { get; set; }

		public string CapabilityParameterValue { get; set; }

		public string FunctionLabel { get; set; }
	}
}