using ExamenFinalAnalisis.Data;
using ExamenFinalAnalisis.Dtos;
using ExamenFinalAnalisis.Models;
using ExamenFinalAnalisis.Services;
using Microsoft.EntityFrameworkCore;

namespace ExamenFinalAnalisis.Test.PruebasUnitarias;

public class EnvioServiceTests
{
    private static AppDbContext NuevoContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<(AppDbContext db, int remitenteCf, int destinatarioNit)> SembrarClientesAsync()
    {
        var db = NuevoContexto();
        var remitente = new Cliente { Nombre = "Juan Pérez", Nit = "CF" };
        var destinatario = new Cliente { Nombre = "Tienda S.A.", Nit = "1234567" };
        db.Clientes.AddRange(remitente, destinatario);
        await db.SaveChangesAsync();
        return (db, remitente.Id, destinatario.Id);
    }

    private static EnvioCrearDto NuevoEnvioDto(int remId, int destId, decimal peso = 3m) => new()
    {
        RemitenteId = remId,
        DestinatarioId = destId,
        DescripcionContenido = "Caja de documentos",
        PesoKg = peso,
        DepartamentoDestino = "Jalapa",
        OficinaOrigen = "Oficina Central Guatemala"
    };

    [Fact]
    public async Task CrearEnvio_GeneraCodigoYEstadoInicial()
    {
        var (db, rem, dest) = await SembrarClientesAsync();
        var service = new EnvioService(db);

        var envio = await service.CrearEnvioAsync(NuevoEnvioDto(rem, dest));

        Assert.StartsWith("ENV-", envio.CodigoRastreo);
        Assert.Matches(@"^ENV-\d{8}-\d{4}$", envio.CodigoRastreo);
        Assert.Equal("Registrado", envio.Estado);
        Assert.Single(envio.Historial); // estado inicial registrado en historial
        Assert.Equal("Oficina Central Guatemala", envio.Historial[0].Ubicacion);
    }

    [Fact]
    public async Task CrearEnvio_AplicaDescuentoPorNitDelDestinatario()
    {
        var (db, rem, dest) = await SembrarClientesAsync();
        var service = new EnvioService(db);

        var envio = await service.CrearEnvioAsync(NuevoEnvioDto(rem, dest, peso: 3m));

        Assert.Equal(45.00m, envio.TarifaBase);
        Assert.Equal(2.25m, envio.Descuento);
        Assert.Equal(42.75m, envio.TarifaFinal);
    }

    [Fact]
    public async Task CrearEnvio_CorrelativoIncrementaEnElDia()
    {
        var (db, rem, dest) = await SembrarClientesAsync();
        var service = new EnvioService(db);

        var primero = await service.CrearEnvioAsync(NuevoEnvioDto(rem, dest));
        var segundo = await service.CrearEnvioAsync(NuevoEnvioDto(rem, dest));

        Assert.EndsWith("-0001", primero.CodigoRastreo);
        Assert.EndsWith("-0002", segundo.CodigoRastreo);
    }

