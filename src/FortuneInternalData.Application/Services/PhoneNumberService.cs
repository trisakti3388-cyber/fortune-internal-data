using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Domain.Entities;

namespace FortuneInternalData.Application.Services;

public class PhoneNumberService : IPhoneNumberService
{
    private readonly IPhoneNumberRepository _phoneNumberRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PhoneNumberService(
        IPhoneNumberRepository phoneNumberRepository,
        IActivityLogRepository activityLogRepository,
        IUnitOfWork unitOfWork)
    {
        _phoneNumberRepository = phoneNumberRepository;
        _activityLogRepository = activityLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResultDto<PhoneNumberListItemDto>> SearchAsync(string? phoneNumber, string? status, string? whatsappStatus, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var items = await _phoneNumberRepository.SearchAsync(phoneNumber, status, whatsappStatus, page, pageSize, cancellationToken);
        var totalCount = await _phoneNumberRepository.CountAsync(phoneNumber, status, whatsappStatus, cancellationToken);

        return new PagedResultDto<PhoneNumberListItemDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items.Select(x => new PhoneNumberListItemDto
            {
                Id = x.Id,
                Seq = x.Seq,
                PhoneNumber = x.PhoneNumber,
                Remark = x.Remark,
                Status = x.Status,
                WhatsappStatus = x.WhatsappStatus,
                UploadDate = x.UploadDate,
                ModifiedDate = x.ModifiedDate
            }).ToList()
        };
    }

    public async Task UpdateAsync(PhoneNumberUpdateDto request, ulong userId, CancellationToken cancellationToken = default)
    {
        var entity = await _phoneNumberRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException("Phone number record not found.");

        var oldValue = System.Text.Json.JsonSerializer.Serialize(new
        {
            entity.Status,
            entity.WhatsappStatus,
            entity.Remark
        });

        entity.Status = request.Status;
        entity.WhatsappStatus = request.WhatsappStatus;
        entity.Remark = request.Remark;
        entity.ModifiedDate = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _phoneNumberRepository.UpdateAsync(entity, cancellationToken);

        var newValue = System.Text.Json.JsonSerializer.Serialize(new
        {
            entity.Status,
            entity.WhatsappStatus,
            entity.Remark
        });

        await _activityLogRepository.AddAsync(new ActivityLog
        {
            UserId = userId,
            Action = "update_phone_data",
            TargetType = "phone_numbers",
            TargetId = entity.Id,
            OldValueJson = oldValue,
            NewValueJson = newValue,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
