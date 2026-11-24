using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using LogiTrack.Api.IntegrationTests.Infrastructure;
using LogiTrack.Domain.Aggregates.Assignments;
using LogiTrack.Domain.Aggregates.Drivers;
using LogiTrack.Domain.Aggregates.Trucks;

namespace LogiTrack.Api.IntegrationTests.Controllers.Catalogs;

public sealed class CatalogsEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    // Cada catálogo debe tener exactamente los valores de su enum: si el dominio cambia, esta prueba avisa.
    public static TheoryData<string, Type> CatalogToEnum => new()
    {
        { "truck_types", typeof(TruckType) },
        { "truck_statuses", typeof(TruckStatus) },
        { "driver_statuses", typeof(DriverStatus) },
        { "license_categories", typeof(LicenseCategory) },
        { "license_statuses", typeof(LicenseStatus) },
        { "assignment_release_reasons", typeof(AssignmentReleaseReason) },
    };

    [Fact]
    public async Task List_ReturnsCatalogsWithCodesAndLabels()
    {
        var response = await factory.CreateClient().GetAsync("/api/catalogs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        var truckTypes = body!["catalogs"]!["truck_types"]!.AsArray();
        Assert.Contains(truckTypes, item => item!["code"]!.GetValue<string>() == "Light" && item["label"]!.GetValue<string>() == "Liviano");
    }

    [Theory]
    [MemberData(nameof(CatalogToEnum))]
    public async Task Catalog_HasExactlyTheValuesOfItsEnum(string catalog, Type enumType)
    {
        var response = await factory.CreateClient().GetAsync("/api/catalogs");

        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        var codes = body!["catalogs"]![catalog]!.AsArray()
            .Select(item => item!["code"]!.GetValue<string>())
            .OrderBy(code => code, StringComparer.Ordinal)
            .ToArray();
        var expected = Enum.GetNames(enumType).OrderBy(name => name, StringComparer.Ordinal).ToArray();

        Assert.Equal(expected, codes);
    }

    [Fact]
    public async Task List_WithoutToken_Returns401()
    {
        var response = await factory.CreateAnonymousClient().GetAsync("/api/catalogs");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
