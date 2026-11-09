using System.Linq.Expressions;
using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Pagination;
using LogiTrack.Domain.Aggregates.Drivers;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Features.Drivers.Queries.ListDrivers;

public sealed class ListDriversHandler(IApplicationDbContext db) : IQueryHandler<ListDriversQuery, PagedResult<DriverListItem>>
{
    public async ValueTask<PagedResult<DriverListItem>> Handle(ListDriversQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Driver> drivers = db.Drivers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            drivers = ApplySearch(drivers, query.Search.Trim());
        }

        if (query.Status is { } status)
        {
            drivers = drivers.Where(driver => driver.Status == status);
        }

        if (query.LicenseCategory is { } category)
        {
            drivers = drivers.Where(driver => driver.License.Category == category);
        }

        if (query.LicenseExpiringBefore is { } expiringBefore)
        {
            drivers = drivers.Where(driver => driver.License.ExpiresOn < expiringBefore);
        }

        var totalCount = await drivers.CountAsync(cancellationToken);

        // EF no traduce DocumentNumber.Value en SQL, así que se proyectan las columnas y la cédula se convierte en memoria.
        var rows = await ApplySort(drivers, query.Sort)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(driver => new
            {
                driver.Id,
                driver.DocumentNumber,
                driver.FirstName,
                driver.LastName,
                driver.Status,
                Category = driver.License.Category,
                ExpiresOn = driver.License.ExpiresOn,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new DriverListItem(
                row.Id,
                row.DocumentNumber.Value,
                row.FirstName,
                row.LastName,
                row.Status,
                row.Category,
                row.ExpiresOn))
            .ToList();

        return new PagedResult<DriverListItem>(items, query.Page, query.PageSize, totalCount);
    }

    // EF Core traduce ToLower() a LOWER() en SQL, pero no ToLowerInvariant(). Por eso la columna usa ToLower()
    // y solo el texto buscado usa ToLowerInvariant(). Las advertencias de cultura solo aplican al lado de la columna.
#pragma warning disable CA1304, CA1311
    private static IQueryable<Driver> ApplySearch(IQueryable<Driver> drivers, string search)
    {
        // Búsqueda sin distinguir mayúsculas. EF.Functions.Like es portable entre proveedores (sin ILIKE de Npgsql).
        var namePattern = $"%{search.ToLowerInvariant()}%";

        // Una cédula completa y válida se busca exacta; cualquier otro texto busca dentro del nombre o los apellidos.
        if (DocumentNumber.TryCreate(search, out var documentNumber))
        {
            return drivers.Where(driver =>
                driver.DocumentNumber == documentNumber
                || EF.Functions.Like(driver.FirstName.ToLower(), namePattern)
                || EF.Functions.Like(driver.LastName.ToLower(), namePattern));
        }

        return drivers.Where(driver =>
            EF.Functions.Like(driver.FirstName.ToLower(), namePattern)
            || EF.Functions.Like(driver.LastName.ToLower(), namePattern));
    }

#pragma warning restore CA1304, CA1311

    private static IOrderedQueryable<Driver> ApplySort(IQueryable<Driver> drivers, string? sort)
    {
        IOrderedQueryable<Driver>? ordered = null;

        foreach (var token in (sort ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var descending = token.StartsWith('-');
            var field = token.TrimStart('-');

            ordered = field switch
            {
                "firstName" => Order(drivers, ordered, driver => driver.FirstName, descending),
                "lastName" => Order(drivers, ordered, driver => driver.LastName, descending),
                "documentNumber" => Order(drivers, ordered, driver => driver.DocumentNumber, descending),
                "status" => Order(drivers, ordered, driver => driver.Status, descending),
                "licenseExpiresOn" => Order(drivers, ordered, driver => driver.License.ExpiresOn, descending),
                "createdAt" => Order(drivers, ordered, driver => driver.CreatedAt, descending),
                _ => ordered,
            };
        }

        ordered ??= drivers.OrderBy(driver => driver.LastName).ThenBy(driver => driver.FirstName);

        // Desempate por Id para que la paginación sea estable entre páginas.
        return ordered.ThenBy(driver => driver.Id);
    }

    private static IOrderedQueryable<Driver> Order<TKey>(
        IQueryable<Driver> drivers,
        IOrderedQueryable<Driver>? previous,
        Expression<Func<Driver, TKey>> key,
        bool descending)
    {
        if (previous is null)
        {
            return descending ? drivers.OrderByDescending(key) : drivers.OrderBy(key);
        }

        return descending ? previous.ThenByDescending(key) : previous.ThenBy(key);
    }
}
