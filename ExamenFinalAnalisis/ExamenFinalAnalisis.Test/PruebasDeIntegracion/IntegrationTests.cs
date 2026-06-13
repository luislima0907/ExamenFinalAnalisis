using System.Net;
using System.Net.Http.Json;
using ExamenFinalAnalisis.Dtos;
using ExamenFinalAnalisis.Models;

namespace ExamenFinalAnalisis.Test.PruebasDeIntegracion;

public class IntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public IntegrationTests(ApiWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Get_Home_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task FlujoCompleto_CrearClientesRegistrarYRastrearEnvio()
    {
        // Crear remitente y destinatario.
        var remResp = await _client.PostAsJsonAsync("/api/clientes",
            new ClienteCrearDto { Nombre = "Remitente Test", Nit = "CF" });
        remResp.EnsureSuccessStatusCode();
        var remitente = await remResp.Content.ReadFromJsonAsync<Cliente>();

        var destResp = await _client.PostAsJsonAsync("/api/clientes",
            new ClienteCrearDto { Nombre = "Destinatario Test", Nit = "1234567" });
        destResp.EnsureSuccessStatusCode();
        var destinatario = await destResp.Content.ReadFromJsonAsync<Cliente>();

        // Registrar envío.
        var envioResp = await _client.PostAsJsonAsync("/api/envios", new EnvioCrearDto
        {
            RemitenteId = remitente!.Id,
            DestinatarioId = destinatario!.Id,
            DescripcionContenido = "Paquete de prueba",
            PesoKg = 2m,
            DepartamentoDestino = "Jalapa",
            OficinaOrigen = "Central"
        });
        Assert.Equal(HttpStatusCode.Created, envioResp.StatusCode);
        var envio = await envioResp.Content.ReadFromJsonAsync<EnvioDto>();

        Assert.NotNull(envio);
        Assert.Matches(@"^ENV-\d{8}-\d{4}$", envio!.CodigoRastreo);
        Assert.Equal(45.00m, envio.TarifaBase);
        Assert.Equal(2.25m, envio.Descuento); // descuento por NIT del destinatario
        Assert.Equal("Registrado", envio.Estado);

        // Rastrear envío.
        var rastreo = await _client.GetFromJsonAsync<EnvioDto>($"/api/envios/{envio.CodigoRastreo}");
        Assert.Equal(envio.CodigoRastreo, rastreo!.CodigoRastreo);
    }

    [Fact]
    public async Task ActualizarEstado_TransicionInvalida_Devuelve400()
    {
        var rem = await (await _client.PostAsJsonAsync("/api/clientes",
            new ClienteCrearDto { Nombre = "R" })).Content.ReadFromJsonAsync<Cliente>();
        var dest = await (await _client.PostAsJsonAsync("/api/clientes",
            new ClienteCrearDto { Nombre = "D" })).Content.ReadFromJsonAsync<Cliente>();

        var envio = await (await _client.PostAsJsonAsync("/api/envios", new EnvioCrearDto
        {
            RemitenteId = rem!.Id,
            DestinatarioId = dest!.Id,
            DescripcionContenido = "X",
            PesoKg = 1m,
            DepartamentoDestino = "Jalapa",
            OficinaOrigen = "Central"
        })).Content.ReadFromJsonAsync<EnvioDto>();

        // Registrado -> Entregado es inválido.
        var resp = await _client.PostAsJsonAsync($"/api/envios/{envio!.CodigoRastreo}/estado",
            new ActualizarEstadoDto { NuevoEstado = EstadoEnvio.Entregado, Ubicacion = "X" });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Rastrear_CodigoInexistente_Devuelve404()
    {
        var resp = await _client.GetAsync("/api/envios/ENV-20000101-9999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task ReporteEficiencia_Responde200()
    {
        var resp = await _client.GetAsync("/api/reportes/eficiencia");
        resp.EnsureSuccessStatusCode();
        var reporte = await resp.Content.ReadFromJsonAsync<ReporteEficienciaDto>();
        Assert.NotNull(reporte);
    }
}
