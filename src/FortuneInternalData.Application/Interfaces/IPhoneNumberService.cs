using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Application.Interfaces;

public interface IPhoneNumberService
{
    Task<PagedResultDto<PhoneNumberListItemDto>> SearchAsync(string? phoneNumber, string? status, string? whatsappStatus, string? remark, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PhoneNumberListItemDto>> SearchAllAsync(string? phoneNumber, string? status, string? whatsappStatus, string? remark, DateTime? dateFrom, DateTime? dateTo, CancellationToken cancellationToken = default);
    Task<PhoneNumberListItemDto?> GetByIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task UpdateAsync(PhoneNumberUpdateDto request, string userId, CancellationToken cancellationToken = default);
    Task BatchDeleteAsync(IEnumerable<ulong> ids, string userId, CancellationToken cancellationToken = default);
    Task BatchUpdateAsync(IEnumerable<ulong> ids, string? status, string? whatsappStatus, string? agentName, string? remark, string? reference, string userId, CancellationToken cancellationToken = default);
}