    [Fact]
    public async Task CrearEnvio_RemitenteInexistente_LanzaReglaNegocio()
    {
        var (db, _, dest) = await SembrarClientesAsync();
        var service = new EnvioService(db);

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => service.CrearEnvioAsync(NuevoEnvioDto(999, dest)));
    }

    [Fact]
    public async Task ActualizarEstado_TransicionValida_RegistraHistorial()
    {
        var (db, rem, dest) = await SembrarClientesAsync();
        var service = new EnvioService(db);
        var envio = await service.CrearEnvioAsync(NuevoEnvioDto(rem, dest));

        var actualizado = await service.ActualizarEstadoAsync(envio.CodigoRastreo,
            new ActualizarEstadoDto { NuevoEstado = EstadoEnvio.EnTransito, Ubicacion = "Bodega Central" });

        Assert.Equal("EnTransito", actualizado.Estado);
        Assert.Equal(2, actualizado.Historial.Count);
        Assert.Equal("Bodega Central", actualizado.Historial[^1].Ubicacion);
    }

    [Fact]
    public async Task ActualizarEstado_TransicionInvalida_LanzaReglaNegocio()
    {
        var (db, rem, dest) = await SembrarClientesAsync();
        var service = new EnvioService(db);
        var envio = await service.CrearEnvioAsync(NuevoEnvioDto(rem, dest));

        // Registrado -> Entregado no es válido.
        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            service.ActualizarEstadoAsync(envio.CodigoRastreo,
                new ActualizarEstadoDto { NuevoEstado = EstadoEnvio.Entregado, Ubicacion = "X" }));
    }

    [Fact]
    public async Task Rastrear_CodigoInexistente_LanzaRecursoNoEncontrado()
    {
        var (db, _, _) = await SembrarClientesAsync();
        var service = new EnvioService(db);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => service.ObtenerPorCodigoAsync("ENV-20000101-9999"));
    }

    [Fact]
    public async Task IntentoFallido_TercerIntento_CambiaAEnDevolucionAutomaticamente()
    {
        var (db, rem, dest) = await SembrarClientesAsync();
        var service = new EnvioService(db);
        var envio = await service.CrearEnvioAsync(NuevoEnvioDto(rem, dest));

        // Avanzar hasta EnReparto.
        await service.ActualizarEstadoAsync(envio.CodigoRastreo,
            new ActualizarEstadoDto { NuevoEstado = EstadoEnvio.EnTransito, Ubicacion = "Bodega" });
        await service.ActualizarEstadoAsync(envio.CodigoRastreo,
            new ActualizarEstadoDto { NuevoEstado = EstadoEnvio.EnReparto, Ubicacion = "Jalapa" });

        // 3 intentos fallidos.
        await service.RegistrarIntentoFallidoAsync(envio.CodigoRastreo, new IntentoFallidoDto { Ubicacion = "Jalapa" });
        await service.RegistrarIntentoFallidoAsync(envio.CodigoRastreo, new IntentoFallidoDto { Ubicacion = "Jalapa" });
        var resultado = await service.RegistrarIntentoFallidoAsync(envio.CodigoRastreo, new IntentoFallidoDto { Ubicacion = "Jalapa" });

        Assert.Equal(3, resultado.IntentosFallidos);
        Assert.Equal("EnDevolucion", resultado.Estado);
    }

    [Fact]
    public async Task IntentoFallido_FueraDeReparto_LanzaReglaNegocio()
    {
        var (db, rem, dest) = await SembrarClientesAsync();
        var service = new EnvioService(db);
        var envio = await service.CrearEnvioAsync(NuevoEnvioDto(rem, dest)); // estado Registrado

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            service.RegistrarIntentoFallidoAsync(envio.CodigoRastreo, new IntentoFallidoDto { Ubicacion = "X" }));
    }

    [Fact]
    public async Task GenerarReporte_CalculaPorcentajes()
    {
        var (db, rem, dest) = await SembrarClientesAsync();
        var service = new EnvioService(db);

        // Envío 1: entregado.
        var e1 = await service.CrearEnvioAsync(NuevoEnvioDto(rem, dest));
        await service.ActualizarEstadoAsync(e1.CodigoRastreo, new ActualizarEstadoDto { NuevoEstado = EstadoEnvio.EnTransito, Ubicacion = "B" });
        await service.ActualizarEstadoAsync(e1.CodigoRastreo, new ActualizarEstadoDto { NuevoEstado = EstadoEnvio.EnReparto, Ubicacion = "B" });
        await service.ActualizarEstadoAsync(e1.CodigoRastreo, new ActualizarEstadoDto { NuevoEstado = EstadoEnvio.Entregado, Ubicacion = "B" });

        // Envío 2: en proceso.
        await service.CrearEnvioAsync(NuevoEnvioDto(rem, dest));

        var reporte = await service.GenerarReporteAsync();

        Assert.Equal(2, reporte.TotalEnvios);
        Assert.Equal(1, reporte.Entregados);
        Assert.Equal(1, reporte.EnProceso);
        Assert.Equal(50.0, reporte.PorcentajeEntregaExitosa);
    }
}
