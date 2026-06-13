using ExamenFinalAnalisis.Data;
using ExamenFinalAnalisis.Dtos;
using ExamenFinalAnalisis.Models;
using Microsoft.EntityFrameworkCore;

namespace ExamenFinalAnalisis.Services;

public interface IEnvioService
{
    Task<EnvioDto> CrearEnvioAsync(EnvioCrearDto dto);
    Task<EnvioDto> ActualizarEstadoAsync(string codigo, ActualizarEstadoDto dto);
    Task<EnvioDto> RegistrarIntentoFallidoAsync(string codigo, IntentoFallidoDto dto);
    Task<EnvioDto> ObtenerPorCodigoAsync(string codigo);
    Task<List<EnvioDto>> ListarAsync();
    Task<List<HistorialDto>> ObtenerHistorialAsync(string codigo);
    Task<ReporteEficienciaDto> GenerarReporteAsync();
}

public class EnvioService : IEnvioService
{
    private readonly AppDbContext _db;

    public EnvioService(AppDbContext db) => _db = db;

    public async Task<EnvioDto> CrearEnvioAsync(EnvioCrearDto dto)
    {
        var remitente = await _db.Clientes.FindAsync(dto.RemitenteId)
            ?? throw new ReglaNegocioException($"No existe el remitente con Id {dto.RemitenteId}.");
        var destinatario = await _db.Clientes.FindAsync(dto.DestinatarioId)
            ?? throw new ReglaNegocioException($"No existe el destinatario con Id {dto.DestinatarioId}.");

        if (dto.PesoKg <= 0)
            throw new ReglaNegocioException("El peso debe ser mayor a 0.");

        // Regla 1 y 7: cálculo automático de tarifa + descuento por NIT válido.
        var (tarifaBase, descuento, tarifaFinal) =
            CalculadoraTarifa.Calcular(dto.PesoKg, remitente.Nit, destinatario.Nit);

        var ahora = DateTime.UtcNow;

        // Regla 5: código de rastreo ENV-YYYYMMDD-XXXX (correlativo del día).
        var inicioDia = ahora.Date;
        var finDia = inicioDia.AddDays(1);
        var enviosHoy = await _db.Envios.CountAsync(e => e.FechaRegistro >= inicioDia && e.FechaRegistro < finDia);
        var codigo = GeneradorCodigo.Generar(ahora, enviosHoy + 1);

        var envio = new Envio
        {
            CodigoRastreo = codigo,
            RemitenteId = remitente.Id,
            DestinatarioId = destinatario.Id,
            DescripcionContenido = dto.DescripcionContenido,
            PesoKg = dto.PesoKg,
            DepartamentoDestino = dto.DepartamentoDestino,
            TarifaBase = tarifaBase,
            Descuento = descuento,
            TarifaFinal = tarifaFinal,
            Estado = EstadoEnvio.Registrado,
            IntentosFallidos = 0,
            FechaRegistro = ahora
        };

        // Regla 6: registrar el estado inicial en el historial.
        envio.Historial.Add(new HistorialEstado
        {
            Estado = EstadoEnvio.Registrado,
            Ubicacion = dto.OficinaOrigen,
            FechaHora = ahora,
            Notas = "Envío registrado."
        });

        _db.Envios.Add(envio);
        await _db.SaveChangesAsync();

        return await ObtenerPorCodigoAsync(codigo);
    }

    public async Task<EnvioDto> ActualizarEstadoAsync(string codigo, ActualizarEstadoDto dto)
    {
        var envio = await CargarEnvioAsync(codigo);

        // Regla 3: solo se permite avanzar en la dirección válida.
        if (!MaquinaEstados.PuedeTransicionar(envio.Estado, dto.NuevoEstado))
        {
            throw new ReglaNegocioException(
                $"No se permite cambiar de '{envio.Estado}' a '{dto.NuevoEstado}'. " +
                $"Estados válidos: {string.Join(", ", MaquinaEstados.SiguientesEstados(envio.Estado))}.");
        }

        AplicarCambioEstado(envio, dto.NuevoEstado, dto.Ubicacion, dto.Notas);
        await _db.SaveChangesAsync();

        return MapearEnvio(envio);
    }

