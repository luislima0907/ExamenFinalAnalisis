using System.ComponentModel.DataAnnotations;
using ExamenFinalAnalisis.Models;

namespace ExamenFinalAnalisis.Dtos;

// ----------- Clientes -----------

public class ClienteCrearDto
{
    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(20)]
    public string? Nit { get; set; }

    [MaxLength(200)]
    public string? Direccion { get; set; }
}

// ----------- Envíos -----------

public class EnvioCrearDto
{
    [Required]
    public int RemitenteId { get; set; }

    [Required]
    public int DestinatarioId { get; set; }

    [Required]
    [MaxLength(100)]
    public string DescripcionContenido { get; set; } = string.Empty;

    [Range(0.01, 10000, ErrorMessage = "El peso debe ser mayor a 0.")]
    public decimal PesoKg { get; set; }

    [Required]
    [MaxLength(100)]
    public string DepartamentoDestino { get; set; } = string.Empty;

    /// <summary>Oficina donde se registra el envío (queda en el historial).</summary>
    [Required]
    [MaxLength(100)]
    public string OficinaOrigen { get; set; } = string.Empty;
}

public class ActualizarEstadoDto
{
    [Required]
    public EstadoEnvio NuevoEstado { get; set; }

    /// <summary>Oficina donde se realiza el cambio de estado (obligatoria).</summary>
    [Required]
    [MaxLength(100)]
    public string Ubicacion { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Notas { get; set; }
}

public class IntentoFallidoDto
{
    [Required]
    [MaxLength(100)]
    public string Ubicacion { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Notas { get; set; }
}

// ----------- Respuestas -----------

public class HistorialDto
{
    public string Estado { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string? Notas { get; set; }
}

public class EnvioDto
{
    public int Id { get; set; }
    public string CodigoRastreo { get; set; } = string.Empty;
    public string Remitente { get; set; } = string.Empty;
    public string Destinatario { get; set; } = string.Empty;
    public string DescripcionContenido { get; set; } = string.Empty;
    public decimal PesoKg { get; set; }
    public string DepartamentoDestino { get; set; } = string.Empty;
    public decimal TarifaBase { get; set; }
    public decimal Descuento { get; set; }
    public decimal TarifaFinal { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int IntentosFallidos { get; set; }
    public DateTime FechaRegistro { get; set; }
    public List<HistorialDto> Historial { get; set; } = new();
}

// ----------- Reporte -----------

public class ReporteEficienciaDto
{
    public int TotalEnvios { get; set; }
    public int Entregados { get; set; }
    public int Devueltos { get; set; }
    public int EnProceso { get; set; }
    public int ConIntentosFallidos { get; set; }
    public double PorcentajeEntregaExitosa { get; set; }
    public double PorcentajeDevolucion { get; set; }
}
