using ExamenFinalAnalisis.Dtos;
using ExamenFinalAnalisis.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExamenFinalAnalisis.Controllers;

[ApiController]
[Route("api/reportes")]
[Produces("application/json")]
public class ReportesController : ControllerBase
{
    private readonly IEnvioService _service;

    public ReportesController(IEnvioService service) => _service = service;

    /// <summary>Reporte de eficiencia de entrega (entregados, devueltos, porcentajes).</summary>
    [HttpGet("eficiencia")]
    public async Task<ActionResult<ReporteEficienciaDto>> Eficiencia()
        => Ok(await _service.GenerarReporteAsync());
}
