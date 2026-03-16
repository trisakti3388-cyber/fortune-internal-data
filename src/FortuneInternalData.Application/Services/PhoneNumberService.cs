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

    private static PhoneNumberListItemDto ToDto(PhoneNumberRecord x) => new()
    {
        Id = x.Id,
        Seq = x.Seq,
        PhoneNumber = x.PhoneNumber,
        Remark = x.Remark,
        Status = x.Status,
        WhatsappStatus = x.WhatsappStatus,
        AgentName = x.AgentName,
        Reference = x.Reference,
        UploadDate = x.UploadDate,
        ModifiedDate = x.ModifiedDate
    };

    public async Task<PagedResultDto<PhoneNumberListItemDto>> SearchAsync(string? phoneNumber, string? status, string? whatsappStatus, string? remark, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var items = await _phoneNumberRepository.SearchAsync(phoneNumber, status, whatsappStatus, remark, dateFrom, dateTo, page, pageSize, cancellationToken);
        var totalCount = await _phoneNumberRepository.CountAsync(phoneNumber, status, whatsappStatus, remark, dateFrom, dateTo, cancellationToken);

        return new PagedResultDto<PhoneNumberListItemDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items.Select(ToDto).ToList()
        };
    }

    public async Task<IReadOnlyList<PhoneNumberListItemDto>> SearchAllAsync(string? phoneNumber, string? status, string? whatsappStatus, string? remark, DateTime? dateFrom, DateTime? dateTo, CancellationToken cancellationToken = default)
    {
        var items = await _phoneNumberRepository.SearchAllAsync(phoneNumber, status, whatsappStatus, remark, dateFrom, dateTo, cancellationToken);
        return items.Select(ToDto).ToList();
    }

    public async Task<PhoneNumberListItemDto?> GetByIdAsync(ulong id, CancellationToken cancellationToken = default)
    {
        var entity = await _phoneNumberRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return null;
        return ToDto(entity);
    }

    public async Task UpdateAsync(PhoneNumberUpdateDto request, string userId, CancellationToken cancellationToken = default)
    {
        var entity = await _phoneNumberRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException("Phone number record not found.");

        var oldValue = System.Text.Json.JsonSerializer.Serialize(new
        {
            entity.Status,
            entity.WhatsappStatus,
            entity.Remark,
            entity.AgentName,
            entity.Reference
        });

        entity.Status = request.Status;
        entity.WhatsappStatus = request.WhatsappStatus;
        entity.Remark = request.Remark;
        entity.AgentName = request.AgentName;
        entity.Reference = request.Reference;
        entity.ModifiedDate = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _phoneNumberRepository.UpdateAsync(entity, cancellationToken);

        var newValue = System.Text.Json.JsonSerializer.Serialize(new
        {
            entity.Status,
            entity.WhatsappStatus,
            entity.Remark,
            entity.AgentName,
            entity.Reference
        });

        await _activityLogRepository.AddAsync(new ActivityLog
        {
            UserId = userId,
            Action = "update_phone_data",
            TargetType = "phone_numbers",
            TargetId = entity.Id,
            OldValueJson = oldValue,
            NewValueJson = newValue,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task BatchDeleteAsync(IEnumerable<ulong> ids, string userId, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        if (!idList.Any()) return;

        await _phoneNumberRepository.DeleteRangeAsync(idList, cancellationToken);

        await _activityLogRepository.AddAsync(new ActivityLog
        {
            UserId = userId,
            Action = "batch_delete_phone_data",
            TargetType = "phone_numbers",
            TargetId = 0,
            NewValueJson = System.Text.Json.JsonSerializer.Serialize(new { ids = idList, count = idList.Count }),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task BatchUpdateAsync(IEnumerable<ulong> ids, string? status, string? whatsappStatus, string? agentName, string userId, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        if (!idList.Any()) return;

        var records = await _phoneNumberRepository.GetByIdsAsync(idList, cancellationToken);

        var oldValues = records.Select(r => new { r.Id, r.Status, r.WhatsappStatus, r.AgentName }).ToList();

        foreach (var record in records)
        {
            if (!string.IsNullOrEmpty(status))
                record.Status = status;
            if (!string.IsNullOrEmpty(whatsappStatus))
                record.WhatsappStatus = whatsappStatus;
            if (!string.IsNullOrEmpty(agentName))
                record.AgentName = agentName;
            record.ModifiedDate = DateTime.UtcNow;
            record.UpdatedAt = DateTime.UtcNow;
            await _phoneNumberRepository.UpdateAsync(record, cancellationToken);
        }

        await _activityLogRepository.AddAsync(new ActivityLog
        {
            UserId = userId,
            Action = "batch_update_phone_data",
            TargetType = "phone_numbers",
            TargetId = 0,
            OldValueJson = System.Text.Json.JsonSerializer.Serialize(oldValues),
            NewValueJson = System.Text.Json.JsonSerializer.Serialize(new { ids = idList, status, whatsappStatus, agentName, count = idList.Count }),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
