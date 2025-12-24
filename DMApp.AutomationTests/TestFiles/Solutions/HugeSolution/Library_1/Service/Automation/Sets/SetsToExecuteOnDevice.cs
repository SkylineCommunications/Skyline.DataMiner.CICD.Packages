namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation.Sets
{
	using System.Collections.Generic;
	using Skyline.DataMiner.Automation;

	public class SetsToExecuteOnDevice
	{
		public Element ResourceElement { get; set; }

		public List<ISetToExecute> SetsToExecute { get; set; } = new List<ISetToExecute>();
	}
}
