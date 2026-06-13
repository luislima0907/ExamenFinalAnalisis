using ExamenFinalAnalisis.Services;

namespace ExamenFinalAnalisis.Test.PruebasUnitarias;

public class GeneradorCodigoTests
{
    // ---------- Regla 5: formato ENV-YYYYMMDD-XXXX ----------

    [Fact]
    public void Generar_FormatoCorrecto()
    {
        var fecha = new DateTime(2026, 6, 13);
        var codigo = GeneradorCodigo.Generar(fecha, 1);
        Assert.Equal("ENV-20260613-0001", codigo);
    }

    [Fact]
    public void Generar_CorrelativoConPadding()
    {
        var fecha = new DateTime(2026, 1, 5);
        var codigo = GeneradorCodigo.Generar(fecha, 42);
        Assert.Equal("ENV-20260105-0042", codigo);
    }

    [Fact]
    public void Generar_CorrelativoGrande()
    {
        var fecha = new DateTime(2026, 12, 31);
        var codigo = GeneradorCodigo.Generar(fecha, 1234);
        Assert.Equal("ENV-20261231-1234", codigo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Generar_CorrelativoInvalido_LanzaExcepcion(int correlativo)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => GeneradorCodigo.Generar(new DateTime(2026, 6, 13), correlativo));
    }
}
