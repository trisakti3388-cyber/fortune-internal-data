namespace FortuneInternalData.Application.DTOs;

public class WebSummaryDto
{
    public int TotalRecords { get; set; }
    public List<WebColumnSummaryItem> Columns { get; set; } = new();
    public List<WebDistributionItem> Distribution { get; set; } = new();
}

public class WebColumnSummaryItem
{
    public string ColumnName { get; set; } = string.Empty;
    public int YesCount { get; set; }
    public int NoCount { get; set; }
    public int NullCount { get; set; }
}

public class WebDistributionItem
{
    public int WebCount { get; set; }
    public int NumberCount { get; set; }
}
