namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using NPOI.SS.UserModel;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports.Extensions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports.Locations;

	public class OrderSheetHelper : SheetHelper, ISheetHelper<Order>
    {
        private readonly List<Order> allOrders;

        public OrderSheetHelper(IEngine engine, IWorkbook workBook, List<Order> allOrders, Dictionary<int, string> columnModel, DirectoryInfo reportAttachmentRootDir)
            : base(engine, workBook, columnModel, reportAttachmentRootDir)
        {
            this.allOrders = allOrders;
        }

        public void CreateRow(ISheet sheet, int rowNbr, Order currentObject)
        {
            var orderFiles = HandleAttachments(currentObject);

            IRow row = sheet.CreateRow(rowNbr);

            var orderMainProperties = currentObject.GetOrderProperties(orderFiles);
            foreach (var column in columnModel)
            {
                if (!orderMainProperties.TryGetValue(column.Value, out string valueToSet))
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
            foreach (var order in allOrders.Where(x => x != null).OrderBy(x => x.Start))
            {
                CreateRow(sheet, rowIndex, order);
                rowIndex++;
            }

            AutoSizeColumns(sheet);
        }

        public List<string> HandleAttachments(Order currentObject)
        {
            var fileAttachments = currentObject?.GetAttachments(engine, DirectoryLocations.OrderAttachmentActualLocation);

            CreatingSubDirectory(fileAttachments, currentObject?.Id.ToString());

            return fileAttachments;
        }
    }
}
