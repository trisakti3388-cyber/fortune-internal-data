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

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant().Replace(" ", "_"),
            BadDataFound = null
        };

        using var reader = new StreamReader(storedFilePath);
        using var csv = new CsvReader(reader, config);

        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            var phoneNumber = csv.GetField("phone_number")
                              ?? csv.GetField("phonenumber")
                              ?? csv.GetField("phone")
                              ?? string.Empty;

            var seq = csv.GetField("seq") ?? csv.GetField("no") ?? null;
            var remark = csv.GetField("remark") ?? csv.GetField("remarks") ?? null;

            rows.Add(new ParsedImportRow
            {
                Seq = seq,
                PhoneNumber = phoneNumber.Trim(),
                Remark = remark
            });
        }

        return Task.FromResult<IReadOnlyList<ParsedImportRow>>(rows);
    }
}
