using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FortuneInternalData.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _dbContext;

    public DashboardService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PhoneNumbers.AsNoTracking();

        var total = await query.CountAsync(cancellationToken);
        var active = await query.CountAsync(x => x.Status == "active", cancellationToken);
        var inactive = await query.CountAsync(x => x.Status == "inactive", cancellationToken);
        var wa1day = await query.CountAsync(x => x.WhatsappStatus == "1day", cancellationToken);
        var wa3day = await query.CountAsync(x => x.WhatsappStatus == "3day", cancellationToken);
        var wa7day = await query.CountAsync(x => x.WhatsappStatus == "7day", cancellationToken);
        var waActive = await query.CountAsync(x => x.WhatsappStatus == "active", cancellationToken);
        var waInactive = await query.CountAsync(x => x.WhatsappStatus == "inactive", cancellationToken);

        return new DashboardSummaryDto
        {
            TotalPhoneData = total,
            ActiveCount = active,
            InactiveCount = inactive,
            Whatsapp1DayCount = wa1day,
            Whatsapp3DayCount = wa3day,
            Whatsapp7DayCount = wa7day,
            WhatsappActiveCount = waActive,
            WhatsappInactiveCount = waInactive
        };
    }
}
