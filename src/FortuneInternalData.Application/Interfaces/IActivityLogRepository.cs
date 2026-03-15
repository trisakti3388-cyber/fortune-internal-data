using FortuneInternalData.Domain.Entities;

namespace FortuneInternalData.Application.Interfaces;

public interface IActivityLogRepository
{
    Task AddAsync(ActivityLog log, CancellationToken cancellationToken = default);
}
