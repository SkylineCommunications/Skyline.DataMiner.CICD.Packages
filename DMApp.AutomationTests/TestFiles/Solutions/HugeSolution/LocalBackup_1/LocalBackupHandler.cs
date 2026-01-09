namespace LocalBackup_1
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using NPOI.SS.UserModel;
	using NPOI.XSSF.UserModel;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports.Locations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Messages;

	public class LocalBackupHandler
	{
		private readonly Helpers helpers;
		private readonly Dictionary<string, int> allCompanies;

		private const int amountOfBackupFoldersToRemain = 2;
		private const string sheetNameForEvents = "Events";
		private const string sheetNameForOrders = "Orders";
		private const string sheetNameForServices = "Services";

		private DirectoryInfo eventBackupAttachmentRootDir;
		private DirectoryInfo orderBackupAttachmentsRootDir;

		private readonly Dictionary<int, string> eventColumnModel = new Dictionary<int, string>
		{
			{ 0, "Id" },
			{ 1, "Name" },
			{ 2, "Start Time" },
			{ 3, "End Time" },
			{ 4, "Event Level RX" },
			{ 5, "Info" },
			{ 6, "Customer" },
			{ 7, "Contract" },
			{ 8, "Visibility Rights" },
			{ 9, "Status" },
			{ 10, "Attachments" },
		};

		private readonly Dictionary<int, string> orderColumnModel = new Dictionary<int, string>
		{
			{ 0, "Id" },
			{ 1, "Name" },
			{ 2, "Start Time" },
			{ 3, "End Time" },
			{ 4, "Additional Information" },
			{ 5, "Created By" },
			{ 6, "Customer" },
			{ 7, "Status" },
			{ 8, "Comments" },
			{ 9, "Attachments" },
			{ 10, "Linked Event Id" },
			{ 11, "Billable Company" },
			{ 12, "Customer Company" },
			{ 13, "Sources" },
			{ 14, "Destinations" },
			{ 15, "Recordings" },
			{ 16, "Transmissions" },
		};

		private readonly Dictionary<int, string> serviceColumnModel = new Dictionary<int, string>
		{
			{ 0, "Id" },
			{ 1, "Name" },
			{ 2, "Start Time" },
			{ 3, "End Time" },
			{ 4, "Technology" },
			{ 5, "Tech Details" },
			{ 6, "Audio Format Details" },
			{ 7, "Status" },
			{ 8, "Description" },
			{ 9, "Recording Location"},
			{ 10, "Recording Signal"},
			{ 11, "Recording Time Codec"},
			{ 12, "Recording Video Codec"},
			{ 13, "Plasma ID for Archive" },
			{ 14, "Additional Recording Needs"},
			{ 15, "Recording File Destination" },
			{ 16, "Recording File Destination Path" },
			{ 17, "Target" },
			{ 18, "Linked Order Ids" },
			{ 19, "User Task 1 Description" },
			{ 20, "User Task 1 Status" },
			{ 21, "User Task 2 Description" },
			{ 22, "User Task 2 Status" },
			{ 23, "User Task 3 Description" },
			{ 24, "User Task 3 Status" },
		};

		public LocalBackupHandler(Helpers helpers)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			allCompanies = helpers.ContractManager.GetAllCompanySecurityViewIds();

			Initialize();
		}

		public List<Order> AllRetrievedOrders { get; private set; }

		public List<Event> AllEvents { get; private set; }

		public List<Service> AllServices { get; private set; }

		public List<Service> AllEventLevelReceptions { get; private set; }

		public IWorkbook Workbook { get; private set; }

		public void GenerateExcelFile()
		{
			try
			{
				DateTime now = DateTime.Now;
				var newBackupRoot = Path.Combine(DirectoryLocations.BackupLocation, String.Format(CultureInfo.InvariantCulture, @"Backup_{0}", now.ToString("yyyyMMdd_HHmm")));
				string backupFileSuffix = String.Format(CultureInfo.InvariantCulture, @"Backup_{0}.xlsx", now.ToString("yyyyMMdd_HHmm"));

				DirectoryInfo newRootDir = new DirectoryInfo(newBackupRoot);
				if (!newRootDir.Exists)
				{
					newRootDir.Create();

					eventBackupAttachmentRootDir = newRootDir.CreateSubdirectory(@"Event_Attachments");
					orderBackupAttachmentsRootDir = newRootDir.CreateSubdirectory(@"Order_Attachments");
				}
				
				Workbook = CreateExcel();

				var filePath = Path.Combine(newBackupRoot, backupFileSuffix);
				using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
				{
					Workbook.Write(fs);
				}

				Workbook.Close();

				helpers.Engine.SendFileChangeNotification(NotifyType.FileAdd, filePath);

				RemoveOutDatedBackupDirectories(helpers);
			}
			catch (Exception e)
			{
				helpers.Engine.ExitFail("Unexpected exception occurred during generating excel file: " + e);
			}
		}

		private void Initialize()
		{
			DateTime now = DateTime.Now;
			AllRetrievedOrders = helpers.OrderManager.GetAllOrdersWithinTimeFrame(now, now.AddDays(3));

			AllEvents = AllRetrievedOrders.Select(x => x.Event).ToList();

			AllEventLevelReceptions = new List<Service>();
			foreach (var order in AllRetrievedOrders)
			{
				var eventLevelReceptionSource = order.Sources.FirstOrDefault(s => s != null && s.BackupType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.BackupType.None && s.IsSharedSource);
				if (eventLevelReceptionSource != null) AllEventLevelReceptions.Add(eventLevelReceptionSource);
			}

			AllServices = AllRetrievedOrders.SelectMany(x => x.AllServices).OrderBy(x => x.Start).ToList();
		}

		private IWorkbook CreateExcel()
		{
			IWorkbook workBook = new XSSFWorkbook();

			if (AllEvents != null && AllEvents.Any())
			{
				EventSheetHelper eventSheetHelper = new EventSheetHelper(helpers.Engine, workBook, AllEvents, eventColumnModel, eventBackupAttachmentRootDir, allCompanies, AllEventLevelReceptions);

				eventSheetHelper.CreateSheet(sheetNameForEvents);
			}

			if (AllRetrievedOrders != null && AllRetrievedOrders.Any())
			{
				OrderSheetHelper orderSheetHelper = new OrderSheetHelper(helpers.Engine, workBook, AllRetrievedOrders, orderColumnModel, orderBackupAttachmentsRootDir);
				orderSheetHelper.CreateSheet(sheetNameForOrders);
			}

			if (AllServices != null && AllServices.Any() && AllRetrievedOrders != null && AllRetrievedOrders.Any())
			{
				ServiceSheetHelper serviceSheetHelper = new ServiceSheetHelper(helpers.Engine, workBook, AllServices, AllRetrievedOrders, serviceColumnModel);
				serviceSheetHelper.CreateSheet(sheetNameForServices);
			}

			if (workBook.NumberOfSheets == 0)
			{
				// Workbook always needs to have 1 sheet.
				workBook.CreateSheet();
			}

			return workBook;
		}

		private static void RemoveOutDatedBackupDirectories(Helpers helpers)
		{
			DirectoryInfo mainBackupRoot = new DirectoryInfo(DirectoryLocations.BackupLocation);
			var allBackupDirectories = mainBackupRoot.GetDirectories().OrderByDescending(x => x.CreationTime).ToList();
			if (allBackupDirectories.Count > amountOfBackupFoldersToRemain)
			{
				var allBackupDirectoriesToDelete = allBackupDirectories.Skip(amountOfBackupFoldersToRemain);
				foreach (var directory in allBackupDirectoriesToDelete)
				{
					Directory.Delete(directory.FullName, recursive: true);
					helpers.Engine.SendFileChangeNotification(NotifyType.DeleteFolder, directory.FullName);
				}
			}
		}
	}
}