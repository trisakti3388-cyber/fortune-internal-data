using ClosedXML.Excel;
using FortuneInternalData.Application.Interfaces;

namespace FortuneInternalData.Infrastructure.Services;

public class XlsxImportFileParser : IImportFileParser
{
    public Task<IReadOnlyList<ParsedImportRow>> ParseAsync(string storedFilePath, CancellationToken cancellationToken = default)
    {
        var rows = new List<ParsedImportRow>();

        using var workbook = new XLWorkbook(storedFilePath);
        var worksheet = workbook.Worksheets.First();

        var headerRow = worksheet.Row(1);
        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int col = 1; col <= headerRow.LastCellUsed()?.Address.ColumnNumber; col++)
        {
            var headerVal = headerRow.Cell(col).GetString().Trim().ToLowerInvariant().Replace(" ", "_");
            if (!string.IsNullOrWhiteSpace(headerVal))
                headers[headerVal] = col;
        }

        int phoneCol = headers.GetValueOrDefault("phone_number",
                        headers.GetValueOrDefault("phonenumber",
                        headers.GetValueOrDefault("phone", -1)));

        int seqCol = headers.GetValueOrDefault("seq",
                     headers.GetValueOrDefault("no", -1));

        int remarkCol = headers.GetValueOrDefault("remark",
                        headers.GetValueOrDefault("remarks", -1));

        if (phoneCol == -1)
            return Task.FromResult<IReadOnlyList<ParsedImportRow>>(rows);

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
        for (int row = 2; row <= lastRow; row++)
        {
            var wsRow = worksheet.Row(row);
            var phoneNumber = wsRow.Cell(phoneCol).GetString().Trim();

            if (string.IsNullOrWhiteSpace(phoneNumber))
                continue;

            rows.Add(new ParsedImportRow
            {
                Seq = seqCol > 0 ? wsRow.Cell(seqCol).GetString().Trim() : null,
                PhoneNumber = phoneNumber,
                Remark = remarkCol > 0 ? wsRow.Cell(remarkCol).GetString().Trim() : null
            });
        }

        return Task.FromResult<IReadOnlyList<ParsedImportRow>>(rows);
    }
}
