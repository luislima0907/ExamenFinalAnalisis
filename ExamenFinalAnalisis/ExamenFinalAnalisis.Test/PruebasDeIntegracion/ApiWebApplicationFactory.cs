using ExamenFinalAnalisis.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace ExamenFinalAnalisis.Test.PruebasDeIntegracion;

/// <summary>
/// Fábrica de la aplicación para pruebas de integración que reemplaza
/// la base de datos SQLite por una base en memoria compartida entre las
/// peticiones de la misma instancia de fábrica.
/// </summary>
public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    // Una raíz por instancia de fábrica: los datos persisten entre peticiones
    // del mismo test, pero cada fábrica está aislada de las demás.
    private readonly InMemoryDatabaseRoot _root = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            // Quitar el AppDbContext (SQLite) registrado en Program.cs.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("PruebasIntegracion", _root));
        });
    }
}
