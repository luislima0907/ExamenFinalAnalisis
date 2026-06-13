using System.Text.Json.Serialization;
using ExamenFinalAnalisis.Data;
using ExamenFinalAnalisis.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Puerto dinámico para Render (si no hay PORT, se usa el de launchSettings).
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// MVC (vistas existentes) + API REST.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
    {
        // Serializar los enums como texto ("Registrado") en lugar de números.
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Base de datos SQLite. La ruta se puede configurar con la variable de
// entorno SQLITE_PATH (útil en Render); por defecto usa un archivo local.
var dbPath = Environment.GetEnvironmentVariable("SQLITE_PATH") ?? "enviosgt.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<IEnvioService, EnvioService>();

// Swagger / OpenAPI para probar la API.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Envíos Rápidos GT - API REST",
        Version = "v1",
        Description = "API de gestión y rastreo de envíos. Examen Final Análisis de Sistemas I."
    });
});

var app = builder.Build();

// Crear la base de datos si no existe.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Manejo centralizado de excepciones de negocio -> respuestas JSON.
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (RecursoNoEncontradoException ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsJsonAsync(new { mensaje = ex.Message });
    }
    catch (ReglaNegocioException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { mensaje = ex.Message });
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Swagger disponible siempre (incluido en Render). Se sirve en la raíz "/"
// para que el enlace de Render abra directamente la documentación de la API.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Envíos Rápidos GT API v1");
    c.RoutePrefix = string.Empty; // Swagger UI en "/"
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
