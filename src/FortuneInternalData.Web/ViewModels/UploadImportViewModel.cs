using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Web.ViewModels;

public class UploadImportViewModel
{
    public IFormFile? File { get; set; }
    public string? AssignedUserId { get; set; }
    public IReadOnlyList<UserListItemDto> Users { get; set; } = new List<UserListItemDto>();
}
