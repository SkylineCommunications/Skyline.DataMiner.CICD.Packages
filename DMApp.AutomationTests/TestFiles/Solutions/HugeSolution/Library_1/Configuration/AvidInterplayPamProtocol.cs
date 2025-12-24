namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration
{
	public static class AvidInterplayPamProtocol
	{
		public static readonly string Name = "Avid Interplay PAM";

		public static class AccessRulesRequestsTable
		{
			public static readonly int PID = 1600;

			public static readonly int StatusColumnPid = 1604;

			public static readonly int ResponseColumnPid = 1605;

			public static readonly int RequestWritePid = 1621;
		}

		public static class FolderRequestsTable
		{
			public static readonly int PID = 1700;

			public static readonly int StatusColumnPid = 1704;

			public static readonly int ResponseColumnPid = 1705;
		}

		public static class Parameter
		{
			public static readonly int ExternalFolderRequest = 1720;
		}
	}
}