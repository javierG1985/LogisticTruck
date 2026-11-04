using LogiTrack.Api.Http;
using LogiTrack.Api.Requests.Fleet.Trucks;
using LogiTrack.Application.Common.Pagination;
using LogiTrack.Application.Modules.Fleet.Trucks;
using LogiTrack.Application.Modules.Fleet.Trucks.ChangeTruckStatus;
using LogiTrack.Application.Modules.Fleet.Trucks.CreateTruck;
using LogiTrack.Application.Modules.Fleet.Trucks.DeleteTruck;
using LogiTrack.Application.Modules.Fleet.Trucks.GetTruckById;
using LogiTrack.Application.Modules.Fleet.Trucks.ListTrucks;
using LogiTrack.Application.Modules.Fleet.Trucks.UpdateTruck;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Api.Controllers.Fleet.Trucks;

[ApiController]
[Route("api/trucks")]
[Tags("Fleet")]
[Produces("application/json")]
[Consumes("application/json")]
public sealed class TrucksController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<TruckListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<TruckListItem>>> List(
        [FromQuery] ListTrucksQuery query,
        CancellationToken cancellationToken)
    {
        var trucks = await sender.Send(query, cancellationToken);
        return Ok(trucks);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TruckDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TruckDetailsResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var truck = await sender.Send(new GetTruckByIdQuery(id), cancellationToken);
        Response.Headers.ETag = ETag.FromVersion(truck.Version);
        return Ok(truck);
    }

    [HttpPost]
    [ProducesResponseType<TruckDetailsResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TruckDetailsResponse>> Create(CreateTruckCommand command, CancellationToken cancellationToken)
    {
        var truck = await sender.Send(command, cancellationToken);
        Response.Headers.ETag = ETag.FromVersion(truck.Version);
        return CreatedAtAction(nameof(GetById), new { id = truck.Id }, truck);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    public async Task<ActionResult> Update(Guid id, UpdateTruckRequest request, CancellationToken cancellationToken)
    {
        var expectedVersion = ETag.RequireIfMatch(Request);

        var command = new UpdateTruckCommand(
            id,
            expectedVersion,
            request.Brand,
            request.ModelYear,
            request.CapacityKg,
            request.VolumeM3,
            request.SoatExpiresOn,
            request.TechReviewExpiresOn);

        var version = await sender.Send(command, cancellationToken);
        Response.Headers.ETag = ETag.FromVersion(version);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    public async Task<ActionResult> ChangeStatus(Guid id, ChangeTruckStatusRequest request, CancellationToken cancellationToken)
    {
        var expectedVersion = ETag.RequireIfMatch(Request);

        var version = await sender.Send(
            new ChangeTruckStatusCommand(id, expectedVersion, request.Status),
            cancellationToken);

        Response.Headers.ETag = ETag.FromVersion(version);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var expectedVersion = ETag.RequireIfMatch(Request);
        await sender.Send(new DeleteTruckCommand(id, expectedVersion), cancellationToken);
        return NoContent();
    }
}
