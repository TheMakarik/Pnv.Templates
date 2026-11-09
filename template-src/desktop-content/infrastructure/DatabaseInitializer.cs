namespace Pnv.Desktop.Infrastructure;

public sealed class DatabaseInitializer(AppDbContext dbContext)
{
    public void Initialize() => dbContext.Database.EnsureCreated();
}
