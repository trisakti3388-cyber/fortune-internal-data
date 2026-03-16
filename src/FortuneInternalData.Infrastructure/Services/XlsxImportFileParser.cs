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

        var cols = GetColumns(worksheet);

        if (cols.phoneCol == -1)
            return Task.FromResult<IReadOnlyList<ParsedImportRow>>(rows);

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
        for (int row = 2; row <= lastRow; row++)
        {
            var wsRow = worksheet.Row(row);
            var phoneNumber = wsRow.Cell(cols.phoneCol).GetString().Trim();

            if (string.IsNullOrWhiteSpace(phoneNumber))
                continue;

            rows.Add(BuildRow(wsRow, cols, phoneNumber));
        }

        return Task.FromResult<IReadOnlyList<ParsedImportRow>>(rows);
    }

    public async IAsyncEnumerable<IReadOnlyList<ParsedImportRow>> ParseInChunksAsync(
        string storedFilePath,
        int chunkSize = 10000,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook(storedFilePath);
        var worksheet = workbook.Worksheets.First();

        var cols = GetColumns(worksheet);

        if (cols.phoneCol == -1)
            yield break;

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
        var chunk = new List<ParsedImportRow>(chunkSize);

        for (int row = 2; row <= lastRow; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wsRow = worksheet.Row(row);
            var phoneNumber = wsRow.Cell(cols.phoneCol).GetString().Trim();

            if (string.IsNullOrWhiteSpace(phoneNumber))
                continue;

            chunk.Add(BuildRow(wsRow, cols, phoneNumber));

            if (chunk.Count >= chunkSize)
            {
                yield return chunk;
                chunk = new List<ParsedImportRow>(chunkSize);
                await Task.Yield();
            }
        }

        if (chunk.Count > 0)
            yield return chunk;
    }

    private static ParsedImportRow BuildRow(IXLRow wsRow, (int phoneCol, int seqCol, int remarkCol, int whatsappStatusCol, int agentNameCol, int referenceCol) cols, string phoneNumber)
    {
        return new ParsedImportRow
        {
            Seq = cols.seqCol > 0 ? wsRow.Cell(cols.seqCol).GetString().Trim() : null,
            PhoneNumber = phoneNumber,
            Remark = cols.remarkCol > 0 ? wsRow.Cell(cols.remarkCol).GetString().Trim() : null,
            WhatsappStatus = cols.whatsappStatusCol > 0 ? NullIfEmpty(wsRow.Cell(cols.whatsappStatusCol).GetString().Trim()) : null,
            AgentName = cols.agentNameCol > 0 ? NullIfEmpty(wsRow.Cell(cols.agentNameCol).GetString().Trim()) : null,
            Reference = cols.referenceCol > 0 ? NullIfEmpty(wsRow.Cell(cols.referenceCol).GetString().Trim()) : null,
        };
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

    private static (int phoneCol, int seqCol, int remarkCol, int whatsappStatusCol, int agentNameCol, int referenceCol) GetColumns(IXLWorksheet worksheet)
    {
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

        int whatsappStatusCol = headers.GetValueOrDefault("whatsapp_status", -1);
        int agentNameCol = headers.GetValueOrDefault("agent_name", -1);
        int referenceCol = headers.GetValueOrDefault("reference", -1);

        return (phoneCol, seqCol, remarkCol, whatsappStatusCol, agentNameCol, referenceCol);
    }
}
