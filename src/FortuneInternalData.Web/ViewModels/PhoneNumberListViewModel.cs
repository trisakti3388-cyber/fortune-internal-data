using FortuneInternalData.Application.DTOs;

namespace FortuneInternalData.Web.ViewModels;

public class PhoneNumberListViewModel
{
    public string? SearchPhoneNumber { get; set; }
    public string? Status { get; set; }
    public string? WhatsappStatus { get; set; }
    public string? SearchRemark { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public PagedResultDto<PhoneNumberListItemDto> Result { get; set; } = new();
}
