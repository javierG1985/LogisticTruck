using LogiTrack.Api.Controllers.Drivers.Requests;
using LogiTrack.Api.Http;
using LogiTrack.Application.Common.Pagination;
using LogiTrack.Application.Features.Drivers;
using LogiTrack.Application.Features.Drivers.Commands.ChangeDriverStatus;
using LogiTrack.Application.Features.Drivers.Commands.CreateDriver;
using LogiTrack.Application.Features.Drivers.Commands.DeleteDriver;
using LogiTrack.Application.Features.Drivers.Commands.UpdateDriver;
using LogiTrack.Application.Features.Drivers.Queries.GetDriverById;
using LogiTrack.Application.Features.Drivers.Queries.ListDrivers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Api.Controllers.Drivers;

[ApiController]
[Route("api/drivers")]
[Tags("Drivers")]
[Produces("application/json")]
[Consumes("application/json")]
public sealed class DriversController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<DriverListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<DriverListItem>>> List(
        [FromQuery] ListDriversQuery query,
        CancellationToken cancellationToken)
    {
        var drivers = await sender.Send(query, cancellationToken);
        return Ok(drivers);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<DriverDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DriverDetailsResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var driver = await sender.Send(new GetDriverByIdQuery(id), cancellationToken);
        Response.Headers.ETag = ETag.FromVersion(driver.Version);
        return Ok(driver);
    }

    [HttpPost]
    [ProducesResponseType<DriverDetailsResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DriverDetailsResponse>> Create(CreateDriverCommand command, CancellationToken cancellationToken)
    {
        var driver = await sender.Send(command, cancellationToken);
        Response.Headers.ETag = ETag.FromVersion(driver.Version);
        return CreatedAtAction(nameof(GetById), new { id = driver.Id }, driver);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    public async Task<ActionResult> Update(Guid id, UpdateDriverRequest request, CancellationToken cancellationToken)
    {
        var expectedVersion = ETag.RequireIfMatch(Request);

        var command = new UpdateDriverCommand(
            id,
            expectedVersion,
            request.FirstName,
            request.LastName,
            request.Address,
            request.Phone,
            request.Email,
            request.ProfilePhotoUrl,
            request.LicenseCategory,
            request.LicenseIssuedOn,
            request.LicenseExpiresOn);

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
    public async Task<ActionResult> ChangeStatus(Guid id, ChangeDriverStatusRequest request, CancellationToken cancellationToken)
    {
        var expectedVersion = ETag.RequireIfMatch(Request);

        var version = await sender.Send(
            new ChangeDriverStatusCommand(id, expectedVersion, request.Status),
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
        await sender.Send(new DeleteDriverCommand(id, expectedVersion), cancellationToken);
        return NoContent();
    }
}
