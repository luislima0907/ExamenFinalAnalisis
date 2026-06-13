using System.ComponentModel.DataAnnotations;

namespace ExamenFinalAnalisis.Models;

/// <summary>
/// Representa un paquete/envío gestionado por Envíos Rápidos GT.
/// </summary>
public class Envio
{
    public int Id { get; set; }

    /// <summary>Código de rastreo autogenerado con formato ENV-YYYYMMDD-XXXX.</summary>
    [Required]
    [MaxLength(20)]
    public string CodigoRastreo { get; set; } = string.Empty;

    // Remitente
    public int RemitenteId { get; set; }
    public Cliente? Remitente { get; set; }

    // Destinatario
    public int DestinatarioId { get; set; }
    public Cliente? Destinatario { get; set; }

    [Required]
    [MaxLength(100)]
    public string DescripcionContenido { get; set; } = string.Empty;

    /// <summary>Peso del paquete en kilogramos. Determina la tarifa.</summary>
    [Range(0.01, 10000)]
    public decimal PesoKg { get; set; }

    /// <summary>Departamento/oficina destino del envío.</summary>
    [Required]
    [MaxLength(100)]
    public string DepartamentoDestino { get; set; } = string.Empty;

    /// <summary>Tarifa base calculada por peso (antes de descuento).</summary>
    public decimal TarifaBase { get; set; }

    /// <summary>Monto de descuento aplicado por NIT válido (5%).</summary>
    public decimal Descuento { get; set; }

    /// <summary>Tarifa final a cobrar = TarifaBase - Descuento.</summary>
    public decimal TarifaFinal { get; set; }

    public EstadoEnvio Estado { get; set; } = EstadoEnvio.Registrado;

    /// <summary>Cantidad de intentos de entrega fallidos (máximo 3).</summary>
    public int IntentosFallidos { get; set; }

    public DateTime FechaRegistro { get; set; }

    public List<HistorialEstado> Historial { get; set; } = new();
}
