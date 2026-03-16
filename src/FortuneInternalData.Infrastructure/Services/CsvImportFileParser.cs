using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FortuneInternalData.Application.Interfaces;

namespace FortuneInternalData.Infrastructure.Services;

public class CsvImportFileParser : IImportFileParser
{
    public Task<IReadOnlyList<ParsedImportRow>> ParseAsync(string storedFilePath, CancellationToken cancellationToken = default)
    {
        var rows = new List<ParsedImportRow>();

        var config = BuildConfig();

        using var reader = new StreamReader(storedFilePath);
        using var csv = new CsvReader(reader, config);

        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            var row = ReadRow(csv);
            if (row != null) rows.Add(row);
        }

        return Task.FromResult<IReadOnlyList<ParsedImportRow>>(rows);
    }

    public async IAsyncEnumerable<IReadOnlyList<ParsedImportRow>> ParseInChunksAsync(
        string storedFilePath,
        int chunkSize = 10000,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var config = BuildConfig();

        using var reader = new StreamReader(storedFilePath);
        using var csv = new CsvReader(reader, config);

        csv.Read();
        csv.ReadHeader();

        var chunk = new List<ParsedImportRow>(chunkSize);

        while (csv.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var row = ReadRow(csv);
            if (row != null)
            {
                chunk.Add(row);

                if (chunk.Count >= chunkSize)
                {
                    yield return chunk;
                    chunk = new List<ParsedImportRow>(chunkSize);
                    await Task.Yield();
                }
            }
        }

        if (chunk.Count > 0)
            yield return chunk;
    }

    private static CsvConfiguration BuildConfig() => new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        MissingFieldFound = null,
        HeaderValidated = null,
        PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant().Replace(" ", "_"),
        BadDataFound = null
    };

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

    private static string? GetField(CsvReader csv, params string[] names)
    {
        foreach (var name in names)
        {
            try { return csv.GetField(name); } catch { }
        }
        return null;
    }

    private static ParsedImportRow? ReadRow(CsvReader csv)
    {
        var phoneNumber = GetField(csv, "phone_number", "phonenumber", "phone") ?? string.Empty;

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        return new ParsedImportRow
        {
            Seq = GetField(csv, "seq", "no"),
            PhoneNumber = phoneNumber.Trim(),
            Remark = GetField(csv, "remark", "remarks"),
            WhatsappStatus = NullIfEmpty(GetField(csv, "whatsapp_status")),
            AgentName = NullIfEmpty(GetField(csv, "agent_name")),
            Reference = NullIfEmpty(GetField(csv, "reference")),
            UpdateStatus = NullIfEmpty(GetField(csv, "status")),
            Web1 = NullIfEmpty(GetField(csv, "web1")),
            Web2 = NullIfEmpty(GetField(csv, "web2")),
            Web3 = NullIfEmpty(GetField(csv, "web3")),
            Web4 = NullIfEmpty(GetField(csv, "web4")),
            Web5 = NullIfEmpty(GetField(csv, "web5")),
            Web6 = NullIfEmpty(GetField(csv, "web6")),
            Web7 = NullIfEmpty(GetField(csv, "web7")),
            Web8 = NullIfEmpty(GetField(csv, "web8")),
            Web9 = NullIfEmpty(GetField(csv, "web9")),
            Web10 = NullIfEmpty(GetField(csv, "web10")),
        };
    }
}
