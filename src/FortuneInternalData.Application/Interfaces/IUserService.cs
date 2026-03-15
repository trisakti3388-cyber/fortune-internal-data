using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Application.Interfaces;

public interface IUserService
{
    Task CreateAsync(CreateUserDto request, ulong createdByUserId, CancellationToken cancellationToken = default);
}
