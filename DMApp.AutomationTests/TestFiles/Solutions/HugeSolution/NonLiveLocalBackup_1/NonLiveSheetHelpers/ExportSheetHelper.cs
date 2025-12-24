namespace NonLiveLocalBackup_1.NonLiveSheetHelpers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using NPOI.SS.UserModel;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class ExportSheetHelper : NonLiveSheetHelper
	{
		public ExportSheetHelper(Helpers helpers, List<Export> exportOrders , Dictionary<int, string> columnModel, DirectoryInfo backupAttachmentRootDir)
			: base(helpers, exportOrders, columnModel, backupAttachmentRootDir)
		{
		}
		
		protected override Dictionary<string, string> GetOrderRowCellValues(ISheet sheet, NonLiveOrder order)
		{
			var exportOrder = (Export)order;
			return exportOrder.GetExportProperties();
		}
	}
}