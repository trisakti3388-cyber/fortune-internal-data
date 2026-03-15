namespace FortuneInternalData.Application.Interfaces;

public interface IImportFileParser
{
    Task<IReadOnlyList<ParsedImportRow>> ParseAsync(string storedFilePath, CancellationToken cancellationToken = default);
}

public class ParsedImportRow
{
    public string? Seq { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Remark { get; set; }
}
