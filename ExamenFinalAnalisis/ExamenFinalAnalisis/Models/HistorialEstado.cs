using System.ComponentModel.DataAnnotations;

namespace ExamenFinalAnalisis.Models;

/// <summary>
/// Registro inmutable de cada cambio de estado de un envío.
/// Se crea automáticamente cada vez que el envío cambia de estado.
/// </summary>
public class HistorialEstado
{
    public int Id { get; set; }

    public int EnvioId { get; set; }
    public Envio? Envio { get; set; }

    /// <summary>Nuevo estado al que pasó el envío.</summary>
    public EstadoEnvio Estado { get; set; }

    /// <summary>Oficina/ubicación donde se realizó la actualización.</summary>
    [Required]
    [MaxLength(100)]
    public string Ubicacion { get; set; } = string.Empty;

    /// <summary>Timestamp automático del cambio.</summary>
    public DateTime FechaHora { get; set; }

    /// <summary>Notas opcionales del operador.</summary>
    [MaxLength(300)]
    public string? Notas { get; set; }
}
