using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Application.Interfaces;

public interface IPhoneNumberService
{
    Task<PagedResultDto<PhoneNumberListItemDto>> SearchAsync(string? phoneNumber, string? status, string? whatsappStatus, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PhoneNumberListItemDto?> GetByIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task UpdateAsync(PhoneNumberUpdateDto request, string userId, CancellationToken cancellationToken = default);
    Task BatchDeleteAsync(IEnumerable<ulong> ids, string userId, CancellationToken cancellationToken = default);
    Task BatchUpdateAsync(IEnumerable<ulong> ids, string? status, string? whatsappStatus, string userId, CancellationToken cancellationToken = default);
}
