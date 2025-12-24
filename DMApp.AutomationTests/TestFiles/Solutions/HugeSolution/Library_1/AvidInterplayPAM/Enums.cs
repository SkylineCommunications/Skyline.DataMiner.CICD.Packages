namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public enum InterplayPamElements
	{
		[Description("IPLAY Helsinki")]
		Helsinki,

		[Description("IPLAY Tampere")]
		Tampere,

		[Description("IPLAY Vaasa")]
		Vaasa,

		[Description("IPLAY UA")]
		UA
	}

	public enum RequestStatus
	{
		Pending = 0,

		Completed = 1,

		Failed = 2
	}
}