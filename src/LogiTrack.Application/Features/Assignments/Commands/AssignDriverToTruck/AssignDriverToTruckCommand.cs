using Mediator;

namespace LogiTrack.Application.Features.Assignments.Commands.AssignDriverToTruck;

// Id del camión y del conductor, más la versión del camión que llega en If-Match.
// Devuelve la versión del camión para que el controlador la envíe en el ETag de la respuesta.
public sealed record AssignDriverToTruckCommand(Guid TruckId, uint ExpectedVersion, Guid DriverId) : ICommand<uint>;
