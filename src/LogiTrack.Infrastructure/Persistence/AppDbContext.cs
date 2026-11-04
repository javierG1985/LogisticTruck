using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Domain.Aggregates.Trucks;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Truck> Trucks => Set<Truck>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
