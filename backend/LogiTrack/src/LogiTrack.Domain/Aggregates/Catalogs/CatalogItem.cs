namespace LogiTrack.Domain.Aggregates.Catalogs;

// Elemento de un catálogo de parametrización (fleet.catalog_items). Lo lee el frontend para mostrar etiquetas
// con los mismos códigos que espera la API. Los códigos deben coincidir con los enums del dominio: lo comprueba una prueba.
public sealed class CatalogItem
{
    private CatalogItem()
    {
    }

    // Nombre del catálogo, por ejemplo "truck_types" o "license_categories".
    public string CatalogCode { get; private set; } = string.Empty;

    // Código tal cual lo espera la API, por ejemplo "Light" o "C1".
    public string Code { get; private set; } = string.Empty;

    // Texto que muestra el frontend.
    public string Label { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }
}
