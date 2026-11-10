using LogiTrack.Api;
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
    .AddPresentation();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "LogiTrack v1"));
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
