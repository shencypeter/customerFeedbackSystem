using ClosedXML.Excel;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
namespace CustomerFeedbackSystem.Models;

/// <summary>
/// 這一半用來寫 Dapper/ SQL 查詢邏輯
/// </summary>
public partial class DocControlContext : DbContext
{
   
    public async Task<(List<T> Items, int TotalCount)> BySqlGetPagedWithCountAsync<T>(
        string selectPart, string orderByPart, int pageNumber = 0, int pageSize = 0, object parameters = null)
    {
        if (string.IsNullOrWhiteSpace(orderByPart))
        {
            orderByPart = "ORDER BY (SELECT NULL)";
        }

        var hasPaging = pageNumber > 0 && pageSize > 0;
        var rowOffset = (pageNumber - 1) * pageSize;

        var baseCte = $"WITH BASE AS ({selectPart})";

        var pagedSql = $@"
            {baseCte}
            , RowNumbers AS (
                SELECT ROW_NUMBER() OVER ({orderByPart}) AS RowNum, * FROM BASE
            )
            SELECT * FROM RowNumbers
            ORDER BY RowNum
            {(hasPaging ? $"OFFSET {rowOffset} ROWS FETCH NEXT {pageSize} ROWS ONLY" : string.Empty)};";

        var countSql = $@"
        {baseCte}
        SELECT COUNT(1) FROM BASE;";

        var sql = $"{pagedSql}\n{countSql}";

        Console.WriteLine($"execute query sql: {Regex.Replace(sql.Replace(Environment.NewLine, " "), @"\s+", " ").Trim()}");

        var conn = this.Database.GetDbConnection();
        using var multi = await conn.QueryMultipleAsync(sql, parameters);

        var items = (await multi.ReadAsync<T>()).ToList();
        var totalCount = (await multi.ReadAsync<int>()).FirstOrDefault();

        return (items, totalCount);
    }
    
    public async Task<MemoryStream> ExportToExcelAsync(string sql, Dictionary<string, string> headers, object parameters = null, string sheetName = "Sheet1", bool autoFilters = true)
    {
        using var connection = this.Database.GetDbConnection();
        await connection.OpenAsync();

        var data = await connection.QueryAsync(sql, parameters);


        if (data.Any())
        {
            //有查詢結果才可以匯出
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            if (data is not null)
            {
                var dataTable = ToDataTable(data, headers);
                worksheet.Cell(1, 1).InsertTable(dataTable);

                // Auto-adjust column widths
                worksheet.Columns().AdjustToContents();
            }

            if (!autoFilters)
            {
                //不要產生預設的 Excel 篩選 (Default 有)
                worksheet.AutoFilter.Clear();
            }

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        throw new FileNotFoundException("No data to export");
    }

    private System.Data.DataTable ToDataTable(IEnumerable<dynamic> data, Dictionary<string, string> headers = null)
    {
        var table = new System.Data.DataTable();
        if (!data.Any()) return table;

        if (headers == null)
        {
            // Use property names from the first item in the data collection
            foreach (string property in ((IDictionary<string, object>)data.First()).Keys)
            {
                table.Columns.Add(property);
            }
        }
        else
        {
            // Use the headers dictionary
            foreach (var header in headers)
            {
                // Add the column using the custom header name (value) and map it to the original key (key)
                table.Columns.Add(header.Value);
            }
        }

        int index = 1; // 用來產生序號的變數

        // Populate the rows
        foreach (var row in data)
        {
            var dataRow = table.NewRow();
            var dictRow = (IDictionary<string, object>)row;

            if (headers == null)
            {
                // Map values directly when headers are not provided
                foreach (var kvp in dictRow)
                {
                    dataRow[kvp.Key] = kvp.Value ?? DBNull.Value;
                }
            }
            else
            {
                // Map values according to the headers dictionary
                foreach (var header in headers)
                {
                    if (header.Key == "RowNum")
                    {
                        dataRow[header.Value] = index++;
                    }

                    else if (dictRow.TryGetValue(header.Key, out object? value))
                    {
                        if (value is DateTime dt)
                        {
                            dataRow[header.Value] = dt.ToString("yyyy/M/d");
                        }
                        else
                        {
                            dataRow[header.Value] = value ?? DBNull.Value;
                        }
                    }
                    else
                    {
                        dataRow[header.Value] = DBNull.Value; // Handle missing keys
                    }
                }
            }

            table.Rows.Add(dataRow);
        }

        return table;
    }
}

