using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GropMng.Data.DbContext;

/// <summary>
/// Defines the Entity Framework Core context represented by GropContextFactory.
/// Exposes mapped sets and configuration required for database interaction.
/// </summary>
public class GropContextFactory : IDesignTimeDbContextFactory<GropContext>
{
    public GropContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GropContext>();

        // Design-time fallback connection only for migration scaffolding.
        const string designTimeConnection = "Server=localhost\\SQL2019,49707;Database=GropMng_db;Integrated Security=False;Persist Security Info=False;User ID=sa;Password=al68kias;Encrypt=True;TrustServerCertificate=True;";
        optionsBuilder.UseSqlServer(designTimeConnection);

        return new GropContext(optionsBuilder.Options);
    }
}
