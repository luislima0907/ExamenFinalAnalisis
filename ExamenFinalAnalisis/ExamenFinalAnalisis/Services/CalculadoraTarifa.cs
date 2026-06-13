namespace ExamenFinalAnalisis.Services;

/// <summary>
/// Reglas de negocio para el cálculo de tarifas y descuentos.
/// Clase estática y pura para facilitar las pruebas unitarias.
/// </summary>
public static class CalculadoraTarifa
{
    public const decimal PorcentajeDescuentoNit = 0.05m; // 5%

    /// <summary>
    /// Tarifa base según el peso (Regla 1):
    ///  &lt;= 1kg          -> Q25.00
    ///  1.01 - 5 kg      -> Q45.00
    ///  5.01 - 10 kg     -> Q75.00
    ///  &gt; 10kg          -> Q100.00
    /// </summary>
    public static decimal CalcularTarifaBase(decimal pesoKg)
    {
        if (pesoKg <= 0)
            throw new ArgumentException("El peso debe ser mayor a 0.", nameof(pesoKg));

        return pesoKg switch
        {
            <= 1m => 25.00m,
            <= 5m => 45.00m,
            <= 10m => 75.00m,
            _ => 100.00m
        };
    }

    /// <summary>
    /// Valida un NIT guatemalteco de forma simple para efectos del prototipo:
    /// debe contener solo dígitos (permitiendo guion y una 'K' final como
    /// dígito verificador) y tener entre 2 y 9 caracteres significativos.
    /// "CF" / vacío / nulo NO es un NIT válido.
    /// </summary>
    public static bool EsNitValido(string? nit)
    {
        if (string.IsNullOrWhiteSpace(nit)) return false;

        var limpio = nit.Trim().Replace("-", "").ToUpperInvariant();
        if (limpio == "CF") return false;
        if (limpio.Length < 2 || limpio.Length > 9) return false;

        // Todos dígitos, salvo la última que puede ser 'K' (dígito verificador).
        for (int i = 0; i < limpio.Length; i++)
        {
            var c = limpio[i];
            var esUltima = i == limpio.Length - 1;
            if (!char.IsDigit(c) && !(esUltima && c == 'K'))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Calcula el descuento por NIT válido (5% sobre la tarifa base).
    /// Devuelve 0 si el NIT no es válido.
    /// </summary>
    public static decimal CalcularDescuento(decimal tarifaBase, string? nit)
    {
        if (!EsNitValido(nit)) return 0m;
        return Math.Round(tarifaBase * PorcentajeDescuentoNit, 2);
    }

    /// <summary>
    /// Resultado completo del cálculo de la tarifa.
    /// Aplica descuento si el remitente O el destinatario tienen NIT válido.
    /// </summary>
    public static (decimal tarifaBase, decimal descuento, decimal tarifaFinal) Calcular(
        decimal pesoKg, string? nitRemitente, string? nitDestinatario)
    {
        var tarifaBase = CalcularTarifaBase(pesoKg);
        var aplicaDescuento = EsNitValido(nitRemitente) || EsNitValido(nitDestinatario);
        var descuento = aplicaDescuento ? Math.Round(tarifaBase * PorcentajeDescuentoNit, 2) : 0m;
        var tarifaFinal = tarifaBase - descuento;
        return (tarifaBase, descuento, tarifaFinal);
    }
}
