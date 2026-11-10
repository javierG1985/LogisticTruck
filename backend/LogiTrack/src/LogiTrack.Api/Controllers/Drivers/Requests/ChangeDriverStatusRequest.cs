using LogiTrack.Domain.Aggregates.Drivers;

namespace LogiTrack.Api.Controllers.Drivers.Requests;

// Cuerpo del PATCH de estado. El id viene en la ruta y la versión en If-Match.
public sealed record ChangeDriverStatusRequest(DriverStatus Status);
