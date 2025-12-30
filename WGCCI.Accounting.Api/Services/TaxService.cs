using WGCCI.Accounting.Api.Data;

namespace WGCCI.Accounting.Api.Services;

public class TaxService
{
    private readonly AppDbContext _db;

    public TaxService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TaxCode> CreateTaxCodeAsync(
        int orgId,
        string name,
        decimal rate,
        TaxType type)
    {
        var tc = new TaxCode
        {
            OrgId = orgId,
            Name = name,
            Rate = rate,
            Type = type
        };

        _db.TaxCodes.Add(tc);
        await _db.SaveChangesAsync();

        return tc;
    }
}
