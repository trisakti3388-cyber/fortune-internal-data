using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;

namespace FortuneInternalData.Application.Services;

public class UserService : IUserService
{
    public Task CreateAsync(CreateUserDto request, ulong createdByUserId, CancellationToken cancellationToken = default)
    {
        // TODO: Integrate with ASP.NET Identity user creation, role assignment,
        // password hashing, and activity logging.
        return Task.CompletedTask;
    }
}
