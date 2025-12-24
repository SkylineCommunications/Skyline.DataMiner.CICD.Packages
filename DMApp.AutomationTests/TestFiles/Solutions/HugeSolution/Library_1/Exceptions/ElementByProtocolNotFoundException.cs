namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	[Serializable]
	public class ElementByProtocolNotFoundException : Exception
	{
		public ElementByProtocolNotFoundException(string protocolName)
			: base($"Unable to find any active elements with protocol {protocolName} on the DMA.")
		{
		}
	}
}