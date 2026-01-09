namespace NonLiveLocalBackup_1
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using Newtonsoft.Json;
	using NonLiveLocalBackup_1.NonLiveSheetHelpers;
	using NPOI.SS.UserModel;
	using NPOI.XSSF.UserModel;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Project;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Transfer;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Ticketing;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Messages;
	using Type = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Type;

	public class NonLiveLocalBackupHandler
	{
		private readonly Helpers helpers;
		private const int AmountOfBackupFoldersToRemain = 2;
		private const string TicketDomainName = "Ingest/Export";

		private readonly TicketingManager ticketingManager;

		private DirectoryInfo exportBackupAttachmentRootDir;
		private DirectoryInfo importBackupAttachmentsRootDir;
		private DirectoryInfo iplayFolderCreationBackupAttachmentsRootDir;
		private DirectoryInfo nonIplayProjectBackupAttachmentsRootDir;
		private DirectoryInfo wgTransferBackupAttachmentsRootDir;

		public NonLiveLocalBackupHandler(Helpers helpers)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			ticketingManager = new TicketingManager(helpers, TicketDomainName);

			Initialize();
		}

		public List<Export> AllExportNonLiveOrders { get; private set; } = new List<Export>();

		public List<Ingest> AllImportNonLiveOrders { get; private set; } = new List<Ingest>();

		public List<FolderCreation> AllIplayFolderCreationNonLiveOrders { get; private set; } = new List<FolderCreation>();

		public List<Project> AllNonIplayProjectNonLiveOrders { get; private set; } = new List<Project>();

		public List<Transfer> AllWgTransferNonLiveOrders { get; private set; } = new List<Transfer>();

		public void GenerateExcelFile()
		{
			try
			{
				var filePath = CreatingFullBackupDirectory();

				IWorkbook workbook = CreateExcelWorkBook();

				using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
				{
					workbook.Write(fs);
				}

				workbook.Close();

				helpers.Engine.SendFileChangeNotification(NotifyType.FileAdd, filePath);
			}
			catch (Exception e)
			{
				helpers.Log(nameof(NonLiveLocalBackupHandler), nameof(GenerateExcelFile), "Unexpected exception occurred during generating excel file: " + e);
			}
		}

		public static void RemoveOutDatedBackupDirectories(Helpers helpers)
		{
			DirectoryInfo mainBackupRoot = new DirectoryInfo(BackupLocations.BackupRootLocation);
			var allBackupDirectories = mainBackupRoot.GetDirectories().OrderByDescending(x => x.CreationTime).ToList();

			if (allBackupDirectories.Count > AmountOfBackupFoldersToRemain)
			{
				var allBackupDirectoriesToDelete = allBackupDirectories.Skip(AmountOfBackupFoldersToRemain);
				foreach (var directory in allBackupDirectoriesToDelete)
				{
					Directory.Delete(directory.FullName, recursive: true);
					helpers.Engine.SendFileChangeNotification(NotifyType.DeleteFolder, directory.FullName);
				}
			}
		}

		private string CreatingFullBackupDirectory()
		{
			DateTime now = DateTime.Now;
			var newBackupRoot = Path.Combine(BackupLocations.BackupRootLocation, String.Format(CultureInfo.InvariantCulture, @"Backup_{0}", now.ToString("yyyyMMdd_HHmm")));
			var backupFileSuffix = String.Format(CultureInfo.InvariantCulture, @"Backup_{0}.xlsx", now.ToString("yyyyMMdd_HHmm"));
			DirectoryInfo newRootDir = new DirectoryInfo(newBackupRoot);

			if (!newRootDir.Exists)
			{
				newRootDir.Create();

				exportBackupAttachmentRootDir = newRootDir.CreateSubdirectory(@"Export_Attachments");
				importBackupAttachmentsRootDir = newRootDir.CreateSubdirectory(@"Import_Attachments");
				iplayFolderCreationBackupAttachmentsRootDir = newRootDir.CreateSubdirectory(@"IplayFolderCreation_Attachments");
				nonIplayProjectBackupAttachmentsRootDir = newRootDir.CreateSubdirectory(@"NonIplayProject_Attachments");
				wgTransferBackupAttachmentsRootDir = newRootDir.CreateSubdirectory(@"WgTransfer_Attachments");
			}

			return Path.Combine(newBackupRoot, backupFileSuffix);
		}

		private void Initialize()
		{
			DateTime now = DateTime.Now;
			var allRetrievedTickets = ticketingManager.GetTicketsWithinTimeFrame(now.AddDays(3).ToUniversalTime()).ToList();

			foreach (var ticket in allRetrievedTickets)
			{
				if (ticket != null && helpers.NonLiveOrderManager.TryGetNonLiveOrder(ticket.ID.DataMinerID, ticket.ID.TID, out NonLiveOrder retrievedNonLiveOrder) && retrievedNonLiveOrder != null)
				{
					Type type = (Type)ticket.GetIntegerFieldValue(NonLiveOrder.TypeTicketField);
					switch (type)
					{
						case Type.Export:
							AllExportNonLiveOrders.Add((Export)retrievedNonLiveOrder);
							break;
						case Type.Import:
							AllImportNonLiveOrders.Add((Ingest)retrievedNonLiveOrder);
							break;
						case Type.IplayFolderCreation:
							AllIplayFolderCreationNonLiveOrders.Add((FolderCreation)retrievedNonLiveOrder);
							break;
						case Type.IplayWgTransfer:
							AllWgTransferNonLiveOrders.Add((Transfer)retrievedNonLiveOrder);
							break;
						case Type.NonInterplayProject:
							AllNonIplayProjectNonLiveOrders.Add((Project)retrievedNonLiveOrder);
							break;
						default:
							helpers.Log(nameof(NonLiveLocalBackupHandler), nameof(Initialize), $"No matching type found for ticket: {ticket.ID}");
							break;
					}
				}
			}
		}

		private IWorkbook CreateExcelWorkBook()
		{
			IWorkbook workBook = new XSSFWorkbook();
			List<NonLiveSheetHelper> allSheetHelpers = InitializeAllSheetHelpers();

			CreateAllSheets(allSheetHelpers, workBook);

			if (workBook.NumberOfSheets == 0)
			{
				// Workbook always needs to have 1 sheet.
				workBook.CreateSheet();
			}

			return workBook;
		}

		private List<NonLiveSheetHelper> InitializeAllSheetHelpers()
		{
			var allSheetHelpers = new List<NonLiveSheetHelper>();

			var exportSheetHelper = new ExportSheetHelper(helpers, AllExportNonLiveOrders, ColumnModels.ColumnModels.ExportColumnModel, exportBackupAttachmentRootDir);
			allSheetHelpers.Add(exportSheetHelper);

			var importSheetHelper = new ImportSheetHelper(helpers, AllImportNonLiveOrders, ColumnModels.ColumnModels.ImportColumnModel, importBackupAttachmentsRootDir);
			allSheetHelpers.Add(importSheetHelper);

			var iPlayFolderCreationSheetHelper = new IplayFolderCreationSheetHelper(helpers, AllIplayFolderCreationNonLiveOrders, ColumnModels.ColumnModels.IplayFolderCreationColumnModel, iplayFolderCreationBackupAttachmentsRootDir);
			allSheetHelpers.Add(iPlayFolderCreationSheetHelper);

			var iPlayFolderCreationDeleteDateSheetHelper = new IplayFolderCreationDeleteDateSheetHelper(helpers, AllIplayFolderCreationNonLiveOrders, ColumnModels.ColumnModels.IplayFolderCreationDeletionDateColumnModel, iplayFolderCreationBackupAttachmentsRootDir);
			allSheetHelpers.Add(iPlayFolderCreationDeleteDateSheetHelper);

			var nonIplayProjectSheetHelper = new NonIplayProjectSheetHelper(helpers, AllNonIplayProjectNonLiveOrders, ColumnModels.ColumnModels.NonIplayProjectColumnModel, nonIplayProjectBackupAttachmentsRootDir);
			allSheetHelpers.Add(nonIplayProjectSheetHelper);

			var wgTransferSheetHelper = new WgTransferSheetHelper(helpers, AllWgTransferNonLiveOrders, ColumnModels.ColumnModels.WgTransferColumnModel, wgTransferBackupAttachmentsRootDir);
			allSheetHelpers.Add(wgTransferSheetHelper);

			return allSheetHelpers;
		}

		private static void CreateAllSheets(List<NonLiveSheetHelper> allSheetHelpers, IWorkbook workBook)
		{
			foreach (var sheetHelper in allSheetHelpers)
			{
				sheetHelper.CreateSheet(workBook);
				if (sheetHelper.CopyAttachmentsAllowed) sheetHelper.CopyOrderAttachments();
			}
		}
	}
}