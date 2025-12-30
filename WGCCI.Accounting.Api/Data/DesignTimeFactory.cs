using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace WGCCI.Accounting.Api.Data;

public class DesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var ob = new DbContextOptionsBuilder<AppDbContext>();
        ob.UseSqlServer("Server=(localdb)\\\\MSSQLLocalDB;Database=WGCCI_Accounting;Trusted_Connection=True;MultipleActiveResultSets=true");
        return new AppDbContext(ob.Options);
    }
}