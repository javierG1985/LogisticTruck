using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using LogiTrack.Api.IntegrationTests.Infrastructure;

namespace LogiTrack.Api.IntegrationTests.Controllers.Drivers;

public sealed class DriversLicenseStatusTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // Las fechas se calculan a partir de hoy para que la prueba no caduque.
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public async Task LicenseWithLessThanSixtyDaysLeft_IsExpiringSoon()
    {
        var id = await CreateDriverAsync("C2", Today.AddDays(30));

        var body = await GetDriverAsync(id);

        Assert.Equal("ExpiringSoon", body["licenseStatus"]!.GetValue<string>());
    }

    [Fact]
    public async Task LicenseWithOverAYearLeft_IsValid()
    {
        var id = await CreateDriverAsync("C3", Today.AddYears(1));

        var body = await GetDriverAsync(id);

        Assert.Equal("Valid", body["licenseStatus"]!.GetValue<string>());
    }

    [Fact]
    public async Task ExpiredLicense_IsExpired()
    {
        var id = await CreateDriverAsync("C1", Today.AddDays(-1));

        var body = await GetDriverAsync(id);

        Assert.Equal("Expired", body["licenseStatus"]!.GetValue<string>());
    }

    [Fact]
    public async Task ListItems_CarryTheLicenseStatus()
    {
        var id = await CreateDriverAsync("C2", Today.AddDays(10));

        var list = await _client.GetFromJsonAsync<JsonNode>("/api/drivers?licenseCategory=C2&pageSize=100");
        var item = list!["items"]!.AsArray().FirstOrDefault(i => i!["id"]!.GetValue<string>() == id.ToString());

        Assert.NotNull(item);
        Assert.Equal("ExpiringSoon", item!["licenseStatus"]!.GetValue<string>());
    }

    private async Task<Guid> CreateDriverAsync(string category, DateOnly expiresOn)
    {
        // Licencia expedida hace dos años (conductor de 44 años): el máximo permitido es de tres años.
        var issuedOn = Today.AddYears(-2);
        var response = await _client.PostAsJsonAsync("/api/drivers", new
        {
            documentNumber = Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L).ToString(CultureInfo.InvariantCulture),
            firstName = "Ana",
            lastName = "Gómez Ruiz",
            address = "Calle 10 # 20-30",
            phone = "3001234567",
            email = "ana@example.com",
            birthDate = "1981-01-01",
            licenseCategory = category,
            licenseIssuedOn = issuedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            licenseExpiresOn = expiresOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<JsonNode>();
        return created!["id"]!.GetValue<Guid>();
    }

    private async Task<JsonNode> GetDriverAsync(Guid id)
    {
        var response = await _client.GetAsync($"/api/drivers/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return (await response.Content.ReadFromJsonAsync<JsonNode>())!;
    }
}
