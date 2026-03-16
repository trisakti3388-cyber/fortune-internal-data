using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Application.Interfaces;

public interface IWebSummaryService
{
    Task<WebSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
