using Microsoft.EntityFrameworkCore;
using WGCCI.Accounting.Api.Data;

namespace WGCCI.Accounting.Api.Services;

public class FXService
{
    private readonly AppDbContext _db;

    public FXService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CurrencyRate> UpsertRateAsync(
        int orgId,
        DateOnly date,
        string from,
        string to,
        decimal rate)
    {
        var row = await _db.CurrencyRates
            .FirstOrDefaultAsync(r =>
                r.OrgId == orgId &&
                r.Date == date &&
                r.FromCurrency == from &&
                r.ToCurrency == to);

        if (row == null)
        {
            row = new CurrencyRate
            {
                OrgId = orgId,
                Date = date,
                FromCurrency = from,
                ToCurrency = to,
                Rate = rate
            };

            _db.CurrencyRates.Add(row);
        }
        else
        {
            row.Rate = rate;
        }

        await _db.SaveChangesAsync();

        return row;
    }

    public async Task<object> RevalueAsync(
        int orgId,
        DateOnly asOf,
        string revalEquityCode)
    {
        _db.RevaluationRuns.Add(new RevaluationRun
        {
            OrgId = orgId,
            RunDate = asOf,
            Note = "Stub revaluation"
        });

        await _db.SaveChangesAsync();

        return new
        {
            postedEntryId = (int?)null,
            note = "Stub - implement per-account revaluation"
        };
    }
}
