namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System.ComponentModel;

	public enum UserTaskSource
	{
		LiveService,

		NonLiveOrder
	}

	public enum UserTaskStatus
	{
		[Description("Incomplete")]
		Incomplete = 1,

		[Description("Complete")]
		Complete = 2,

		[Description("Delete Date Near")]
		DeleteDateNear = 3,

		[Description("Deletion In Progress")]
		DeletionInProgress = 4,

		[Description("Folder Deleted")]
		FolderDeleted = 5,

		[Description("Backup Delete Date Near")]
		BackupDeleteDateNear = 6,

		[Description("Backup Deleted")]
		BackupDeleted = 7,

		[Description("Pending")]
		Pending = 8,
	}

	public enum UserGroup
	{
		[Description("None")]
		None = 0,

		[Description("Booking Office")]
		BookingOffice = 1,

		[Description("MCR Operator")]
		McrOperator = 2,

		[Description("Fiber Specialist")]
		FiberSpecialist = 3,

		[Description("MW Specialist")]
		MwSpecialist = 4,

		[Description("Media Operator")]
		MediaOperator = 5,

		[Description("Audio MCR Operator")]
		AudioMcrOperator = 6,

		[Description("Messi Specific User Group")]
		MessiSpecific = 7,

		[Description("Mediamylly Specific User Group")]
		MediamyllySpecific = 8,

		[Description("Mediaputiikki Specific User Group")]
		MediaputiikkiSpecific = 9,

		[Description("UA Specific User Group")]
		UaSpecific = 10,

		[Description("TOM")]
		Tom = 11,
	}
}