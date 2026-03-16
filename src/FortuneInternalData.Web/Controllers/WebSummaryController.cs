using ClosedXML.Excel;
using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FortuneInternalData.Web.Controllers;

[Authorize(Policy = PolicyNames.SuperadminOnly)]
public class WebSummaryController : Controller
{
    private readonly IWebSummaryService _webSummaryService;

    public WebSummaryController(IWebSummaryService webSummaryService)
    {
        _webSummaryService = webSummaryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var summary = await _webSummaryService.GetSummaryAsync(cancellationToken);
        return View(summary);
    }

    [HttpGet]
    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var summary = await _webSummaryService.GetSummaryAsync(cancellationToken);

        using var workbook = new XLWorkbook();

        // Sheet 1: Column Summary
        var ws1 = workbook.Worksheets.Add("Web Column Summary");
        ws1.Cell(1, 1).Value = "Column";
        ws1.Cell(1, 2).Value = "YES Count";
        ws1.Cell(1, 3).Value = "NO Count";
        ws1.Cell(1, 4).Value = "NULL Count";
        ws1.Row(1).Style.Font.Bold = true;
        ws1.Row(1).Style.Fill.BackgroundColor = XLColor.LightBlue;

        int row = 2;
        foreach (var col in summary.Columns)
        {
            ws1.Cell(row, 1).Value = col.ColumnName;
            ws1.Cell(row, 2).Value = col.YesCount;
            ws1.Cell(row, 3).Value = col.NoCount;
            ws1.Cell(row, 4).Value = col.NullCount;
            row++;
        }
        ws1.Columns().AdjustToContents();

        // Sheet 2: Distribution
        var ws2 = workbook.Worksheets.Add("Distribution");
        ws2.Cell(1, 1).Value = "Webs Active";
        ws2.Cell(1, 2).Value = "Number Count";
        ws2.Row(1).Style.Font.Bold = true;
        ws2.Row(1).Style.Fill.BackgroundColor = XLColor.LightBlue;

        row = 2;
        foreach (var dist in summary.Distribution)
        {
            ws2.Cell(row, 1).Value = dist.WebCount;
            ws2.Cell(row, 2).Value = dist.NumberCount;
            row++;
        }
        ws2.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"web_summary_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }
}
