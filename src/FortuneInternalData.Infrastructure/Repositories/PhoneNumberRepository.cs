using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Domain.Entities;
using FortuneInternalData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FortuneInternalData.Infrastructure.Repositories;

public class PhoneNumberRepository : IPhoneNumberRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PhoneNumberRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PhoneNumberRecord?> GetByIdAsync(ulong id, CancellationToken cancellationToken = default)
        => _dbContext.PhoneNumbers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<PhoneNumberRecord?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        => _dbContext.PhoneNumbers.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);

    public Task<bool> ExistsAsync(string phoneNumber, CancellationToken cancellationToken = default)
        => _dbContext.PhoneNumbers.AnyAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);

    public async Task<IReadOnlyList<PhoneNumberRecord>> SearchAsync(string? phoneNumber, string? status, string? whatsappStatus, string? remark, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(phoneNumber, status, whatsappStatus, remark, dateFrom, dateTo);

        return await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(string? phoneNumber, string? status, string? whatsappStatus, string? remark, DateTime? dateFrom, DateTime? dateTo, CancellationToken cancellationToken = default)
    {
        return BuildFilterQuery(phoneNumber, status, whatsappStatus, remark, dateFrom, dateTo).CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PhoneNumberRecord>> SearchAllAsync(string? phoneNumber, string? status, string? whatsappStatus, string? remark, DateTime? dateFrom, DateTime? dateTo, CancellationToken cancellationToken = default)
    {
        return await BuildFilterQuery(phoneNumber, status, whatsappStatus, remark, dateFrom, dateTo)
            .OrderByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<PhoneNumberRecord> BuildFilterQuery(string? phoneNumber, string? status, string? whatsappStatus, string? remark, DateTime? dateFrom, DateTime? dateTo)
    {
        var query = _dbContext.PhoneNumbers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(phoneNumber))
            query = query.Where(x => x.PhoneNumber.Contains(phoneNumber.Trim()));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status);

        if (!string.IsNullOrWhiteSpace(whatsappStatus))
            query = query.Where(x => x.WhatsappStatus != null && x.WhatsappStatus.Contains(whatsappStatus.Trim()));

        if (!string.IsNullOrWhiteSpace(remark))
            query = query.Where(x => x.Remark != null && x.Remark.Contains(remark.Trim()));

        if (dateFrom.HasValue)
            query = query.Where(x => x.UploadDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(x => x.UploadDate <= dateTo.Value.AddDays(1).AddSeconds(-1));

        return query;
    }

    public async Task AddRangeAsync(IEnumerable<PhoneNumberRecord> records, CancellationToken cancellationToken = default)
        => await _dbContext.PhoneNumbers.AddRangeAsync(records, cancellationToken);

    public Task UpdateAsync(PhoneNumberRecord record, CancellationToken cancellationToken = default)
    {
        _dbContext.PhoneNumbers.Update(record);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<PhoneNumberRecord>> GetByIdsAsync(IEnumerable<ulong> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _dbContext.PhoneNumbers
            .Where(x => idList.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<ulong> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        await _dbContext.PhoneNumbers
            .Where(x => idList.Contains(x.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }
}
