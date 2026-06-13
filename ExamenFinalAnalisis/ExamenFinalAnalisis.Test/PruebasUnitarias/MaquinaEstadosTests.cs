using ExamenFinalAnalisis.Models;
using ExamenFinalAnalisis.Services;

namespace ExamenFinalAnalisis.Test.PruebasUnitarias;

public class MaquinaEstadosTests
{
    // ---------- Regla 3: transiciones válidas ----------

    [Theory]
    [InlineData(EstadoEnvio.Registrado, EstadoEnvio.EnTransito)]
    [InlineData(EstadoEnvio.EnTransito, EstadoEnvio.EnReparto)]
    [InlineData(EstadoEnvio.EnReparto, EstadoEnvio.Entregado)]
    [InlineData(EstadoEnvio.EnReparto, EstadoEnvio.EnDevolucion)]
    [InlineData(EstadoEnvio.EnDevolucion, EstadoEnvio.Devuelto)]
    public void PuedeTransicionar_TransicionesValidas_DevuelveTrue(EstadoEnvio actual, EstadoEnvio nuevo)
    {
        Assert.True(MaquinaEstados.PuedeTransicionar(actual, nuevo));
    }

    [Theory]
    [InlineData(EstadoEnvio.Registrado, EstadoEnvio.EnReparto)]   // salto de estado
    [InlineData(EstadoEnvio.EnReparto, EstadoEnvio.Registrado)]   // retroceso
    [InlineData(EstadoEnvio.EnTransito, EstadoEnvio.Registrado)]  // retroceso
    [InlineData(EstadoEnvio.Entregado, EstadoEnvio.EnReparto)]    // desde estado final
    [InlineData(EstadoEnvio.Devuelto, EstadoEnvio.EnDevolucion)]  // desde estado final
    [InlineData(EstadoEnvio.Registrado, EstadoEnvio.Entregado)]   // salto grande
    public void PuedeTransicionar_TransicionesInvalidas_DevuelveFalse(EstadoEnvio actual, EstadoEnvio nuevo)
    {
        Assert.False(MaquinaEstados.PuedeTransicionar(actual, nuevo));
    }

    [Theory]
    [InlineData(EstadoEnvio.Entregado)]
    [InlineData(EstadoEnvio.Devuelto)]
    public void EsEstadoFinal_EstadosTerminales_DevuelveTrue(EstadoEnvio estado)
    {
        Assert.True(MaquinaEstados.EsEstadoFinal(estado));
    }

    [Fact]
    public void SiguientesEstados_EnReparto_TieneEntregadoYDevolucion()
    {
        var siguientes = MaquinaEstados.SiguientesEstados(EstadoEnvio.EnReparto);
        Assert.Contains(EstadoEnvio.Entregado, siguientes);
        Assert.Contains(EstadoEnvio.EnDevolucion, siguientes);
        Assert.Equal(2, siguientes.Length);
    }
}
