namespace ExamenFinalAnalisis.Services;

/// <summary>
/// Genera el código de rastreo con el formato ENV-YYYYMMDD-XXXX (Regla 5),
/// donde XXXX es un correlativo de 4 dígitos del día.
/// </summary>
public static class GeneradorCodigo
{
    public static string Generar(DateTime fecha, int correlativoDelDia)
    {
        if (correlativoDelDia < 1)
            throw new ArgumentOutOfRangeException(nameof(correlativoDelDia), "El correlativo debe ser >= 1.");

        var fechaParte = fecha.ToString("yyyyMMdd");
        var correlativoParte = correlativoDelDia.ToString("D4");
        return $"ENV-{fechaParte}-{correlativoParte}";
    }
}
