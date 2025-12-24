namespace NonLiveLocalBackup_1.NonLiveSheetHelpers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using NPOI.SS.UserModel;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class IplayFolderCreationSheetHelper : NonLiveSheetHelper
	{
		public IplayFolderCreationSheetHelper(Helpers helpers, List<FolderCreation> iplayFolderCreationOrders, Dictionary<int, string> columnModel, DirectoryInfo backupAttachmentRootDir)
			: base(helpers, iplayFolderCreationOrders, columnModel, backupAttachmentRootDir)
		{
		}

		protected override Dictionary<string, string> GetOrderRowCellValues(ISheet sheet, NonLiveOrder order)
		{
			var folderCreationOrder = (FolderCreation)order;
			return folderCreationOrder.GetIplayFolderCreationProperties();
		}
	}
}