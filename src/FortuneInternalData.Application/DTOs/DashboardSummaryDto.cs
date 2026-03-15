namespace FortuneInternalData.Application.DTOs;

public class DashboardSummaryDto
{
    public int TotalPhoneData { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
    public int Whatsapp1DayCount { get; set; }
    public int Whatsapp3DayCount { get; set; }
    public int Whatsapp7DayCount { get; set; }
    public int WhatsappActiveCount { get; set; }
    public int WhatsappInactiveCount { get; set; }
}
