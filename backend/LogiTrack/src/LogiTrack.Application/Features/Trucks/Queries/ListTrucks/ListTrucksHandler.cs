using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Pagination;
using LogiTrack.Application.Common.Search;
using LogiTrack.Domain.Aggregates.Trucks;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Features.Trucks.Queries.ListTrucks;

public sealed class ListTrucksHandler(IApplicationDbContext db) : IQueryHandler<ListTrucksQuery, PagedResult<TruckListItem>>
{
    public async ValueTask<PagedResult<TruckListItem>> Handle(ListTrucksQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Truck> trucks = db.Trucks.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            trucks = ApplySearch(trucks, query.Search.Trim());
        }

        if (query.Status is { } status)
        {
            trucks = trucks.Where(truck => truck.Status == status);
        }

        if (query.TruckType is { } truckType)
        {
            trucks = trucks.Where(truck => truck.TruckType == truckType);
        }

        var totalCount = await trucks.CountAsync(cancellationToken);

        // EF no traduce Plate.Value en SQL, así que se proyectan las columnas y la placa se convierte en memoria.
        var rows = await TruckSorts.Map.Apply(trucks, query.Sort)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(truck => new
            {
                truck.Id,
                truck.Plate,
                truck.Brand,
                truck.TruckType,
                truck.ModelYear,
                truck.CapacityKg,
                truck.Status,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new TruckListItem(
                row.Id,
                row.Plate.Value,
                row.Brand,
                row.TruckType,
                row.ModelYear,
                row.CapacityKg,
                row.Status))
            .ToList();

        return new PagedResult<TruckListItem>(items, query.Page, query.PageSize, totalCount);
    }

    private static IQueryable<Truck> ApplySearch(IQueryable<Truck> trucks, string search)
    {
        // Búsqueda sin distinguir mayúsculas, mediante TextSearch (ver Common/Search/TextSearch.cs).
        var brandPattern = TextSearch.ToContainsPattern(search);

        // Una placa completa y válida se busca exacta; cualquier otro texto busca dentro de la marca.
        if (Plate.TryCreate(search, out var plate))
        {
            return trucks.Where(truck =>
                truck.Plate == plate
                || EF.Functions.Like(TextSearch.ToLowerText(truck.Brand), brandPattern, TextSearch.EscapeCharacter));
        }

        return trucks.Where(truck => EF.Functions.Like(TextSearch.ToLowerText(truck.Brand), brandPattern, TextSearch.EscapeCharacter));
    }
}
