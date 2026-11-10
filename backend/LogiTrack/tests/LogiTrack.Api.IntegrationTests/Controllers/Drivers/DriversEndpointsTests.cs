using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using LogiTrack.Api.IntegrationTests.Infrastructure;

namespace LogiTrack.Api.IntegrationTests.Controllers.Drivers;

public sealed class DriversEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_ThenGetById_ReturnsTheSameDriver()
    {
        var document = NewDocument();
        var id = await CreateDriverAsync(document);

        var response = await _client.GetAsync($"/api/drivers/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Headers.ETag);
        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        Assert.Equal(document, body!["documentNumber"]!.GetValue<string>());
        Assert.Equal("Ana", body["firstName"]!.GetValue<string>());
        Assert.Equal("C2", body["licenseCategory"]!.GetValue<string>());
        Assert.Null(body["profilePhotoUrl"]?.GetValue<string?>());
    }

    [Fact]
    public async Task Create_WithProfilePhoto_ReturnsTheReference()
    {
        var created = await _client.PostAsJsonAsync("/api/drivers", NewDriverBodyWithPhoto(NewDocument(), "fotos/ana.jpg"));

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var body = await created.Content.ReadFromJsonAsync<JsonNode>();
        Assert.Equal("fotos/ana.jpg", body!["profilePhotoUrl"]!.GetValue<string>());
    }

    [Fact]
    public async Task Create_WithDuplicateDocument_ReturnsConflict()
    {
        var document = NewDocument();
        await CreateDriverAsync(document);

        var response = await _client.PostAsJsonAsync("/api/drivers", NewDriverBody(document));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidData_ReturnsBadRequestWithErrorsByField()
    {
        var response = await _client.PostAsJsonAsync("/api/drivers", NewDriverBody("ABC", email: "no-es-correo"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        Assert.NotNull(body!["errors"]!["documentNumber"]);
        Assert.NotNull(body["errors"]!["email"]);
    }

    [Fact]
    public async Task Create_WithLicenseBeyondLegalLimit_ReturnsConflictWithTheRuleMessage()
    {
        // Conductor de 62 años: la licencia no puede durar tres años.
        var response = await _client.PostAsJsonAsync(
            "/api/drivers",
            NewDriverBody(NewDocument(), birthDate: "1964-01-01"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        Assert.Contains("no puede vencer después del", body!["detail"]!.GetValue<string>());
    }

    [Fact]
    public async Task GetById_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/drivers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task List_WithPageSizeAndSort_ReturnsOrderedPages()
    {
        foreach (var lastName in new[] { "Zzlista A", "Zzlista B", "Zzlista C" })
        {
            await CreateDriverAsync(NewDocument(), lastName: lastName);
        }

        var firstPage = await _client.GetFromJsonAsync<JsonNode>("/api/drivers?search=zzlista&page=1&pageSize=2&sort=lastName");
        var secondPage = await _client.GetFromJsonAsync<JsonNode>("/api/drivers?search=zzlista&page=2&pageSize=2&sort=lastName");

        var first = firstPage!["items"]!.AsArray().Select(item => item!["lastName"]!.GetValue<string>()).ToList();
        var second = secondPage!["items"]!.AsArray().Select(item => item!["lastName"]!.GetValue<string>()).ToList();

        Assert.Equal(["Zzlista A", "Zzlista B"], first);
        Assert.Equal(["Zzlista C"], second);
        Assert.Equal(3, firstPage["totalCount"]!.GetValue<int>());
    }

    [Fact]
    public async Task List_FilteredByLicenseCategory_ReturnsOnlyThatCategory()
    {
        var lastName = $"Zzcat{Guid.NewGuid():N}"[..14];
        await CreateDriverAsync(NewDocument(), lastName: lastName, licenseCategory: "C3");

        var list = await _client.GetFromJsonAsync<JsonNode>($"/api/drivers?search={lastName}&licenseCategory=C3");
        var other = await _client.GetFromJsonAsync<JsonNode>($"/api/drivers?search={lastName}&licenseCategory=C1");

        Assert.Equal(1, list!["totalCount"]!.GetValue<int>());
        Assert.Equal(0, other!["totalCount"]!.GetValue<int>());
    }

    [Fact]
    public async Task Update_WithValidIfMatch_ReturnsNoContentAndNewEtag()
    {
        var id = await CreateDriverAsync(NewDocument());
        var etag = await ReadEtagAsync(id);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/drivers/{id}")
        {
            Content = JsonContent.Create(NewUpdateBody()),
        };
        request.Headers.TryAddWithoutValidation("If-Match", etag);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotEqual(etag, response.Headers.ETag?.Tag);
    }

    [Fact]
    public async Task ChangeStatus_WithValidIfMatch_ReturnsNoContentAndNewEtag()
    {
        var id = await CreateDriverAsync(NewDocument());
        var etag = await ReadEtagAsync(id);

        var response = await ChangeStatusAsync(id, "OnLeave", etag);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotEqual(etag, response.Headers.ETag?.Tag);
    }

    [Fact]
    public async Task Update_WithoutIfMatch_ReturnsPreconditionRequired()
    {
        var id = await CreateDriverAsync(NewDocument());

        var response = await _client.PutAsJsonAsync($"/api/drivers/{id}", NewUpdateBody());

        Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithStaleEtag_ReturnsPreconditionFailed()
    {
        var id = await CreateDriverAsync(NewDocument());
        var staleEtag = await ReadEtagAsync(id);
        await ChangeStatusAsync(id, "OnLeave", await ReadEtagAsync(id));

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/drivers/{id}") { Content = JsonContent.Create(NewUpdateBody()) };
        request.Headers.TryAddWithoutValidation("If-Match", staleEtag);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ThenGetAndList_DriverIsGone()
    {
        var id = await CreateDriverAsync(NewDocument());

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/drivers/{id}");
        request.Headers.TryAddWithoutValidation("If-Match", await ReadEtagAsync(id));
        var deleteResponse = await _client.SendAsync(request);

        var getResponse = await _client.GetAsync($"/api/drivers/{id}");
        var list = await _client.GetFromJsonAsync<JsonNode>("/api/drivers?pageSize=100");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.DoesNotContain(list!["items"]!.AsArray(), item => item!["id"]!.GetValue<string>() == id.ToString());
    }

    [Fact]
    public async Task Delete_WithoutIfMatch_ReturnsPreconditionRequired()
    {
        var id = await CreateDriverAsync(NewDocument());

        var response = await _client.DeleteAsync($"/api/drivers/{id}");

        Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
    }

    private async Task<Guid> CreateDriverAsync(string document, string lastName = "Gómez Ruiz", string licenseCategory = "C2")
    {
        var response = await _client.PostAsJsonAsync(
            "/api/drivers",
            NewDriverBody(document, lastName: lastName, licenseCategory: licenseCategory));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        return body!["id"]!.GetValue<Guid>();
    }

    private async Task<string> ReadEtagAsync(Guid id)
    {
        var response = await _client.GetAsync($"/api/drivers/{id}");
        return response.Headers.ETag!.Tag;
    }

    private async Task<HttpResponseMessage> ChangeStatusAsync(Guid id, string status, string etag)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/drivers/{id}/status")
        {
            Content = JsonContent.Create(new { status }),
        };
        request.Headers.TryAddWithoutValidation("If-Match", etag);

        return await _client.SendAsync(request);
    }

    // Cédula aleatoria de 8 dígitos: cada prueba crea conductores sin chocar con las demás.
    private static string NewDocument() => Random.Shared.Next(10_000_000, 100_000_000).ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static object NewDriverBody(
        string documentNumber,
        string email = "ana@example.com",
        string birthDate = "1981-01-01",
        string lastName = "Gómez Ruiz",
        string licenseCategory = "C2") => new
    {
        documentNumber,
        firstName = "Ana",
        lastName,
        address = "Calle 10 # 20-30",
        phone = "3001234567",
        email,
        birthDate,
        licenseCategory,
        licenseIssuedOn = "2026-01-15",
        licenseExpiresOn = "2029-01-15",
        profilePhotoUrl = (string?)null,
    };

    private static object NewDriverBodyWithPhoto(string documentNumber, string photo) => new
    {
        documentNumber,
        firstName = "Ana",
        lastName = "Gómez Ruiz",
        address = "Calle 10 # 20-30",
        phone = "3001234567",
        email = "ana@example.com",
        birthDate = "1981-01-01",
        licenseCategory = "C2",
        licenseIssuedOn = "2026-01-15",
        licenseExpiresOn = "2029-01-15",
        profilePhotoUrl = photo,
    };

    private static object NewUpdateBody() => new
    {
        firstName = "Ana María",
        lastName = "Gómez Ruiz",
        address = "Carrera 5 # 1-2",
        phone = "3109876543",
        email = "ana.nueva@example.com",
        profilePhotoUrl = (string?)null,
        licenseCategory = "C3",
        licenseIssuedOn = "2026-01-15",
        licenseExpiresOn = "2029-01-15",
    };
}
