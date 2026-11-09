namespace Pnv.Combo.Web.Infrastructure;

public sealed class DatabaseInitializer(AppDbContext dbContext)
{
    public void Initialize() => dbContext.Database.EnsureCreated();
}
