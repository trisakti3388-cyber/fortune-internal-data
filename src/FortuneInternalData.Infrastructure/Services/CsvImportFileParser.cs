using FortuneInternalData.Application.Interfaces;

namespace FortuneInternalData.Infrastructure.Services;

public class CsvImportFileParser : IImportFileParser
{
    public Task<IReadOnlyList<ParsedImportRow>> ParseAsync(string storedFilePath, CancellationToken cancellationToken = default)
    {
        // TODO: Implement CsvHelper-based parsing.
        return Task.FromResult<IReadOnlyList<ParsedImportRow>>(new List<ParsedImportRow>());
    }
}
