using System.ComponentModel.DataAnnotations;

namespace ExamenFinalAnalisis.Models;

/// <summary>
/// Cliente que puede actuar como remitente o destinatario de un envío.
/// Si posee un NIT válido se aplica un 5% de descuento sobre la tarifa.
/// </summary>
public class Cliente
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    /// <summary>
    /// NIT del cliente. Puede ser nulo (consumidor final / "CF").
    /// Cuando es un NIT válido se otorga el descuento.
    /// </summary>
    [MaxLength(20)]
    public string? Nit { get; set; }

    [MaxLength(200)]
    public string? Direccion { get; set; }
}
