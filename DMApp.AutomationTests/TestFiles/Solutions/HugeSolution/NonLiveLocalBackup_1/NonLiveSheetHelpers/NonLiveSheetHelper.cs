namespace NonLiveLocalBackup_1.NonLiveSheetHelpers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using NPOI.SS.UserModel;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public abstract class NonLiveSheetHelper
	{
		protected Dictionary<int, string> columnModel;
		protected DirectoryInfo backupAttachmentRootDir;
		protected IEnumerable<NonLiveOrder> allOrders;

		protected Helpers helpers;

		protected NonLiveSheetHelper(Helpers helpers, IEnumerable<NonLiveOrder> nonLiveOrders, Dictionary<int, string> columnModel, DirectoryInfo backupAttachmentRootDir)
		{
			this.helpers = helpers;
			this.allOrders = nonLiveOrders;
			this.columnModel = columnModel;
			this.backupAttachmentRootDir = backupAttachmentRootDir;
		}

		public bool CopyAttachmentsAllowed { get; protected set; } = true;

		public string SheetName { get; protected set; } 

		public void CreateSheet(IWorkbook workBook)
		{
			if (!allOrders.Any())
			{
				helpers.Log(nameof(NonLiveSheetHelper), nameof(CreateSheet), $"No orders available to create excel");
				return;
			}

			string sheetName = string.Empty;

			try
			{
				sheetName = SheetName ?? allOrders.First().OrderType.GetDescription();
				var sheet = workBook.CreateSheet(sheetName.ToUpper());

				int rowIndex = 0;

				CreateLayout(workBook, sheet, rowIndex);

				CreateRowForEachOrder(sheet, rowIndex);

				foreach (var column in columnModel)
				{
					sheet.AutoSizeColumn(column.Key);
				}
			}
			catch (Exception e)
			{
				helpers.Log(nameof(NonLiveSheetHelper), nameof(CreateSheet), $"Something went wrong while creating {sheetName} sheet: {e}");
			}
		}

		public void CopyOrderAttachments()
		{
			try
			{
				foreach (var order in allOrders)
				{
					var fileAttachments = order.GetAttachments(helpers.Engine, BackupLocations.TicketAttachmentsActualLocation);

					var backupAttachmentSubDirectory = CreateBackupSubDirectory(order, fileAttachments);
					if (backupAttachmentSubDirectory == null) continue;

					foreach (string oldFilePath in fileAttachments)
					{
						string fileName = Path.GetFileName(oldFilePath);
						string destinationPath = Path.Combine(backupAttachmentSubDirectory.FullName, fileName);
						File.Copy(oldFilePath, destinationPath, true);
					}
				}
			}
			catch (Exception e)
			{
				helpers.Log(nameof(NonLiveSheetHelper), nameof(CopyOrderAttachments), $"Something went wrong while copying order attachments: {e}");
			}
		}

		protected abstract Dictionary<string, string> GetOrderRowCellValues(ISheet sheet, NonLiveOrder order);

		protected virtual void CreateRowForEachOrder(ISheet sheet, int rowIndex)
		{
			rowIndex++;
			foreach (var order in allOrders.Where(x => x != null).OrderBy(x => x.Deadline))
			{
				var valuesToSet = GetOrderRowCellValues(sheet, order);
				CreateRow(sheet, rowIndex, valuesToSet);
				rowIndex++;
			}
		}

		protected virtual void CreateLayout(IWorkbook workBook, ISheet sheet, int rowNumber)
		{
			IRow row = sheet.CreateRow(rowNumber);

			foreach (var columnName in columnModel)
			{
				var propertyNameCell = row.CreateCell(columnName.Key);
				propertyNameCell.SetCellValue(columnName.Value);
				ExcelHelper.SetCellStyleToBold(workBook, propertyNameCell);
			}
		}

		protected void CreateRow(ISheet sheet, int rowNbr, Dictionary<string, string> propertiesToSet)
		{
			IRow row = sheet.CreateRow(rowNbr);

			foreach (var column in columnModel)
			{
				if (!propertiesToSet.TryGetValue(column.Value, out string valueToSet))
				{
					valueToSet = String.Empty;
				}

				row.CreateCell(column.Key).SetCellValue(valueToSet);
			}
		}

		private DirectoryInfo CreateBackupSubDirectory(NonLiveOrder order, List<string> fileAttachments)
		{
			DirectoryInfo backupAttachmentSubDirectory = null;
			if (backupAttachmentRootDir != null && backupAttachmentRootDir.Exists && fileAttachments != null && fileAttachments.Any())
			{
				backupAttachmentSubDirectory = backupAttachmentRootDir.CreateSubdirectory(string.Join("_", order.DataMinerId, order.TicketId));				
			}

			return backupAttachmentSubDirectory;
		}
	}
}