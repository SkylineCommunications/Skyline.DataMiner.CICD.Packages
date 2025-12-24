namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NPOI.SS.UserModel;
    using Skyline.DataMiner.Automation;

    public class SheetHelper
    {
        protected Dictionary<int, string> columnModel;
        protected DirectoryInfo attachmentRootDir;

        protected IWorkbook workBook;
        protected IEngine engine;

        protected SheetHelper(IEngine engine, IWorkbook workBook, Dictionary<int, string> columnModel, DirectoryInfo attachmentRootDir)
        {
            this.engine = engine;
            this.workBook = workBook;
            this.columnModel = columnModel;
            this.attachmentRootDir = attachmentRootDir;
        }

        protected void CreateLayout(ISheet sheet, int rowNumber)
        {
            IRow row = sheet.CreateRow(rowNumber);

            foreach (var columnName in columnModel)
            {
                var propertyNameCell = row.CreateCell(columnName.Key);
                propertyNameCell.SetCellValue(columnName.Value);
                ExcelHelper.SetCellStyleToBold(workBook, propertyNameCell);
            }
        }

        protected void CreatingSubDirectory(List<string> fileAttachments, string id)
        {
            DirectoryInfo reportAttachmentSubDir = null;
            if (attachmentRootDir != null && attachmentRootDir.Exists && fileAttachments != null && fileAttachments.Any())
            {
                reportAttachmentSubDir = attachmentRootDir.CreateSubdirectory(id);

                CopyFileAttachmentsToNewPath(fileAttachments, reportAttachmentSubDir);
            }
        }
        
        protected void CopyFileAttachmentsToNewPath(List<string> fileAttachments, DirectoryInfo backupAttachmentSubDir)
        {
            foreach (string oldFilePath in fileAttachments)
            {
                string fileName = Path.GetFileName(oldFilePath);
                string destinationPath = Path.Combine(backupAttachmentSubDir.FullName, fileName);
                File.Copy(oldFilePath, destinationPath, overwrite: true);
            }
        }

        protected void AutoSizeColumns(ISheet sheet)
        {
            foreach (var column in columnModel)
            {
                sheet.AutoSizeColumn(column.Key);
            }
        }
    }
}
