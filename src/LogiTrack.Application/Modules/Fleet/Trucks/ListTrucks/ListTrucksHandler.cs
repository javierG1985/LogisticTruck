using System.Linq.Expressions;
using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Pagination;
using LogiTrack.Domain.Modules.Fleet.Trucks;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Modules.Fleet.Trucks.ListTrucks;

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

        var totalCount = await trucks.CountAsync(cancellationToken);

        // EF no traduce Plate.Value en SQL, así que se proyectan las columnas y la placa se convierte en memoria.
        var rows = await ApplySort(trucks, query.Sort)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(truck => new
            {
                truck.Id,
                truck.Plate,
                truck.Brand,
                truck.ModelYear,
                truck.CapacityKg,
                truck.Status,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new TruckListItem(row.Id, row.Plate.Value, row.Brand, row.ModelYear, row.CapacityKg, row.Status))
            .ToList();

        return new PagedResult<TruckListItem>(items, query.Page, query.PageSize, totalCount);
    }

    private static IQueryable<Truck> ApplySearch(IQueryable<Truck> trucks, string search)
    {
        // Búsqueda sin distinguir mayúsculas. EF.Functions.Like es portable entre proveedores (sin ILIKE de Npgsql).
        var brandPattern = $"%{search.ToLowerInvariant()}%";

        // Una placa completa y válida se busca exacta; cualquier otro texto busca dentro de la marca.
        if (Plate.TryCreate(search, out var plate))
        {
            return trucks.Where(truck => truck.Plate == plate || EF.Functions.Like(truck.Brand.ToLowerInvariant(), brandPattern));
        }

        return trucks.Where(truck => EF.Functions.Like(truck.Brand.ToLowerInvariant(), brandPattern));
    }

    private static IOrderedQueryable<Truck> ApplySort(IQueryable<Truck> trucks, string? sort)
    {
        IOrderedQueryable<Truck>? ordered = null;

        foreach (var token in (sort ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var descending = token.StartsWith('-');
            var field = token.TrimStart('-');

            ordered = field switch
            {
                "plate" => Order(trucks, ordered, truck => truck.Plate, descending),
                "brand" => Order(trucks, ordered, truck => truck.Brand, descending),
                "modelYear" => Order(trucks, ordered, truck => truck.ModelYear, descending),
                "capacityKg" => Order(trucks, ordered, truck => truck.CapacityKg, descending),
                "status" => Order(trucks, ordered, truck => truck.Status, descending),
                "createdAt" => Order(trucks, ordered, truck => truck.CreatedAt, descending),
                _ => ordered,
            };
        }

        ordered ??= trucks.OrderBy(truck => truck.Plate);

        // Desempate por Id para que la paginación sea estable entre páginas.
        return ordered.ThenBy(truck => truck.Id);
    }

    private static IOrderedQueryable<Truck> Order<TKey>(
        IQueryable<Truck> trucks,
        IOrderedQueryable<Truck>? previous,
        Expression<Func<Truck, TKey>> key,
        bool descending)
    {
        if (previous is null)
        {
            return descending ? trucks.OrderByDescending(key) : trucks.OrderBy(key);
        }

        return descending ? previous.ThenByDescending(key) : previous.ThenBy(key);
    }
}
