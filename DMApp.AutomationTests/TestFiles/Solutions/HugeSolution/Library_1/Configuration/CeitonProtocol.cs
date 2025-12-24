namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration
{
	public static class CeitonProtocol
	{
		public static readonly string Name = "Ceiton Resource Planning";

		public static class ProjectTasksTable
		{
			public static readonly int PID = 1200;

			public static readonly int ProjectIdIdx = 1;

			public static readonly int ProjectIdPid = 1202;

			public static readonly int StartTimeIdx = 5;

			public static readonly int StartTimePid = 1206;

			public static readonly int EndTimeIdx = 6;

			public static readonly int EndTimePid = 1207;

			public static readonly int ActivityTypeNameIdx = 9;

			public static readonly int ActivityTypeNamePid = 1210;
		}

		public static class ProductTasksTable
		{
			public static readonly int PID = 1300;

			public static readonly int ProjectIdIdx = 1;

			public static readonly int ProjectIdPid = 1302;

			public static readonly int StartTimeIdx = 5;

			public static readonly int StartTimePid = 1306;

			public static readonly int EndTimeIdx = 6;

			public static readonly int EndTimePid = 1307;

			public static readonly int ActivityTypeNameIdx = 9;

			public static readonly int ActivityTypeNamePid = 1310;
		}

		public static class AdHocTasksTable
		{
			public static readonly int PID = 1400;

			public static readonly int StartTimeIdx = 5;

			public static readonly int EndTimeIdx = 6;

			public static readonly int ActivityTypeNameIdx = 8;
		}
	}
}