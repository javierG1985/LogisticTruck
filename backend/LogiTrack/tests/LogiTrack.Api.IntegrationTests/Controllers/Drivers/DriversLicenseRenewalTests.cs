using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using LogiTrack.Api.IntegrationTests.Infrastructure;

namespace LogiTrack.Api.IntegrationTests.Controllers.Drivers;

// La licencia se renueva con PUT /api/drivers/{id}/license. El PUT de contacto no la toca.
public sealed class DriversLicenseRenewalTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // Las fechas se calculan desde hoy para que la prueba no caduque.
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public async Task ValidRenewal_Returns204AndReplacesTheLicense()
    {
        var id = await CreateDriverAsync(issuedOn: Today.AddYears(-2), expiresOn: Today.AddDays(30));
        var etag = await ReadEtagAsync(id);

        var response = await RenewAsync(id, etag, issuedOn: Today.AddYears(-1), expiresOn: Today.AddYears(2));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(response.Headers.ETag);
        var body = await GetDriverAsync(id);
        Assert.Equal(Format(Today.AddYears(2)), body["licenseExpiresOn"]!.GetValue<string>());
        Assert.Equal("Valid", body["licenseStatus"]!.GetValue<string>());
    }

    [Fact]
    public async Task RenewalIssuedBeforeTheCurrentLicense_Returns409()
    {
        var id = await CreateDriverAsync(issuedOn: Today.AddYears(-2), expiresOn: Today.AddDays(30));
        var etag = await ReadEtagAsync(id);

        var response = await RenewAsync(id, etag, issuedOn: Today.AddYears(-3), expiresOn: Today.AddYears(-1));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonNode>();
        Assert.Contains("antes de la licencia actual", problem!["detail"]!.GetValue<string>(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RenewalIssuedInTheFuture_Returns409()
    {
        var id = await CreateDriverAsync(issuedOn: Today.AddYears(-2), expiresOn: Today.AddDays(30));
        var etag = await ReadEtagAsync(id);

        var response = await RenewAsync(id, etag, issuedOn: Today.AddDays(5), expiresOn: Today.AddYears(3));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonNode>();
        Assert.Contains("fecha futura", problem!["detail"]!.GetValue<string>(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RenewalWithoutIfMatch_Returns428()
    {
        var id = await CreateDriverAsync(issuedOn: Today.AddYears(-2), expiresOn: Today.AddDays(30));

        var response = await _client.PutAsJsonAsync($"/api/drivers/{id}/license", new
        {
            licenseCategory = "C2",
            licenseIssuedOn = Format(Today.AddYears(-1)),
            licenseExpiresOn = Format(Today.AddYears(2)),
        });

        Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
    }

    [Fact]
    public async Task ContactUpdate_DoesNotTouchTheLicense()
    {
        var id = await CreateDriverAsync(issuedOn: Today.AddYears(-2), expiresOn: Today.AddDays(30));
        var before = await GetDriverAsync(id);
        var etag = await ReadEtagAsync(id);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/drivers/{id}")
        {
            Content = JsonContent.Create(new
            {
                firstName = "Ana",
                lastName = "Gómez Ruiz",
                address = "Carrera 5 # 1-2",
                phone = "3109876543",
                email = "ana.nueva@example.com",
                profilePhotoUrl = (string?)null,
            }),
        };
        request.Headers.TryAddWithoutValidation("If-Match", etag);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var after = await GetDriverAsync(id);
        Assert.Equal(before["licenseCategory"]!.GetValue<string>(), after["licenseCategory"]!.GetValue<string>());
        Assert.Equal(before["licenseIssuedOn"]!.GetValue<string>(), after["licenseIssuedOn"]!.GetValue<string>());
        Assert.Equal(before["licenseExpiresOn"]!.GetValue<string>(), after["licenseExpiresOn"]!.GetValue<string>());
        Assert.Equal("3109876543", after["phone"]!.GetValue<string>());
    }

    private async Task<Guid> CreateDriverAsync(DateOnly issuedOn, DateOnly expiresOn)
    {
        var response = await _client.PostAsJsonAsync("/api/drivers", new
        {
            documentNumber = Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L).ToString(CultureInfo.InvariantCulture),
            firstName = "Ana",
            lastName = "Gómez Ruiz",
            address = "Calle 10 # 20-30",
            phone = "3001234567",
            email = "ana@example.com",
            birthDate = "1981-01-01",
            licenseCategory = "C2",
            licenseIssuedOn = Format(issuedOn),
            licenseExpiresOn = Format(expiresOn),
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<JsonNode>();
        return created!["id"]!.GetValue<Guid>();
    }

    private async Task<string> ReadEtagAsync(Guid id)
    {
        var response = await _client.GetAsync($"/api/drivers/{id}");
        return response.Headers.ETag!.Tag;
    }

    private async Task<HttpResponseMessage> RenewAsync(Guid id, string etag, DateOnly issuedOn, DateOnly expiresOn)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/drivers/{id}/license")
        {
            Content = JsonContent.Create(new
            {
                licenseCategory = "C2",
                licenseIssuedOn = Format(issuedOn),
                licenseExpiresOn = Format(expiresOn),
            }),
        };
        request.Headers.TryAddWithoutValidation("If-Match", etag);

        return await _client.SendAsync(request);
    }

    private async Task<JsonNode> GetDriverAsync(Guid id)
    {
        var response = await _client.GetAsync($"/api/drivers/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return (await response.Content.ReadFromJsonAsync<JsonNode>())!;
    }

    private static string Format(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
