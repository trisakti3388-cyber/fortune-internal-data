using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Domain.Entities;

namespace FortuneInternalData.Application.Services;

public class PhoneNumberService : IPhoneNumberService
{
    private readonly IPhoneNumberRepository _phoneNumberRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserQueryService _userQueryService;

    public PhoneNumberService(
        IPhoneNumberRepository phoneNumberRepository,
        IActivityLogRepository activityLogRepository,
        IUnitOfWork unitOfWork,
        IUserQueryService userQueryService)
    {
        _phoneNumberRepository = phoneNumberRepository;
        _activityLogRepository = activityLogRepository;
        _unitOfWork = unitOfWork;
        _userQueryService = userQueryService;
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
        ModifiedDate = x.ModifiedDate,
        Web1 = x.Web1, Web2 = x.Web2, Web3 = x.Web3, Web4 = x.Web4, Web5 = x.Web5,
        Web6 = x.Web6, Web7 = x.Web7, Web8 = x.Web8, Web9 = x.Web9, Web10 = x.Web10,
        AssignedUserId = x.AssignedUserId
    };

    private async Task ResolveUserNamesAsync(IList<PhoneNumberListItemDto> dtos, CancellationToken cancellationToken)
    {
        var assignedIds = dtos.Where(x => x.AssignedUserId != null).Select(x => x.AssignedUserId!).Distinct().ToList();
        if (!assignedIds.Any()) return;

        var users = await _userQueryService.GetUsersAsync(cancellationToken);
        var userMap = users.ToDictionary(u => u.Id, u => u.FullName);
        foreach (var dto in dtos)
        {
            if (dto.AssignedUserId != null)
                dto.AssignedUserName = userMap.GetValueOrDefault(dto.AssignedUserId);
        }
    }

