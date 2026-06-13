namespace ExamenFinalAnalisis.Models;

/// <summary>
/// Estados posibles de un envío. El flujo solo avanza en una dirección
/// (ver <see cref="Services.MaquinaEstados"/>):
/// Registrado -> EnTransito -> EnReparto -> Entregado
///                                       -> EnDevolucion -> Devuelto
/// </summary>
public enum EstadoEnvio
{
    Registrado = 0,
    EnTransito = 1,
    EnReparto = 2,
    Entregado = 3,
    EnDevolucion = 4,
    Devuelto = 5
}
