using FortuneInternalData.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FortuneInternalData.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> SaveAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var uploadPath = _configuration["AppSettings:UploadPath"] ?? "uploads";
        Directory.CreateDirectory(uploadPath);

        var storedFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(uploadPath, storedFileName);

        await using var output = File.Create(fullPath);
        await fileStream.CopyToAsync(output, cancellationToken);

        return fullPath;
    }
}
