namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using NPOI.SS.UserModel;
	using NPOI.XSSF.UserModel;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports.Locations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public class InvoiceReportHandler
    {
        private readonly Dictionary<string, int> allCompanies;
		private readonly Helpers helpers;
		private readonly string requestedCompany;
        private readonly DateTime requestedStartTime;
        private readonly DateTime requestedEndTime;

        private const string sheetNameForEvents = "Events";
        private const string sheetNameForOrders = "Orders";
        private const string sheetNameForServices = "Services";

        private DirectoryInfo eventReportAttachmentRootDir;
        private DirectoryInfo orderReportAttachmentsRootDir;

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
            { 17, "Processing" },
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

        public InvoiceReportHandler(Helpers helpers, string requestedCompany, DateTime startTime, DateTime endTime)
        {
			this.helpers = helpers;
			this.requestedCompany = requestedCompany;
            this.requestedStartTime = startTime;
            this.requestedEndTime = endTime;

            allCompanies = helpers.ContractManager.GetAllCompanySecurityViewIds();

            Initialize();
        }

        public List<Order> AllRetrievedOrders { get; private set; }

        public List<Event> AllEvents { get; private set; }

        public List<Service> AllServices { get; private set; }

        // TODO: check if these are only ELR or also Shared Sources
        public List<Service> AllEventLevelReceptions { get; private set; }

        public IWorkbook Workbook { get; private set; }

        public void GenerateExcelFile()
        {
            try
            {
                DateTime now = DateTime.Now;
                var newBackupRoot = Path.Combine(DirectoryLocations.ReportRootLocation, String.Format(CultureInfo.InvariantCulture, @"InvoiceReport_{0}_{1}", now.ToString("yyyyMMdd_HHmm"), requestedCompany));
                string backupFileSuffix = String.Format(CultureInfo.InvariantCulture, @"InvoiceReport_{0}_{1}.xlsx", now.ToString("yyyyMMdd_HHmm"), requestedCompany);

                DirectoryInfo newRootDir = new DirectoryInfo(newBackupRoot);
                if (!newRootDir.Exists)
                {
                    newRootDir.Create();

                    eventReportAttachmentRootDir = newRootDir.CreateSubdirectory(@"Event_Attachments");
                    orderReportAttachmentsRootDir = newRootDir.CreateSubdirectory(@"Order_Attachments");
                }

                Workbook = CreateExcel();

                var filePath = Path.Combine(newBackupRoot, backupFileSuffix);
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    Workbook.Write(fs);
                }

                Workbook.Close();
            }
            catch (Exception e)
            {
                helpers.Engine.ExitFail("Unexpected exception occurred during generating excel file: " + e);
            }
        }

        private void Initialize()
        {
            AllEvents = helpers.EventManager.GetAllEventsBasedOnCompany(requestedStartTime, requestedEndTime, requestedCompany).ToList();

            AllRetrievedOrders = new List<Order>();
            foreach (var @event in AllEvents)
            {
                if (@event == null) continue;

                AllRetrievedOrders.AddRange(helpers.EventManager.GetOrdersInEvent(@event.Id));
            }

            AllEventLevelReceptions = new List<Service>();
            //foreach (var order in AllRetrievedOrders)
            //{
            //    var eventLevelReceptionSource = order.Sources.FirstOrDefault(s => s != null && s.BackupType == BackupType.None && s.IsEventLevelReception);
            //    if (eventLevelReceptionSource != null) AllEventLevelReceptions.Add(eventLevelReceptionSource);
            //}

            AllServices = AllRetrievedOrders.SelectMany(x => x.AllServices).OrderBy(x => x.Start).ToList();
        }

        private IWorkbook CreateExcel()
        {
            IWorkbook workBook = new XSSFWorkbook();

            if (AllEvents != null && AllEvents.Any())
            {
                var eventSheetHelper = new EventSheetHelper(helpers.Engine, workBook, AllEvents, eventColumnModel, eventReportAttachmentRootDir, allCompanies, AllEventLevelReceptions);

                eventSheetHelper.CreateSheet(sheetNameForEvents);
            }

            if (AllRetrievedOrders != null && AllRetrievedOrders.Any())
            {
                OrderSheetHelper orderSheetHelper = new OrderSheetHelper(helpers.Engine, workBook, AllRetrievedOrders, orderColumnModel, orderReportAttachmentsRootDir);
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
    }
}
