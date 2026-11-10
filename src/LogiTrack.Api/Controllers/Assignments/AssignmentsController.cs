using LogiTrack.Application.Common.Pagination;
using LogiTrack.Application.Features.Assignments;
using LogiTrack.Application.Features.Assignments.Queries.ListAssignments;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Api.Controllers.Assignments;

[ApiController]
[Route("api/assignments")]
[Tags("Assignments")]
[Produces("application/json")]
[Consumes("application/json")]
public sealed class AssignmentsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<AssignmentDetailsResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<AssignmentDetailsResponse>>> List(
        [FromQuery] ListAssignmentsQuery query,
        CancellationToken cancellationToken)
    {
        var assignments = await sender.Send(query, cancellationToken);
        return Ok(assignments);
    }
}
