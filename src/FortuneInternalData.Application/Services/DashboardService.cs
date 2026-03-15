using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;

namespace FortuneInternalData.Application.Services;

public class DashboardService : IDashboardService
{
    public Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Replace with aggregate queries.
        return Task.FromResult(new DashboardSummaryDto());
    }
}
