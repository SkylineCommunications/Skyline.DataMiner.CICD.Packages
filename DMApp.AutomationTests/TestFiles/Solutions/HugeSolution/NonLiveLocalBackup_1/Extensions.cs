namespace NonLiveLocalBackup_1
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Project;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Transfer;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Ticketing;

	public static class Extensions
	{
		public static Dictionary<string, string> GetExportProperties(this Export exportOrder)
		{
			if (exportOrder == null) return new Dictionary<string, string>();

			string sourcePath = string.Empty;
			switch (exportOrder.MaterialSource)
			{
				case Sources.INTERPLAY_PAM:
					sourcePath = string.Join(";", exportOrder.InterplayPamExport.FolderUrls);
					break;
				case Sources.MEDIAPARKKI:
					sourcePath = string.Join(";", exportOrder.MediaParkkiExport.SourceFolderUrls);
					break;
				default:
					// Do nothing
					break;
			}

			Dictionary<string, string> props = new Dictionary<string, string>
			{
				{ "Id", string.Join("/", exportOrder.DataMinerId, exportOrder.TicketId) },
				{ "Order Name", exportOrder.OrderDescription },
				{ "Description", exportOrder.ShortDescription },
				{ "Source System",  EnumExtensions.GetDescriptionFromEnumValue(exportOrder.MaterialSource) },
				{ "Source Path",  sourcePath },
				{ "Export Department" , exportOrder.ExportInformation?.ExportDepartment },
				{ "Export Type", exportOrder.InterplayPamExport != null ? EnumExtensions.GetDescriptionFromEnumValue(exportOrder.InterplayPamExport.ExportFileType) : string.Empty },
				{ "Export Target", exportOrder.ExportInformation?.TargetOfExport },
				{ "Logo/Time Code/Subtitles", exportOrder.ExportInformation?.ToString() },
				{ "Created By", exportOrder.CreatedBy },
				{ "Responsible", exportOrder.Owner },
				{ "Deadline", exportOrder.Deadline.ToString(CultureInfo.InvariantCulture) },
			};

			return props;
		}

		public static Dictionary<string, string> GetImportProperties(this Ingest importOrder)
		{
			if (importOrder == null) return new Dictionary<string, string>();

			Dictionary<string, string> props = new Dictionary<string, string>
			{
				{ "Id", string.Join("/", importOrder.DataMinerId, importOrder.TicketId) },
				{ "Order Name", importOrder.OrderDescription },
				{ "Description", importOrder.ShortDescription },
				{ "Material Reception Time", importOrder.DeliveryTime.ToString(CultureInfo.InvariantCulture) },
				{ "Material Details", importOrder.ToString() },
				{ "Deadline", importOrder.Deadline.ToString(CultureInfo.InvariantCulture) },
				{ "Target System" , importOrder.IngestDestination.Destination },
				{ "Target Path", importOrder.IngestDestination.InterplayDestinationFolder },
				{ "Created By" , importOrder.CreatedBy },
				{ "Responsible", importOrder.Owner },
				{ "Interplay format", importOrder.InterplayFormat },
				{ "Multicamera material", importOrder.MultiCameraMaterial.ToString()},
				{ "Additional information", importOrder.AdditionalInformation},
				{ "Do you want to store your backups longer than one year?", importOrder.BackUpsLongerStored.ToString()},
				{ "Backup deletion date", importOrder.BackupDeletionDate.ToString(CultureInfo.InvariantCulture)},
				{ "Why must the backup be stored longer?", importOrder.WhyBackUpLongerStored}
			};

			return props;
		}

		public static Dictionary<string, string> GetIplayFolderCreationProperties(this FolderCreation folderCreationOrder)
		{
			if (folderCreationOrder == null) return new Dictionary<string, string>();

			Dictionary<string, string> props = new Dictionary<string, string>
			{
				{ "Id", string.Join("/", folderCreationOrder.DataMinerId, folderCreationOrder.TicketId) },
				{ "Order Name", folderCreationOrder.OrderDescription },
				{ "Description", folderCreationOrder.ShortDescription },
				{ "Target System", folderCreationOrder.Destination },
				{ "Target Path", folderCreationOrder.ParentFolder },
				{ "Content Type", folderCreationOrder.ContentType },
				{ "Created By", folderCreationOrder.CreatedBy },
				{ "Responsible", folderCreationOrder.Owner },
			};

			return props;
		}

		public static Dictionary<string, string> GetIplayFolderCreationDeletionDateProperties(this FolderCreation folderCreationOrder, NewEpisodeFolderRequestDetails newEpisodeFolderRequestDetails = null, NewProgramFolderRequestDetails newProgramFolderRequestDetails = null)
		{
			if (folderCreationOrder == null) return new Dictionary<string, string>();

			string programDeleteDateValue = newProgramFolderRequestDetails != null && newProgramFolderRequestDetails.IsDeleteDateUnknown ? "Unknown" : newProgramFolderRequestDetails?.DeleteDate.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

			Dictionary<string, string> props = new Dictionary<string, string>
			{
				{ "Id", string.Join("/", folderCreationOrder.DataMinerId, folderCreationOrder.TicketId) },
				{ "Order Name", folderCreationOrder.OrderDescription },
				{ "Category", EnumExtensions.GetDescription(folderCreationOrder.OrderType) },
				{ "Target System", folderCreationOrder.Destination },
				{ "Target Path", folderCreationOrder.ParentFolder },
				{ "Content Type", newEpisodeFolderRequestDetails != null ? NewFolderContentTypes.EPISODE.GetDescription() : NewFolderContentTypes.PROGRAM.GetDescription() },
				{ "product or production number", newProgramFolderRequestDetails?.ProductNumber },
				{ "Episode number or name", newEpisodeFolderRequestDetails?.EpisodeNumberOrName },
				{ "Delete date", newEpisodeFolderRequestDetails?.DeleteDate.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? programDeleteDateValue },
				{ "Producer email", newEpisodeFolderRequestDetails?.ProducerEmail ?? newProgramFolderRequestDetails?.ProducerEmail },
				{ "Media manager email", newEpisodeFolderRequestDetails?.MediaManagerEmail ?? newProgramFolderRequestDetails?.MediaManagerEmail },
				{ "Additional customer information", folderCreationOrder.AdditionalInformation },
			};

			return props;
		}

		public static Dictionary<string, string> GetNonIplayProjectProperties(this Project projectOrder)
		{
			if (projectOrder == null) return new Dictionary<string, string>();

			Dictionary<string, string> props = new Dictionary<string, string>
			{
				{ "Id", string.Join("/", projectOrder.DataMinerId, projectOrder.TicketId) },
				{ "Order Name", projectOrder.OrderDescription },
				{ "Description", projectOrder.ShortDescription },
				{ "Ingest Department", projectOrder.ImportDepartment },
				{ "Production Department Name", projectOrder.ProductionDepartmentName },
				{ "Project Type", projectOrder.ProjectType },
				{ "Avid Project Video Format", projectOrder.AvidProjectVideoFormat },
				{ "Project Name", projectOrder.ProjectName },
				{ "Deadline", projectOrder.Deadline.ToString(CultureInfo.InvariantCulture) },
			};

			return props;
		}

		public static Dictionary<string, string> GetWgTransferProperties(this Transfer transferOrder, Helpers helpers)
		{
			if (transferOrder == null) return new Dictionary<string, string>();

			var userTasks = helpers.NonLiveUserTaskManager.GetNonLiveUserTasks(transferOrder).ToArray();

			Dictionary<string, string> props = new Dictionary<string, string>
			{
				{ "Id", string.Join("/", transferOrder.DataMinerId, transferOrder.TicketId) },
				{ "Order Name", transferOrder.OrderDescription },
				{ "Description", transferOrder.ShortDescription },
				{ "Material Source", transferOrder.Source },
				{ "Interplay Source Folder", transferOrder.SourceFolderUrls == null ? string.Empty : string.Join(";", transferOrder.SourceFolderUrls) },
				{ "Destination of Transfer", transferOrder.Destination },
				{ "Interplay Destination Folder", transferOrder.InterplayDestinationFolder },
				{ "Received Email Address", transferOrder.ReceiverEmailAddress },
				{ "User Task 1 Description" , userTasks != null && userTasks.Length >= 1 ? userTasks[0].Description : string.Empty },
				{ "User Task 1 Status" , userTasks != null && userTasks.Length >= 1 ? EnumExtensions.GetDescriptionFromEnumValue(userTasks[0].Status) : string.Empty },
				{ "User Task 2 Description", userTasks != null && userTasks.Length >= 2 ? userTasks[1].Description : string.Empty },
				{ "User Task 2 Status", userTasks != null && userTasks.Length >= 2 ? EnumExtensions.GetDescriptionFromEnumValue(userTasks[1].Status) : string.Empty },
				{ "User Task 3 Description", userTasks != null && userTasks.Length >= 3 ? userTasks[2].Description : string.Empty },
				{ "User Task 3 Status", userTasks != null && userTasks.Length >= 3 ? EnumExtensions.GetDescriptionFromEnumValue(userTasks[2].Status) : string.Empty },
			};

			return props;
		}
	}
}