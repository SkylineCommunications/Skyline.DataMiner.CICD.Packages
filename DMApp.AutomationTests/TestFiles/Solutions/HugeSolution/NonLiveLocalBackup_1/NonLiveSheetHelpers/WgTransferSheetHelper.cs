namespace NonLiveLocalBackup_1
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using NPOI.SS.UserModel;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Transfer;
	using NonLiveLocalBackup_1.NonLiveSheetHelpers;

	public class WgTransferSheetHelper : NonLiveSheetHelper
	{
		public WgTransferSheetHelper(Helpers helpers, List<Transfer> wgTransferOrders, Dictionary<int, string> columnModel, DirectoryInfo backupAttachmentRootDir)
			: base(helpers, wgTransferOrders, columnModel, backupAttachmentRootDir)
		{
		}

		protected override Dictionary<string, string> GetOrderRowCellValues(ISheet sheet, NonLiveOrder order)
		{
			var transferOrder = (Transfer)order;
			return transferOrder.GetWgTransferProperties(helpers);
		}
	}
}