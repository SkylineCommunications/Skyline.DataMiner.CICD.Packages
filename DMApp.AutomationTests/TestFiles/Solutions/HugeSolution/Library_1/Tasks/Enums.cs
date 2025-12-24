using System.ComponentModel;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks
{
	public enum Status
	{
		[Description("Fail")]
		Fail = 0,

		[Description("OK")]
		Ok = 1,

		[Description("Not Started")]
		NotStarted = 2
	}

    public enum ResourceScriptAction
    {
        Assign,
        Release
    }
}