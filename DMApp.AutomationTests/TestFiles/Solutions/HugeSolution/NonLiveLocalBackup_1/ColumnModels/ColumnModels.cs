using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonLiveLocalBackup_1.ColumnModels
{
	public static class ColumnModels
	{
		internal static readonly Dictionary<int, string> ExportColumnModel = new Dictionary<int, string>
		{
			{ 0, "Id" },
			{ 1, "Order Name" },
			{ 2, "Description" },
			{ 3, "Source System" },
			{ 4, "Source Path" },
			{ 5, "Export Department" },
			{ 6, "Export Type" },
			{ 7, "Export Target" },
			{ 8, "Logo/Time Code/Subtitles" },
			{ 9, "Created By" },
			{ 10, "Responsible" },
			{ 11, "Deadline" },
		};

		internal static readonly Dictionary<int, string> ImportColumnModel = new Dictionary<int, string>
		{
			{ 0, "Id" },
			{ 1, "Order Name" },
			{ 2, "Description" },
			{ 3, "Material Reception Time" },
			{ 4, "Material Details" },
			{ 5, "Deadline" },
			{ 6, "Target System" },
			{ 7, "Target Path" },
			{ 8, "Created By" },
			{ 9, "Responsible" },
			{ 10, "Interplay format"},
			{ 11, "Multicamera material"},
			{ 12, "Additional information"},
			{ 13, "Do you want to store your backups longer than one year?"},
			{ 14, "Backup deletion date"},
			{ 15, "Why must the backup be stored longer?"}
		};

		internal static readonly Dictionary<int, string> IplayFolderCreationColumnModel = new Dictionary<int, string>
		{
			{ 0, "Id" },
			{ 1, "Order Name" },
			{ 2, "Description" },
			{ 3, "Target System" },
			{ 4, "Target Path" },
			{ 5, "Content Type" },
			{ 6, "Created By" },
			{ 7, "Responsible" },
		};

		internal static readonly Dictionary<int, string> IplayFolderCreationDeletionDateColumnModel = new Dictionary<int, string>
		{
			{ 0, "Id" },
			{ 1, "Order Name" },
			{ 2, "Category" },
			{ 3, "Target System" },
			{ 4, "Target Path" },
			{ 5, "Content Type" },
			{ 6, "product or production number" },
			{ 7, "Episode number or name" },
			{ 8, "Delete date" },
			{ 9, "Producer email" },
			{ 10, "Media manager email" },
			{ 11, "Additional customer information" },
		};

		internal static readonly Dictionary<int, string> NonIplayProjectColumnModel = new Dictionary<int, string>
		{
			{ 0, "Id" },
			{ 1, "Order Name" },
			{ 2, "Description" },
			{ 3, "Ingest Department" },
			{ 4, "Production Department Name" },
			{ 5, "Project Type" },
			{ 6, "Avid Project Video Format" },
			{ 7, "Project Name" },
			{ 8, "Deadline" },
		};

		internal static readonly Dictionary<int, string> WgTransferColumnModel = new Dictionary<int, string>
		{
			{ 0, "Id" },
			{ 1, "Order Name" },
			{ 2, "Description" },
			{ 3, "Material Source" },
			{ 4, "Interplay Source Folder" },
			{ 5, "Destination of Transfer" },
			{ 6, "Interplay Destination Folder" },
			{ 7, "Received Email Address" },
			{ 8, "User Task 1 Description" },
			{ 9, "User Task 1 Status" },
			{ 10, "User Task 2 Description" },
			{ 11, "User Task 2 Status" },
			{ 12, "User Task 3 Description" },
			{ 13, "User Task 3 Status" },
		};
	}
}
