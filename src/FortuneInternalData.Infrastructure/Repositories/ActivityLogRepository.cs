using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Domain.Entities;
using FortuneInternalData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IReadOnlyList<ActivityLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
        => await _dbContext.ActivityLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
}