    public async Task<PagedResultDto<PhoneNumberListItemDto>> SearchAsync(string? phoneNumber, string? status, string? whatsappStatus, string? remark, string? reference, string? agentName, DateTime? dateFrom, DateTime? dateTo, string? assignedUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var items = await _phoneNumberRepository.SearchAsync(phoneNumber, status, whatsappStatus, remark, reference, agentName, dateFrom, dateTo, assignedUserId, page, pageSize, cancellationToken);
        var totalCount = await _phoneNumberRepository.CountAsync(phoneNumber, status, whatsappStatus, remark, reference, agentName, dateFrom, dateTo, assignedUserId, cancellationToken);

        var dtos = items.Select(ToDto).ToList();
        await ResolveUserNamesAsync(dtos, cancellationToken);

        return new PagedResultDto<PhoneNumberListItemDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = dtos
        };
    }

    public async Task<IReadOnlyList<PhoneNumberListItemDto>> SearchAllAsync(string? phoneNumber, string? status, string? whatsappStatus, string? remark, string? reference, string? agentName, DateTime? dateFrom, DateTime? dateTo, string? assignedUserId, CancellationToken cancellationToken = default)
    {
        var items = await _phoneNumberRepository.SearchAllAsync(phoneNumber, status, whatsappStatus, remark, reference, agentName, dateFrom, dateTo, assignedUserId, cancellationToken);
        var dtos = items.Select(ToDto).ToList();
        await ResolveUserNamesAsync(dtos, cancellationToken);
        return dtos;
    }

    public async Task<PhoneNumberListItemDto?> GetByIdAsync(ulong id, CancellationToken cancellationToken = default)
    {
        var entity = await _phoneNumberRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return null;
        var dto = ToDto(entity);
        if (dto.AssignedUserId != null)
        {
            var users = await _userQueryService.GetUsersAsync(cancellationToken);
            dto.AssignedUserName = users.FirstOrDefault(u => u.Id == dto.AssignedUserId)?.FullName;
        }
        return dto;
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
            entity.Reference,
            entity.Web1, entity.Web2, entity.Web3, entity.Web4, entity.Web5,
            entity.Web6, entity.Web7, entity.Web8, entity.Web9, entity.Web10
        });

        entity.Status = request.Status;
        entity.WhatsappStatus = request.WhatsappStatus;
        entity.Remark = request.Remark;
        entity.AgentName = request.AgentName;
        entity.Reference = request.Reference;
        entity.Web1 = request.Web1;
        entity.Web2 = request.Web2;
        entity.Web3 = request.Web3;
        entity.Web4 = request.Web4;
        entity.Web5 = request.Web5;
        entity.Web6 = request.Web6;
        entity.Web7 = request.Web7;
        entity.Web8 = request.Web8;
        entity.Web9 = request.Web9;
        entity.Web10 = request.Web10;
        entity.ModifiedDate = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _phoneNumberRepository.UpdateAsync(entity, cancellationToken);

        var newValue = System.Text.Json.JsonSerializer.Serialize(new
        {
            entity.Status,
            entity.WhatsappStatus,
            entity.Remark,
            entity.AgentName,
            entity.Reference,
            entity.Web1, entity.Web2, entity.Web3, entity.Web4, entity.Web5,
            entity.Web6, entity.Web7, entity.Web8, entity.Web9, entity.Web10
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

    public async Task BatchUpdateAsync(IEnumerable<ulong> ids, string? status, string? whatsappStatus, string? agentName, string? remark, string? reference,
        string? web1, string? web2, string? web3, string? web4, string? web5, string? web6, string? web7, string? web8, string? web9, string? web10,
        string? assignedUserId, string userId, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        if (!idList.Any()) return;

        var records = await _phoneNumberRepository.GetByIdsAsync(idList, cancellationToken);

        var oldValues = records.Select(r => new { r.Id, r.Status, r.WhatsappStatus, r.AgentName, r.Remark, r.Reference, r.AssignedUserId }).ToList();

        foreach (var record in records)
        {
            if (!string.IsNullOrEmpty(status)) record.Status = status;
            if (!string.IsNullOrEmpty(whatsappStatus)) record.WhatsappStatus = whatsappStatus;
            if (!string.IsNullOrEmpty(agentName)) record.AgentName = agentName;
            if (!string.IsNullOrEmpty(remark)) record.Remark = remark;
            if (!string.IsNullOrEmpty(reference)) record.Reference = reference;
            if (!string.IsNullOrEmpty(web1)) record.Web1 = web1;
            if (!string.IsNullOrEmpty(web2)) record.Web2 = web2;
            if (!string.IsNullOrEmpty(web3)) record.Web3 = web3;
            if (!string.IsNullOrEmpty(web4)) record.Web4 = web4;
            if (!string.IsNullOrEmpty(web5)) record.Web5 = web5;
            if (!string.IsNullOrEmpty(web6)) record.Web6 = web6;
            if (!string.IsNullOrEmpty(web7)) record.Web7 = web7;
            if (!string.IsNullOrEmpty(web8)) record.Web8 = web8;
            if (!string.IsNullOrEmpty(web9)) record.Web9 = web9;
            if (!string.IsNullOrEmpty(web10)) record.Web10 = web10;
            // __unassign__ = set to null; non-empty user ID = assign; empty = keep existing
            if (assignedUserId == "__unassign__")
                record.AssignedUserId = null;
            else if (!string.IsNullOrEmpty(assignedUserId))
                record.AssignedUserId = assignedUserId;
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
            NewValueJson = System.Text.Json.JsonSerializer.Serialize(new { ids = idList, status, whatsappStatus, agentName, remark, reference, assignedUserId, count = idList.Count }),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignToUserAsync(IEnumerable<ulong> ids, string? assignedUserId, string currentUserId, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        if (!idList.Any()) return;

        var records = await _phoneNumberRepository.GetByIdsAsync(idList, cancellationToken);

        foreach (var record in records)
        {
            record.AssignedUserId = assignedUserId;
            record.ModifiedDate = DateTime.UtcNow;
            record.UpdatedAt = DateTime.UtcNow;
            await _phoneNumberRepository.UpdateAsync(record, cancellationToken);
        }

        await _activityLogRepository.AddAsync(new ActivityLog
        {
            UserId = currentUserId,
            Action = "assign_phone_numbers",
            TargetType = "phone_numbers",
            TargetId = 0,
            NewValueJson = System.Text.Json.JsonSerializer.Serialize(new { ids = idList, assignedUserId, count = idList.Count }),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
