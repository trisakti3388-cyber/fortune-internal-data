using FortuneInternalData.Application.Interfaces;

namespace FortuneInternalData.Infrastructure.Services;

public class ImportFileParserFactory
{
    public IImportFileParser GetParser(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".csv" => new CsvImportFileParser(),
            ".xlsx" => new XlsxImportFileParser(),
            ".xls" => new XlsxImportFileParser(),
            _ => throw new InvalidOperationException($"Unsupported file format: {ext}. Only CSV and XLSX files are supported.")
        };
    }
}
