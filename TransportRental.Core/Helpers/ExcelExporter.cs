using ClosedXML.Excel;

namespace TransportRental.Core.Helpers;

public static class ExcelExporter
{
    public static byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName, Dictionary<string, Func<T, object?>> columns)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Headers
        var colIndex = 1;
        foreach (var col in columns)
        {
            worksheet.Cell(1, colIndex).Value = col.Key;
            worksheet.Cell(1, colIndex).Style.Font.Bold = true;
            worksheet.Cell(1, colIndex).Style.Fill.BackgroundColor = XLColor.LightGray;
            colIndex++;
        }

        // Data rows
        var rowIndex = 2;
        foreach (var item in data)
        {
            colIndex = 1;
            foreach (var col in columns)
            {
                var value = col.Value(item);
                if (value is decimal d)
                    worksheet.Cell(rowIndex, colIndex).Value = (double)d;
                else if (value is int i)
                    worksheet.Cell(rowIndex, colIndex).Value = i;
                else if (value is DateTime dt)
                    worksheet.Cell(rowIndex, colIndex).Value = dt;
                else
                    worksheet.Cell(rowIndex, colIndex).Value = value?.ToString() ?? "";
                colIndex++;
            }
            rowIndex++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
