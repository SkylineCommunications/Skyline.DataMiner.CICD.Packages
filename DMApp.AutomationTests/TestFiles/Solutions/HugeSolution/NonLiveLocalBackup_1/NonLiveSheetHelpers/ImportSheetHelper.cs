namespace NonLiveLocalBackup_1.NonLiveSheetHelpers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using NPOI.SS.UserModel;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class ImportSheetHelper : NonLiveSheetHelper
	{
		public ImportSheetHelper(Helpers helpers, List<Ingest> importOrders, Dictionary<int, string> columnModel, DirectoryInfo backupAttachmentRootDir)
			: base (helpers, importOrders, columnModel, backupAttachmentRootDir)
		{
		}

		protected override Dictionary<string, string> GetOrderRowCellValues(ISheet sheet, NonLiveOrder order)
		{			
			var importOrder = (Ingest)order;
			return importOrder.GetImportProperties();		
		}
	}
}