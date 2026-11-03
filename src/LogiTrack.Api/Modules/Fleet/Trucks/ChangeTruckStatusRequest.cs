using LogiTrack.Domain.Modules.Fleet.Trucks;

namespace LogiTrack.Api.Modules.Fleet.Trucks;

// Cuerpo del PATCH de estado. El id viene en la ruta y la versión en If-Match.
public sealed record ChangeTruckStatusRequest(TruckStatus Status);
