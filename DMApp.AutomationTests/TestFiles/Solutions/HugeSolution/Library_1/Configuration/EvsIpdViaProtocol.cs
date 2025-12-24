using System.ComponentModel;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration
{
	public static class EvsIpdViaProtocol
	{
		public static readonly string Name = "EVS IPD-VIA";

		public static class RecordersTable
		{
			public static readonly int TablePid = 1000;

			public static class Idx
			{
				public static readonly int RecordersName = 1;
			}
		}

		public static class TargetsTable
		{
			public static readonly int TablePid = 1100;

			public static class Idx
			{
				public static readonly int TargetsName = 1;
			}
		}

		public static class RecordingSessionsTable
		{
			public static readonly int TablePid = 1400;

			public static readonly int RecordingSessionsName = 1402;

			public static class Idx
			{
				public static readonly int RecordingSessionsInstanceIdx = 0;

				public static readonly int RecordingSessionsNameIdx = 1;

				public static readonly int RecordingSessionsStatusIdx = 2;

				public static readonly int RecordingSessionsStartIdx = 3;

				public static readonly int RecordingSessionsEndIdx = 4;

				public static readonly int RecordingSessionsRecorderIdx = 5;
			}
		}

		public class ProfileFieldsTable
		{
			/// <summary>PID: 1900</summary>
			public static readonly int TablePid = 1900;

			public class Pid
			{
				public static readonly int ProfileFieldsInstance = 1901;

				public static readonly int ProfileFieldsKey = 1902;

				public static readonly int ProfileFieldsLabel = 1903;

				public static readonly int ProfileFieldsType = 1904;

				public static readonly int ProfileFieldsRequired = 1905;

				public static readonly int ProfileFieldsValueConstraints = 1906;

				public static readonly int ProfileFieldsPredefinedValue = 1907;

				public static readonly int ProfileFieldsProfileFqn = 1908;

				public static readonly int ProfileFieldsProfileName = 1909;

				public static readonly int ProfileFieldsDisplayKey = 1910;

				public static readonly int ProfileFieldsFqnContraints = 1911;
			}

			public class Idx
			{
				public static readonly int ProfileFieldsInstance = 0;

				public static readonly int ProfileFieldsKey = 1;

				public static readonly int ProfileFieldsLabel = 2;

				public static readonly int ProfileFieldsType = 3;

				public static readonly int ProfileFieldsRequired = 4;

				public static readonly int ProfileFieldsValueConstraints = 5;

				public static readonly int ProfileFieldsPredefinedValue = 6;

				public static readonly int ProfileFieldsProfileFqn = 7;

				public static readonly int ProfileFieldsProfileName = 8;

				public static readonly int ProfileFieldsDisplayKey = 9;

				public static readonly int ProfileFieldsFqnContraints = 10;
			}
		}

		public static class RecordingSessionsTargetsTable
		{
			public static readonly int TablePid = 1500;

			public class Pid
			{
				public static readonly int RecordingSessionsTargetsInstance = 1501;

				public static readonly int RecordingSessionsTargetsRecordingSessionDisplayKey = 1502;

				public static readonly int RecordingSessionsTargetsRecordingSessionInstance = 1503;

				public static readonly int RecordingSessionsTargetsRecordingSession = 1504;

				public static readonly int RecordingSessionsTargetsTargetInstance = 1505;

				public static readonly int RecordingSessionsTargetsTarget = 1506;
			}

			public class Idx
			{
				public static readonly int RecordingSessionsTargetsInstance = 0;

				public static readonly int RecordingSessionsTargetsRecordingSessionDisplayKey = 1;

				public static readonly int RecordingSessionsTargetsRecordingSessionInstance = 2;

				public static readonly int RecordingSessionsTargetsRecordingSession = 3;

				public static readonly int RecordingSessionsTargetsTargetInstance = 4;

				public static readonly int RecordingSessionsTargetsTarget = 5;
			}
		}

		public class RecordingSessionsMetadataValuesTable
		{
			public static readonly int TablePid = 1700;

			public class Pid
			{
				public static readonly int RecordingSessionsMetadataValuesInstance = 1701;

				public static readonly int RecordingSessionsMetadataValuesKey = 1702;

				public static readonly int RecordingSessionsMetadataValuesValue = 1703;

				public static readonly int RecordingSessionsMetadataValuesRecordingSession = 1704;

				public static readonly int RecordingSessionsMetadataValuesRecordingSessionId = 1705;

				public static readonly int RecordingSessionsMetadataValuesDisplayKey = 1706;

				public static readonly int RecordingSessionsMetadataValuesProfile = 1707;

				public static readonly int RecordingSessionsMetadataValuesProfileId = 1708;
			}

			public class Idx
			{

				public static readonly int RecordingSessionsMetadataValuesInstance = 0;

				public static readonly int RecordingSessionsMetadataValuesKey = 1;

				public static readonly int RecordingSessionsMetadataValuesValue = 2;

				public static readonly int RecordingSessionsMetadataValuesRecordingSession = 3;

				public static readonly int RecordingSessionsMetadataValuesRecordingSessionId = 4;

				public static readonly int RecordingSessionsMetadataValuesDisplayKey = 5;

				public static readonly int RecordingSessionsMetadataValuesProfile = 6;

				public static readonly int RecordingSessionsMetadataValuesProfileId = 7;
			}
		}
	}
}
