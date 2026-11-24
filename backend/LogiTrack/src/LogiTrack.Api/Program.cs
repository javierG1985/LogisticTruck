using LogiTrack.Api;
using LogiTrack.Api.Bootstrap;
using LogiTrack.Application;
using LogiTrack.Infrastructure;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// preserveStaticLogger: permite que el host se construya más de una vez en el mismo proceso (pruebas de integración).
builder.Host.UseSerilog((context, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter()), preserveStaticLogger: true);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration, builder.Environment);

var app = builder.Build();

// dotnet run -- --bootstrap-admin: crea el primer administrador y termina sin levantar el servidor.
if (args.Contains("--bootstrap-admin"))
{
    await AdminBootstrapper.RunAsync(app.Services, app.Configuration, CancellationToken.None);
    return;
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "LogiTrack v1"));
}
else
{
    // Solo fuera de desarrollo: HTTPS obligatorio y HSTS.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

public partial class Program { }
