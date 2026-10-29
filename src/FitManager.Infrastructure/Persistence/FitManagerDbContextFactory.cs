using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FitManager.Infrastructure.Persistence;

public sealed class FitManagerDbContextFactory : IDesignTimeDbContextFactory<FitManagerDbContext>
{
    public FitManagerDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__FitManager")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=FitManagerDev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true";
        var options = new DbContextOptionsBuilder<FitManagerDbContext>().UseSqlServer(connectionString).Options;
        return new FitManagerDbContext(options);
    }
}
