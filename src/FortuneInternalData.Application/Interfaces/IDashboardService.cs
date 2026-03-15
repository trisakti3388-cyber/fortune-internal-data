using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
