using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;

namespace FortuneInternalData.Application.Services;

public class UserQueryService : IUserQueryService
{
    public Task<IReadOnlyList<UserListItemDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Replace with ASP.NET Identity query implementation.
        return Task.FromResult<IReadOnlyList<UserListItemDto>>(new List<UserListItemDto>());
    }
}
