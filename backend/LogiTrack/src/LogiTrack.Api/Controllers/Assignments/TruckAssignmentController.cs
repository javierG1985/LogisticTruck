using LogiTrack.Api.Controllers.Assignments.Requests;
using LogiTrack.Api.Http;
using LogiTrack.Application.Features.Assignments;
using LogiTrack.Application.Features.Assignments.Commands.AssignDriverToTruck;
using LogiTrack.Application.Features.Assignments.Commands.ReleaseTruckAssignment;
using LogiTrack.Application.Features.Assignments.Queries.GetCurrentTruckAssignment;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Api.Controllers.Assignments;

// Asignación actual de un camión: como mucho un conductor a la vez.
[ApiController]
[Route("api/trucks/{truckId:guid}/assignment")]
[Tags("Assignments")]
[Produces("application/json")]
[Consumes("application/json")]
public sealed class TruckAssignmentController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<AssignmentDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignmentDetailsResponse>> GetCurrent(Guid truckId, CancellationToken cancellationToken)
    {
        var assignment = await sender.Send(new GetCurrentTruckAssignmentQuery(truckId), cancellationToken);
        return Ok(assignment);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    public async Task<ActionResult> Assign(Guid truckId, AssignDriverRequest request, CancellationToken cancellationToken)
    {
        var expectedVersion = ETag.RequireIfMatch(Request);

        var version = await sender.Send(
            new AssignDriverToTruckCommand(truckId, expectedVersion, request.DriverId),
            cancellationToken);

        Response.Headers.ETag = ETag.FromVersion(version);
        return NoContent();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    public async Task<ActionResult> Release(Guid truckId, CancellationToken cancellationToken)
    {
        var expectedVersion = ETag.RequireIfMatch(Request);
        await sender.Send(new ReleaseTruckAssignmentCommand(truckId, expectedVersion), cancellationToken);
        return NoContent();
    }
}
