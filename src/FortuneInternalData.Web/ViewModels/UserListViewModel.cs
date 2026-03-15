using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Web.ViewModels;

public class UserListViewModel
{
    public IReadOnlyList<UserListItemDto> Users { get; set; } = Array.Empty<UserListItemDto>();
}
