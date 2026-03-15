using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Application.Interfaces;

public interface IUserQueryService
{
    Task<IReadOnlyList<UserListItemDto>> GetUsersAsync(CancellationToken cancellationToken = default);
}
