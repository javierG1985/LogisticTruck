using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Pagination;
using LogiTrack.Application.Common.Search;
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
        var rows = await DriverSorts.Map.Apply(drivers, query.Sort)
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

    private static IQueryable<Driver> ApplySearch(IQueryable<Driver> drivers, string search)
    {
        // Búsqueda sin distinguir mayúsculas, mediante TextSearch (ver Common/Search/TextSearch.cs).
        var namePattern = TextSearch.ToContainsPattern(search);

        // Una cédula completa y válida se busca exacta; cualquier otro texto busca dentro del nombre o los apellidos.
        if (DocumentNumber.TryCreate(search, out var documentNumber))
        {
            return drivers.Where(driver =>
                driver.DocumentNumber == documentNumber
                || EF.Functions.Like(TextSearch.ToLowerText(driver.FirstName), namePattern, TextSearch.EscapeCharacter)
                || EF.Functions.Like(TextSearch.ToLowerText(driver.LastName), namePattern, TextSearch.EscapeCharacter));
        }

        return drivers.Where(driver =>
            EF.Functions.Like(TextSearch.ToLowerText(driver.FirstName), namePattern, TextSearch.EscapeCharacter)
            || EF.Functions.Like(TextSearch.ToLowerText(driver.LastName), namePattern, TextSearch.EscapeCharacter));
    }
}
