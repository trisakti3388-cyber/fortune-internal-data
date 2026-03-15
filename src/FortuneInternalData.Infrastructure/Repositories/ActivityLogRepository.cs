using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Domain.Entities;
using FortuneInternalData.Infrastructure.Persistence;

namespace FortuneInternalData.Infrastructure.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ActivityLogRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ActivityLog log, CancellationToken cancellationToken = default)
        => await _dbContext.ActivityLogs.AddAsync(log, cancellationToken);
}
