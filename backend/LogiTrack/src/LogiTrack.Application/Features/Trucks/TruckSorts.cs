using LogiTrack.Application.Common.Sorting;
using LogiTrack.Domain.Aggregates.Trucks;

namespace LogiTrack.Application.Features.Trucks;

// Campos por los que se puede ordenar la lista de camiones. Lo usan el handler y el validador.
public static class TruckSorts
{
    public static readonly SortMap<Truck> Map = new SortMap<Truck>("plate")
        .Field("plate", truck => truck.Plate)
        .Field("brand", truck => truck.Brand)
        .Field("modelYear", truck => truck.ModelYear)
        .Field("capacityKg", truck => truck.CapacityKg)
        .Field("status", truck => truck.Status)
        .Field("createdAt", truck => truck.CreatedAt);
}