    public async Task<EnvioDto> RegistrarIntentoFallidoAsync(string codigo, IntentoFallidoDto dto)
    {
        var envio = await CargarEnvioAsync(codigo);

        // Solo tiene sentido registrar intentos cuando está en reparto.
        if (envio.Estado != EstadoEnvio.EnReparto)
            throw new ReglaNegocioException(
                $"Solo se pueden registrar intentos de entrega cuando el envío está 'EnReparto' (estado actual: '{envio.Estado}').");

        envio.IntentosFallidos++;

        envio.Historial.Add(new HistorialEstado
        {
            Estado = envio.Estado,
            Ubicacion = dto.Ubicacion,
            FechaHora = DateTime.UtcNow,
            Notas = $"Intento de entrega fallido #{envio.IntentosFallidos}. {dto.Notas}".Trim()
        });

        // Regla 2: al fallar el tercer intento, pasa automáticamente a EnDevolucion.
        if (envio.IntentosFallidos >= MaquinaEstados.MaxIntentos)
        {
            AplicarCambioEstado(envio, EstadoEnvio.EnDevolucion, dto.Ubicacion,
                $"Cambio automático a En Devolución tras {envio.IntentosFallidos} intentos fallidos.");
        }

        await _db.SaveChangesAsync();
        return MapearEnvio(envio);
    }

    public async Task<EnvioDto> ObtenerPorCodigoAsync(string codigo)
    {
        var envio = await CargarEnvioAsync(codigo);
        return MapearEnvio(envio);
    }

    public async Task<List<EnvioDto>> ListarAsync()
    {
        var envios = await _db.Envios
            .Include(e => e.Remitente)
            .Include(e => e.Destinatario)
            .Include(e => e.Historial)
            .OrderByDescending(e => e.FechaRegistro)
            .ToListAsync();

        return envios.Select(MapearEnvio).ToList();
    }

    public async Task<List<HistorialDto>> ObtenerHistorialAsync(string codigo)
    {
        var envio = await CargarEnvioAsync(codigo);
        return envio.Historial
            .OrderBy(h => h.FechaHora)
            .Select(h => new HistorialDto
            {
                Estado = h.Estado.ToString(),
                Ubicacion = h.Ubicacion,
                FechaHora = h.FechaHora,
                Notas = h.Notas
            }).ToList();
    }

    public async Task<ReporteEficienciaDto> GenerarReporteAsync()
    {
        var envios = await _db.Envios.ToListAsync();
        var total = envios.Count;
        var entregados = envios.Count(e => e.Estado == EstadoEnvio.Entregado);
        var devueltos = envios.Count(e => e.Estado == EstadoEnvio.Devuelto);
        var enProceso = total - entregados - devueltos;
        var conFallidos = envios.Count(e => e.IntentosFallidos > 0);

        return new ReporteEficienciaDto
        {
            TotalEnvios = total,
            Entregados = entregados,
            Devueltos = devueltos,
            EnProceso = enProceso,
            ConIntentosFallidos = conFallidos,
            PorcentajeEntregaExitosa = total == 0 ? 0 : Math.Round(entregados * 100.0 / total, 2),
            PorcentajeDevolucion = total == 0 ? 0 : Math.Round(devueltos * 100.0 / total, 2)
        };
    }

    // ---------------- helpers ----------------

    private async Task<Envio> CargarEnvioAsync(string codigo)
    {
        var envio = await _db.Envios
            .Include(e => e.Remitente)
            .Include(e => e.Destinatario)
            .Include(e => e.Historial)
            .FirstOrDefaultAsync(e => e.CodigoRastreo == codigo);

        return envio ?? throw new RecursoNoEncontradoException($"No existe un envío con código '{codigo}'.");
    }

    // Regla 4 y 6: cada cambio de estado registra ubicación, timestamp y notas.
    private static void AplicarCambioEstado(Envio envio, EstadoEnvio nuevo, string ubicacion, string? notas)
    {
        envio.Estado = nuevo;
        envio.Historial.Add(new HistorialEstado
        {
            Estado = nuevo,
            Ubicacion = ubicacion,
            FechaHora = DateTime.UtcNow,
            Notas = notas
        });
    }

    private static EnvioDto MapearEnvio(Envio e) => new()
    {
        Id = e.Id,
        CodigoRastreo = e.CodigoRastreo,
        Remitente = e.Remitente?.Nombre ?? string.Empty,
        Destinatario = e.Destinatario?.Nombre ?? string.Empty,
        DescripcionContenido = e.DescripcionContenido,
        PesoKg = e.PesoKg,
        DepartamentoDestino = e.DepartamentoDestino,
        TarifaBase = e.TarifaBase,
        Descuento = e.Descuento,
        TarifaFinal = e.TarifaFinal,
        Estado = e.Estado.ToString(),
        IntentosFallidos = e.IntentosFallidos,
        FechaRegistro = e.FechaRegistro,
        Historial = e.Historial
            .OrderBy(h => h.FechaHora)
            .Select(h => new HistorialDto
            {
                Estado = h.Estado.ToString(),
                Ubicacion = h.Ubicacion,
                FechaHora = h.FechaHora,
                Notas = h.Notas
            }).ToList()
    };
}
