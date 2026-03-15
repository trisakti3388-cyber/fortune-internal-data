using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Application.Interfaces;

public interface IPhoneNumberService
{
    Task<PagedResultDto<PhoneNumberListItemDto>> SearchAsync(string? phoneNumber, string? status, string? whatsappStatus, int page, int pageSize, CancellationToken cancellationToken = default);
    Task UpdateAsync(PhoneNumberUpdateDto request, ulong userId, CancellationToken cancellationToken = default);
}
