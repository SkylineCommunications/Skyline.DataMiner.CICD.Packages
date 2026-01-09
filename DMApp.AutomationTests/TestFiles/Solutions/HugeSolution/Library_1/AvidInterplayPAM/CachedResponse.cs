namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class CachedResponse
	{
		public string RequestedFolderPath { get; set; }

		public Response Response { get; set; }
	}
}