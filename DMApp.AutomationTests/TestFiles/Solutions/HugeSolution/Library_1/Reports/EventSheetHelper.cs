namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NPOI.SS.UserModel;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports.Extensions;
    using Service = Service.Service;

    public class EventSheetHelper : SheetHelper, ISheetHelper<Event>
    {
        private readonly List<Event> allEvents;

        public EventSheetHelper(IEngine engine, IWorkbook workBook, List<Event> allEvents, Dictionary<int, string> columnModel, DirectoryInfo reportAttachmentRootDir, Dictionary<string, int> allCompanies, List<Service> allEventLevelReceptions)
            : base(engine, workBook, columnModel, reportAttachmentRootDir)
        {
            this.allEvents = allEvents;
            AllCompanies = allCompanies;
            AllEventLevelReceptions = allEventLevelReceptions;
        }

        public Dictionary<string, int> AllCompanies { get; }

        public List<Service> AllEventLevelReceptions { get; }

        public void CreateRow(ISheet sheet, int rowNbr, Event currentObject)
        {
            var eventFiles = HandleAttachments(currentObject);

            IRow row = sheet.CreateRow(rowNbr);

            var eventMainProperties = currentObject.GetEventProperties(AllCompanies, AllEventLevelReceptions, eventFiles);
            foreach (var column in columnModel)
            {
                if (!eventMainProperties.TryGetValue(column.Value, out string valueToSet))
                {
                    valueToSet = String.Empty;
                }

                row.CreateCell(column.Key).SetCellValue(valueToSet);
            }
        }

        public void CreateSheet(string sheetName)
        {
            var sheet = workBook.CreateSheet(sheetName.ToUpper());

            int rowIndex = 0;
            CreateLayout(sheet, rowIndex);

            rowIndex++;
            foreach (var @event in allEvents.Where(x => x != null).OrderBy(x => x.Start))
            {
                CreateRow(sheet, rowIndex, @event);
                rowIndex++;
            }

            AutoSizeColumns(sheet);
        }

        public List<string> HandleAttachments(Event currentObject)
        {
            var fileAttachments = currentObject?.GetEventAttachments(engine, Locations.DirectoryLocations.EventsAttachmentsActualLocation);

            CreatingSubDirectory(fileAttachments, currentObject?.Id.ToString());

            return fileAttachments;
        }
    }
}
