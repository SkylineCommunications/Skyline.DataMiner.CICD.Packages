namespace NonLiveLocalBackup_1.NonLiveSheetHelpers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using NPOI.SS.UserModel;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Project;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class NonIplayProjectSheetHelper : NonLiveSheetHelper
	{
		public NonIplayProjectSheetHelper(Helpers helpers, List<Project> nonIplayProjectOrders, Dictionary<int, string> columnModel, DirectoryInfo backupAttachmentRootDir)
			: base(helpers, nonIplayProjectOrders, columnModel, backupAttachmentRootDir)
		{
		}

		protected override Dictionary<string, string> GetOrderRowCellValues(ISheet sheet, NonLiveOrder order)
		{
			var projectOrder = (Project)order;
			return projectOrder.GetNonIplayProjectProperties();
		}
	}
}