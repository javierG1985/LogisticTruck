using Mediator;

namespace LogiTrack.Application.Features.Catalogs.Queries.ListCatalogs;

// Consulta sin parámetros: el frontend pide todos los catálogos de una vez.
public sealed record ListCatalogsQuery : IQuery<CatalogsResponse>;
