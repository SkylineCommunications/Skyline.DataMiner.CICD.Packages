namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports
{
    using NPOI.SS.UserModel;
    using System.Collections.Generic;

    public interface ISheetHelper<T>
    {
        void CreateSheet(string sheetName);

        List<string> HandleAttachments(T currentObject);

        void CreateRow(ISheet sheet, int rowNbr, T currentObject);
    }
}
