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

    private static ParsedImportRow? ReadRow(CsvReader csv)
    {
        var phoneNumber = csv.GetField("phone_number")
                          ?? csv.GetField("phonenumber")
                          ?? csv.GetField("phone")
                          ?? string.Empty;

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        return new ParsedImportRow
        {
            Seq = csv.GetField("seq") ?? csv.GetField("no") ?? null,
            PhoneNumber = phoneNumber.Trim(),
            Remark = csv.GetField("remark") ?? csv.GetField("remarks") ?? null,
            WhatsappStatus = NullIfEmpty(csv.GetField("whatsapp_status")),
            AgentName = NullIfEmpty(csv.GetField("agent_name")),
            Reference = NullIfEmpty(csv.GetField("reference")),
        };
    }
}
