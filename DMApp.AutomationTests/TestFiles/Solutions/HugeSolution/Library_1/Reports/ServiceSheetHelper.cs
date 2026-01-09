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
    using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

    public class ServiceSheetHelper : SheetHelper, ISheetHelper<Service>
    {
        private readonly List<Service> allServices;
        private readonly List<Order> allOrders;

        public ServiceSheetHelper(IEngine engine, IWorkbook workBook, List<Service> allServices, List<Order> allOrders, Dictionary<int, string> columnModel, DirectoryInfo reportAttachmentRootDir = null)
            : base(engine, workBook, columnModel, reportAttachmentRootDir)
        {
            this.allServices = allServices;
            this.allOrders = allOrders;
        }

        public void CreateRow(ISheet sheet, int rowNbr, Service currentObject)
        {
            IRow row = sheet.CreateRow(rowNbr);

            var linkedOrder = allOrders.FirstOrDefault(x => currentObject.OrderReferences.Contains(x.Id));
            var serviceMainProperties = currentObject.GetServiceProperties(linkedOrder);
            foreach (var column in columnModel)
            {
                if (!serviceMainProperties.TryGetValue(column.Value, out string valueToSet))
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
            foreach (var service in allServices.Where(x => x != null).OrderBy(x => x.Start))
            {
                CreateRow(sheet, rowIndex, service);
                rowIndex++;
            }

            AutoSizeColumns(sheet);
        }

        public List<string> HandleAttachments(Service currentObject)
        {
            return new List<string>();
        }
    }
}
