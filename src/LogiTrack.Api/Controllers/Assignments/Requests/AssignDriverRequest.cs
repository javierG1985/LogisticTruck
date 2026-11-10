namespace LogiTrack.Api.Controllers.Assignments.Requests;

// Cuerpo del PUT: el conductor que se asigna al camión de la ruta. La versión del camión viene en If-Match.
public sealed record AssignDriverRequest(Guid DriverId);
