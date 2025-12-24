namespace NonLiveLocalBackup_1
{
	using NPOI.SS.UserModel;

	public static class ExcelHelper
	{
		public static void SetCellStyleToBold(IWorkbook workbook, ICell cellToStyle)
		{
			var font = workbook.CreateFont();
			font.FontHeightInPoints = 14;
			font.IsBold = true;

			cellToStyle.CellStyle = workbook.CreateCellStyle();
			cellToStyle.CellStyle.SetFont(font);
		}

		public static void SetHeaderCellStyle(IWorkbook workbook, ICell cellToStyle)
		{
			var font = workbook.CreateFont();
			font.FontHeightInPoints = 14;
			font.Color = NPOI.HSSF.Util.HSSFColor.Blue.Index;

			cellToStyle.CellStyle = workbook.CreateCellStyle();
			cellToStyle.CellStyle.SetFont(font);
		}

		public static ICellStyle CreateStyle(IWorkbook workbook, short color)
		{
			var style = workbook.CreateCellStyle();
			style.FillForegroundColor = color;
			style.FillPattern = FillPattern.SolidForeground;

			return style;
		}
	}
}