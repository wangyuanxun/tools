AppLibrary.WriteExcel.XlsDocument excel = new AppLibrary.WriteExcel.XlsDocument();
excel.FileName = HttpUtility.UrlEncode("yqht.xls", Encoding.UTF8).ToString();
AppLibrary.WriteExcel.Worksheet sheet = excel.Workbook.Worksheets.Add("htlb");
AppLibrary.WriteExcel.Cells cells = sheet.Cells;
AppLibrary.WriteExcel.ColumnInfo colInfo = new AppLibrary.WriteExcel.ColumnInfo(excel, sheet);
colInfo.Width = 20 * 210;
colInfo.ColumnIndexStart = 0;
colInfo.ColumnIndexEnd = 3;
sheet.AddColumnInfo(colInfo);
AppLibrary.WriteExcel.XF XFstyle = excel.NewXF();
XFstyle.HorizontalAlignment = AppLibrary.WriteExcel.HorizontalAlignments.Default;
XFstyle.Font.FontName = "Arial";
XFstyle.UseMisc = true;
XFstyle.TextDirection = AppLibrary.WriteExcel.TextDirections.LeftToRight;
XFstyle.Font.Bold = true;
XFstyle.BottomLineStyle = 1;
XFstyle.LeftLineStyle = 1;
XFstyle.TopLineStyle = 1;
XFstyle.RightLineStyle = 1;
XFstyle.UseBorder = true;
cells.Add(1, 1, "姓名", XFstyle);//第一行表头
cells.Add(1, 2, "用户名", XFstyle);
cells.Add(2, 1, "111");
cells.Add(2, 2, "1111");
excel.Send();



AppLibrary.ReadExcel.Workbook workbook = AppLibrary.ReadExcel.Workbook.getWorkbook(Server.MapPath("/Template/htxxlbmb.xls"));
AppLibrary.WriteExcel.XlsDocument excel = new AppLibrary.WriteExcel.XlsDocument();
excel.FileName = HttpUtility.UrlEncode("yqht.xls", Encoding.UTF8).ToString();
AppLibrary.WriteExcel.Worksheet sheet = excel.Workbook.Worksheets.Add("htlb");
for (int i = 0; i < workbook.Sheets[0].Rows; i++)//读取模板第一个工作区域
{
    for (int o = 0; o < workbook.Sheets[0].Columns; o++)
    {
	AppLibrary.ReadExcel.Cell cell = workbook.Sheets[0].getCell(o, i);
	sheet.Cells.Add(i + 1, o + 1, cell.Value);
    }
}
excel.Send();