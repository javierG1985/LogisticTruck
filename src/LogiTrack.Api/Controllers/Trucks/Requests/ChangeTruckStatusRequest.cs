using LogiTrack.Domain.Aggregates.Trucks;

namespace LogiTrack.Api.Controllers.Trucks.Requests;

// Cuerpo del PATCH de estado. El id viene en la ruta y la versión en If-Match.
public sealed record ChangeTruckStatusRequest(TruckStatus Status);
