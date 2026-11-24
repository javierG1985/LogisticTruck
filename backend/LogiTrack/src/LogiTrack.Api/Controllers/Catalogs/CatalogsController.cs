using LogiTrack.Application.Features.Catalogs;
using LogiTrack.Application.Features.Catalogs.Queries.ListCatalogs;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Api.Controllers.Catalogs;

// Catálogos de parametrización para el frontend. Solo lectura: cualquier usuario autenticado los puede consultar.
[ApiController]
[Route("api/catalogs")]
[Tags("Catalogs")]
[Produces("application/json")]
public sealed class CatalogsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<CatalogsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CatalogsResponse>> List(CancellationToken cancellationToken)
    {
        var catalogs = await sender.Send(new ListCatalogsQuery(), cancellationToken);
        return Ok(catalogs);
    }
}
