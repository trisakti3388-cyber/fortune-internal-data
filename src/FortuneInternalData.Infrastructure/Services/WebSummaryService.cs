using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FortuneInternalData.Infrastructure.Services;

public class WebSummaryService : IWebSummaryService
{
    private readonly ApplicationDbContext _dbContext;

    public WebSummaryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WebSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.PhoneNumbers
            .AsNoTracking()
            .Select(x => new
            {
                x.Web1, x.Web2, x.Web3, x.Web4, x.Web5,
                x.Web6, x.Web7, x.Web8, x.Web9, x.Web10
            })
            .ToListAsync(cancellationToken);

        int total = records.Count;

        string?[][] webValues = records.Select(r => new string?[]
            { r.Web1, r.Web2, r.Web3, r.Web4, r.Web5, r.Web6, r.Web7, r.Web8, r.Web9, r.Web10 })
            .ToArray();

        var columns = new List<WebColumnSummaryItem>();
        for (int i = 0; i < 10; i++)
        {
            int yes = 0, no = 0, nullCount = 0;
            foreach (var row in webValues)
            {
                var v = row[i];
                if (v == null) nullCount++;
                else if (v.Equals("YES", StringComparison.OrdinalIgnoreCase)) yes++;
                else no++;
            }
            columns.Add(new WebColumnSummaryItem
            {
                ColumnName = $"web{i + 1}",
                YesCount = yes,
                NoCount = no,
                NullCount = nullCount
            });
        }

        // Distribution: for each record, count how many webs have YES
        var distribution = new Dictionary<int, int>();
        foreach (var row in webValues)
        {
            int yesCount = row.Count(v => v != null && v.Equals("YES", StringComparison.OrdinalIgnoreCase));
            if (!distribution.ContainsKey(yesCount))
                distribution[yesCount] = 0;
            distribution[yesCount]++;
        }

        var distList = distribution
            .OrderBy(kv => kv.Key)
            .Select(kv => new WebDistributionItem { WebCount = kv.Key, NumberCount = kv.Value })
            .ToList();

        return new WebSummaryDto
        {
            TotalRecords = total,
            Columns = columns,
            Distribution = distList
        };
    }
}
