using ExamenFinalAnalisis.Dtos;
using ExamenFinalAnalisis.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExamenFinalAnalisis.Controllers;

[ApiController]
[Route("api/envios")]
[Produces("application/json")]
public class EnviosController : ControllerBase
{
    private readonly IEnvioService _service;

    public EnviosController(IEnvioService service) => _service = service;

    /// <summary>Registra un nuevo envío. Calcula tarifa, descuento por NIT y código de rastreo.</summary>
    [HttpPost]
    public async Task<ActionResult<EnvioDto>> Registrar([FromBody] EnvioCrearDto dto)
    {
        var envio = await _service.CrearEnvioAsync(dto);
        return CreatedAtAction(nameof(Rastrear), new { codigo = envio.CodigoRastreo }, envio);
    }

    /// <summary>Lista todos los envíos.</summary>
    [HttpGet]
    public async Task<ActionResult<List<EnvioDto>>> Listar()
        => Ok(await _service.ListarAsync());

    /// <summary>Rastrea un envío por su código (ENV-YYYYMMDD-XXXX).</summary>
    [HttpGet("{codigo}")]
    public async Task<ActionResult<EnvioDto>> Rastrear(string codigo)
        => Ok(await _service.ObtenerPorCodigoAsync(codigo));

    /// <summary>Obtiene el historial de estados de un envío.</summary>
    [HttpGet("{codigo}/historial")]
    public async Task<ActionResult<List<HistorialDto>>> Historial(string codigo)
        => Ok(await _service.ObtenerHistorialAsync(codigo));

    /// <summary>Actualiza el estado de un envío (con ubicación obligatoria).</summary>
    [HttpPost("{codigo}/estado")]
    public async Task<ActionResult<EnvioDto>> ActualizarEstado(string codigo, [FromBody] ActualizarEstadoDto dto)
        => Ok(await _service.ActualizarEstadoAsync(codigo, dto));

    /// <summary>Registra un intento de entrega fallido. Al tercer intento pasa a En Devolución.</summary>
    [HttpPost("{codigo}/intentos-fallidos")]
    public async Task<ActionResult<EnvioDto>> RegistrarIntentoFallido(string codigo, [FromBody] IntentoFallidoDto dto)
        => Ok(await _service.RegistrarIntentoFallidoAsync(codigo, dto));
}
