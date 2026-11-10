using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Exceptions;
using LogiTrack.Application.Common.Search;
using LogiTrack.Domain.Aggregates.Assignments;
using LogiTrack.Domain.Aggregates.Drivers;
using LogiTrack.Domain.Aggregates.Trucks;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LogiTrack.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    private const string UniqueViolationMessage =
        "Otro cambio ocupó el mismo dato único al mismo tiempo. Recarga e intenta de nuevo.";

    public DbSet<Truck> Trucks => Set<Truck>();

    public DbSet<Driver> Drivers => Set<Driver>();

    public DbSet<TruckAssignment> TruckAssignments => Set<TruckAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");

        // TextSearch.ToLowerText se traduce a lower() de PostgreSQL, así Application no necesita Npgsql.
        modelBuilder
            .HasDbFunction(typeof(TextSearch).GetMethod(nameof(TextSearch.ToLowerText), [typeof(string)])!)
            .HasName("lower");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    // Dos peticiones pueden pasar la comprobación previa a la vez y chocar en el índice único.
    // PostgreSQL responde 23505 (unique_violation): se traduce a 409 para que Api no dependa de Npgsql.
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflictException(UniqueViolationMessage);
        }
    }
}
