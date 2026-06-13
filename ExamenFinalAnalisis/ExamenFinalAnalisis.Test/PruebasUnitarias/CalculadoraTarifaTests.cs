using ExamenFinalAnalisis.Services;

namespace ExamenFinalAnalisis.Test.PruebasUnitarias;

public class CalculadoraTarifaTests
{
    // ---------- Regla 1: tarifa por peso ----------

    [Theory]
    [InlineData(0.5, 25.00)]
    [InlineData(1.0, 25.00)]      // límite <= 1kg
    [InlineData(1.01, 45.00)]     // inicio del siguiente rango
    [InlineData(5.0, 45.00)]      // límite 5kg
    [InlineData(5.01, 75.00)]
    [InlineData(10.0, 75.00)]     // límite 10kg
    [InlineData(10.01, 100.00)]
    [InlineData(25.0, 100.00)]
    public void CalcularTarifaBase_DevuelveTarifaSegunPeso(double peso, double esperado)
    {
        var resultado = CalculadoraTarifa.CalcularTarifaBase((decimal)peso);
        Assert.Equal((decimal)esperado, resultado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void CalcularTarifaBase_PesoInvalido_LanzaExcepcion(double peso)
    {
        Assert.Throws<ArgumentException>(() => CalculadoraTarifa.CalcularTarifaBase((decimal)peso));
    }

    // ---------- Regla 7: validación de NIT ----------

    [Theory]
    [InlineData("1234567")]
    [InlineData("12345678")]
    [InlineData("123456-7")]
    [InlineData("1234567K")]
    public void EsNitValido_NitsValidos_DevuelveTrue(string nit)
    {
        Assert.True(CalculadoraTarifa.EsNitValido(nit));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("CF")]
    [InlineData("ABC123")]
    [InlineData("1234567890123")] // demasiado largo
    public void EsNitValido_NitsInvalidos_DevuelveFalse(string? nit)
    {
        Assert.False(CalculadoraTarifa.EsNitValido(nit));
    }

    // ---------- Regla 7: descuento 5% ----------

    [Fact]
    public void CalcularDescuento_NitValido_AplicaCincoPorCiento()
    {
        var descuento = CalculadoraTarifa.CalcularDescuento(100m, "1234567");
        Assert.Equal(5.00m, descuento);
    }

    [Fact]
    public void CalcularDescuento_NitInvalido_NoAplicaDescuento()
    {
        var descuento = CalculadoraTarifa.CalcularDescuento(100m, "CF");
        Assert.Equal(0m, descuento);
    }

    [Fact]
    public void Calcular_ConNitDelRemitente_AplicaDescuento()
    {
        var (tarifaBase, descuento, tarifaFinal) = CalculadoraTarifa.Calcular(3m, "7654321", null);
        Assert.Equal(45.00m, tarifaBase);
        Assert.Equal(2.25m, descuento);     // 5% de 45
        Assert.Equal(42.75m, tarifaFinal);
    }

    [Fact]
    public void Calcular_ConNitDelDestinatario_AplicaDescuento()
    {
        var (tarifaBase, descuento, tarifaFinal) = CalculadoraTarifa.Calcular(8m, null, "7654321");
        Assert.Equal(75.00m, tarifaBase);
        Assert.Equal(3.75m, descuento);
        Assert.Equal(71.25m, tarifaFinal);
    }

    [Fact]
    public void Calcular_SinNitValido_NoAplicaDescuento()
    {
        var (tarifaBase, descuento, tarifaFinal) = CalculadoraTarifa.Calcular(0.5m, "CF", null);
        Assert.Equal(25.00m, tarifaBase);
        Assert.Equal(0m, descuento);
        Assert.Equal(25.00m, tarifaFinal);
    }
}
