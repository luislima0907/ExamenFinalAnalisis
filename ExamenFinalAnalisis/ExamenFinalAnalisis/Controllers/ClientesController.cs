using ExamenFinalAnalisis.Data;
using ExamenFinalAnalisis.Dtos;
using ExamenFinalAnalisis.Models;
using ExamenFinalAnalisis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExamenFinalAnalisis.Controllers;

[ApiController]
[Route("api/clientes")]
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ClientesController(AppDbContext db) => _db = db;

    /// <summary>Crea un cliente (remitente o destinatario).</summary>
    [HttpPost]
    public async Task<ActionResult<Cliente>> Crear([FromBody] ClienteCrearDto dto)
    {
        var cliente = new Cliente
        {
            Nombre = dto.Nombre,
            Telefono = dto.Telefono,
            Nit = dto.Nit,
            Direccion = dto.Direccion
        };
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = cliente.Id }, cliente);
    }

    /// <summary>Lista todos los clientes.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> Listar()
        => Ok(await _db.Clientes.AsNoTracking().ToListAsync());

    /// <summary>Obtiene un cliente por su Id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cliente>> ObtenerPorId(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        return cliente is null ? NotFound(new { mensaje = $"No existe el cliente {id}." }) : Ok(cliente);
    }
}
