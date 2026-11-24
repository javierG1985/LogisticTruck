using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using LogiTrack.Api.IntegrationTests.Infrastructure;

namespace LogiTrack.Api.IntegrationTests.Controllers.Auth;

public sealed class AuthEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndBearerToken()
    {
        var response = await factory.CreateAnonymousClient().PostAsJsonAsync(
            "/api/auth/login",
            new { email = ApiFactory.AdminEmail, password = ApiFactory.SeedPassword });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        Assert.Equal(ApiFactory.AdminEmail, body!["email"]!.GetValue<string>());
        Assert.Equal("Administrator", body["role"]!.GetValue<string>());
        Assert.Equal(3600, body["expiresIn"]!.GetValue<int>());
        // El token no debe llegar al cuerpo: si llegara, un script de la página podría leerlo.
        Assert.False(body.AsObject().ContainsKey("accessToken"));

        var setCookie = response.Headers.GetValues("Set-Cookie").Single();
        Assert.StartsWith("lt_session=", setCookie, StringComparison.Ordinal);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", setCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401WithGenericMessage()
    {
        var response = await factory.CreateAnonymousClient().PostAsJsonAsync(
            "/api/auth/login",
            new { email = ApiFactory.AdminEmail, password = "contraseña-incorrecta" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonNode>();
        Assert.Equal("Correo o contraseña no válidos.", problem!["detail"]!.GetValue<string>());
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Returns401WithTheSameMessage()
    {
        var response = await factory.CreateAnonymousClient().PostAsJsonAsync(
            "/api/auth/login",
            new { email = "nadie@test.local", password = ApiFactory.SeedPassword });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonNode>();
        Assert.Equal("Correo o contraseña no válidos.", problem!["detail"]!.GetValue<string>());
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await factory.CreateAnonymousClient().GetAsync("/api/trucks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SessionCookie_AuthenticatesRequestsWithoutAuthorizationHeader()
    {
        var login = await factory.CreateAnonymousClient().PostAsJsonAsync(
            "/api/auth/login",
            new { email = ApiFactory.AdminEmail, password = ApiFactory.SeedPassword });
        var cookie = login.Headers.GetValues("Set-Cookie").Single().Split(';')[0];

        var client = factory.CreateAnonymousClient();
        client.DefaultRequestHeaders.Add("Cookie", cookie);
        var response = await client.GetAsync("/api/trucks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ExpiresTheSessionCookie()
    {
        var response = await factory.CreateAnonymousClient().PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var setCookie = response.Headers.GetValues("Set-Cookie").Single();
        Assert.StartsWith("lt_session=;", setCookie, StringComparison.Ordinal);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Health_IsPublic()
    {
        var response = await factory.CreateAnonymousClient().GetAsync("/health");

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Driver_CanRead_ButCannotWrite()
    {
        var driver = factory.CreateAnonymousClient();
        driver.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiFactory.CreateToken(ApiFactory.DriverPermissions));

        var read = await driver.GetAsync("/api/trucks");
        var write = await driver.PostAsJsonAsync("/api/trucks", new { });

        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, write.StatusCode);
    }

    [Fact]
    public async Task Operator_CannotCreateUsers()
    {
        var operatorClient = factory.CreateAnonymousClient();
        operatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiFactory.CreateToken(ApiFactory.OperatorPermissions));

        var response = await operatorClient.PostAsJsonAsync("/api/users", new { });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CreatesOperator_WhoCanThenLogIn()
    {
        var email = $"operador-{Guid.NewGuid():N}@test.local";

        var created = await factory.CreateClient().PostAsJsonAsync(
            "/api/users",
            new { email, password = "Operador-clave-12345", role = "Operator" });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var body = await created.Content.ReadFromJsonAsync<JsonNode>();
        Assert.Equal(email, body!["email"]!.GetValue<string>());
        Assert.Equal("Operator", body["role"]!.GetValue<string>());
        Assert.Null(body["passwordHash"]);

        var login = await factory.CreateAnonymousClient().PostAsJsonAsync(
            "/api/auth/login",
            new { email, password = "Operador-clave-12345" });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithShortPassword_Returns400()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/users",
            new { email = $"corta-{Guid.NewGuid():N}@test.local", password = "corta", role = "Driver" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithSevenCharacterPassword_Returns400()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/users",
            new { email = $"siete-{Guid.NewGuid():N}@test.local", password = "1234567", role = "Driver" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithEightCharacterPassword_Returns201()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/users",
            new { email = $"ocho-{Guid.NewGuid():N}@test.local", password = "12345678", role = "Driver" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithExistingEmail_Returns409()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/users",
            new { email = ApiFactory.AdminEmail, password = "Otra-clave-123456", role = "Driver" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
