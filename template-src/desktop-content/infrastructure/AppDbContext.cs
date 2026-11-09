using Microsoft.EntityFrameworkCore;
using Pnv.Desktop.Models;

namespace Pnv.Desktop.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<GreetingRecord> Greetings => Set<GreetingRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GreetingRecord>(entity =>
        {
            entity.HasKey(record => record.Id);
            entity.Property(record => record.Text).IsRequired().HasMaxLength(256);
        });
    }
}
