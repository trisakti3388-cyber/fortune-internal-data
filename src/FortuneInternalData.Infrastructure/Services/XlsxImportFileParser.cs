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

    private static ParsedImportRow BuildRow(IXLRow wsRow, ColumnMap cols, string phoneNumber)
    {
        return new ParsedImportRow
        {
            Seq = cols.seqCol > 0 ? wsRow.Cell(cols.seqCol).GetString().Trim() : null,
            PhoneNumber = phoneNumber,
            Remark = cols.remarkCol > 0 ? wsRow.Cell(cols.remarkCol).GetString().Trim() : null,
            WhatsappStatus = cols.whatsappStatusCol > 0 ? NullIfEmpty(wsRow.Cell(cols.whatsappStatusCol).GetString().Trim()) : null,
            AgentName = cols.agentNameCol > 0 ? NullIfEmpty(wsRow.Cell(cols.agentNameCol).GetString().Trim()) : null,
            Reference = cols.referenceCol > 0 ? NullIfEmpty(wsRow.Cell(cols.referenceCol).GetString().Trim()) : null,
            UpdateStatus = cols.statusCol > 0 ? NullIfEmpty(wsRow.Cell(cols.statusCol).GetString().Trim()) : null,
            Web1 = cols.web1Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web1Col).GetString().Trim()) : null,
            Web2 = cols.web2Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web2Col).GetString().Trim()) : null,
            Web3 = cols.web3Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web3Col).GetString().Trim()) : null,
            Web4 = cols.web4Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web4Col).GetString().Trim()) : null,
            Web5 = cols.web5Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web5Col).GetString().Trim()) : null,
            Web6 = cols.web6Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web6Col).GetString().Trim()) : null,
            Web7 = cols.web7Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web7Col).GetString().Trim()) : null,
            Web8 = cols.web8Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web8Col).GetString().Trim()) : null,
            Web9 = cols.web9Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web9Col).GetString().Trim()) : null,
            Web10 = cols.web10Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web10Col).GetString().Trim()) : null,
            Web11 = cols.web11Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web11Col).GetString().Trim()) : null,
            Web12 = cols.web12Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web12Col).GetString().Trim()) : null,
            Web13 = cols.web13Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web13Col).GetString().Trim()) : null,
            Web14 = cols.web14Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web14Col).GetString().Trim()) : null,
            Web15 = cols.web15Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web15Col).GetString().Trim()) : null,
            Web16 = cols.web16Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web16Col).GetString().Trim()) : null,
            Web17 = cols.web17Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web17Col).GetString().Trim()) : null,
            Web18 = cols.web18Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web18Col).GetString().Trim()) : null,
            Web19 = cols.web19Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web19Col).GetString().Trim()) : null,
            Web20 = cols.web20Col > 0 ? NullIfEmpty(wsRow.Cell(cols.web20Col).GetString().Trim()) : null,
        };
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

    private record ColumnMap(int phoneCol, int seqCol, int remarkCol, int whatsappStatusCol, int agentNameCol, int referenceCol, int statusCol,
        int web1Col, int web2Col, int web3Col, int web4Col, int web5Col, int web6Col, int web7Col, int web8Col, int web9Col, int web10Col,
        int web11Col, int web12Col, int web13Col, int web14Col, int web15Col, int web16Col, int web17Col, int web18Col, int web19Col, int web20Col);

    private static ColumnMap GetColumns(IXLWorksheet worksheet)
    {
        var headerRow = worksheet.Row(1);
        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var lastCell = headerRow.LastCellUsed();
        int lastColNum = lastCell?.Address.ColumnNumber ?? 0;
        for (int col = 1; col <= lastColNum; col++)
        {
            var headerVal = headerRow.Cell(col).GetString().Trim().ToLowerInvariant().Replace(" ", "_");
            if (!string.IsNullOrWhiteSpace(headerVal))
                headers[headerVal] = col;
        }

        int Get(params string[] names)
        {
            foreach (var n in names)
                if (headers.TryGetValue(n, out var v)) return v;
            return -1;
        }

        return new ColumnMap(
            phoneCol: Get("phone_number", "phonenumber", "phone"),
            seqCol: Get("seq", "no"),
            remarkCol: Get("remark", "remarks"),
            whatsappStatusCol: Get("whatsapp_status"),
            agentNameCol: Get("agent_name"),
            referenceCol: Get("reference"),
            statusCol: Get("status"),
            web1Col: Get("web1"), web2Col: Get("web2"), web3Col: Get("web3"),
            web4Col: Get("web4"), web5Col: Get("web5"), web6Col: Get("web6"),
            web7Col: Get("web7"), web8Col: Get("web8"), web9Col: Get("web9"),
            web10Col: Get("web10"),
            web11Col: Get("web11"), web12Col: Get("web12"), web13Col: Get("web13"),
            web14Col: Get("web14"), web15Col: Get("web15"), web16Col: Get("web16"),
            web17Col: Get("web17"), web18Col: Get("web18"), web19Col: Get("web19"),
            web20Col: Get("web20")
        );
    }
}
