namespace NonLiveLocalBackup_1.NonLiveSheetHelpers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using NPOI.SS.UserModel;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class IplayFolderCreationDeleteDateSheetHelper : NonLiveSheetHelper
	{
		public IplayFolderCreationDeleteDateSheetHelper(Helpers helpers, List<FolderCreation> iplayFolderCreationOrders, Dictionary<int, string> columnModel, DirectoryInfo backupAttachmentRootDir)
			: base(helpers, iplayFolderCreationOrders, columnModel, backupAttachmentRootDir)
		{
			CopyAttachmentsAllowed = false;
			SheetName = "Iplay folder deletions";
		}

		protected override void CreateRowForEachOrder(ISheet sheet, int rowIndex)
		{
			rowIndex++;

			foreach (var order in allOrders.Where(x => x != null).OrderBy(x => x.TicketId))
			{
				var iPlayFolderOrder = (FolderCreation)order;
				foreach (var episodeFolderRequestDetails in iPlayFolderOrder.NewEpisodeFolderRequestDetails)
				{
					var episodeFolderCreationProperties = iPlayFolderOrder.GetIplayFolderCreationDeletionDateProperties(episodeFolderRequestDetails);
					CreateRow(sheet, rowIndex, episodeFolderCreationProperties);

					rowIndex++;
				}

				if (iPlayFolderOrder.NewProgramFolderRequestDetails != null && iPlayFolderOrder.ContentType == EnumExtensions.GetDescriptionFromEnumValue(NewFolderContentTypes.PROGRAM))
				{
					var programFolderProperties = iPlayFolderOrder.GetIplayFolderCreationDeletionDateProperties(null, iPlayFolderOrder.NewProgramFolderRequestDetails);
					CreateRow(sheet, rowIndex, programFolderProperties);

					rowIndex++;
				}
			}
		}

		protected override Dictionary<string, string> GetOrderRowCellValues(ISheet sheet, NonLiveOrder order)
		{
			return new Dictionary<string, string>();
		}
	}
}