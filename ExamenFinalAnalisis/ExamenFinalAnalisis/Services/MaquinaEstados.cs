using ExamenFinalAnalisis.Models;

namespace ExamenFinalAnalisis.Services;

/// <summary>
/// Máquina de estados del envío. Los estados solo avanzan en una dirección
/// (Regla 3). No se permiten retrocesos ni saltos.
///
/// Registrado -> EnTransito -> EnReparto -> Entregado
///                                       -> EnDevolucion -> Devuelto
/// </summary>
public static class MaquinaEstados
{
    public const int MaxIntentos = 3;

    private static readonly Dictionary<EstadoEnvio, EstadoEnvio[]> Transiciones = new()
    {
        [EstadoEnvio.Registrado]   = new[] { EstadoEnvio.EnTransito },
        [EstadoEnvio.EnTransito]   = new[] { EstadoEnvio.EnReparto },
        [EstadoEnvio.EnReparto]    = new[] { EstadoEnvio.Entregado, EstadoEnvio.EnDevolucion },
        [EstadoEnvio.EnDevolucion] = new[] { EstadoEnvio.Devuelto },
        [EstadoEnvio.Entregado]    = Array.Empty<EstadoEnvio>(), // estado final
        [EstadoEnvio.Devuelto]     = Array.Empty<EstadoEnvio>()  // estado final
    };

    /// <summary>Indica si se permite pasar de <paramref name="actual"/> a <paramref name="nuevo"/>.</summary>
    public static bool PuedeTransicionar(EstadoEnvio actual, EstadoEnvio nuevo)
    {
        return Transiciones.TryGetValue(actual, out var permitidos) && permitidos.Contains(nuevo);
    }

    /// <summary>Estados a los que se puede avanzar desde el estado actual.</summary>
    public static EstadoEnvio[] SiguientesEstados(EstadoEnvio actual)
    {
        return Transiciones.TryGetValue(actual, out var permitidos) ? permitidos : Array.Empty<EstadoEnvio>();
    }

    public static bool EsEstadoFinal(EstadoEnvio estado) => SiguientesEstados(estado).Length == 0;
}
