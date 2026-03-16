using FortuneInternalData.Domain.Entities;

namespace FortuneInternalData.Application.Interfaces;

public interface IPhoneNumberRepository
{
    Task<PhoneNumberRecord?> GetByIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task<PhoneNumberRecord?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PhoneNumberRecord>> SearchAsync(string? phoneNumber, string? status, string? whatsappStatus, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? phoneNumber, string? status, string? whatsappStatus, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<PhoneNumberRecord> records, CancellationToken cancellationToken = default);
    Task UpdateAsync(PhoneNumberRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PhoneNumberRecord>> GetByIdsAsync(IEnumerable<ulong> ids, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<ulong> ids, CancellationToken cancellationToken = default);
}
