namespace LogiTrack.Application.Features.Catalogs;

// Todos los catálogos de parametrización, agrupados por nombre: { "truck_types": [{ "code": "Light", "label": "Liviano" }], ... }.
public sealed record CatalogsResponse(IReadOnlyDictionary<string, IReadOnlyList<CatalogItemResponse>> Catalogs);

public sealed record CatalogItemResponse(string Code, string Label);
