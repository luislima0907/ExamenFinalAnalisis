# Envíos Rápidos GT — API REST

Prototipo de API REST para la gestión y rastreo de paquetes de la empresa de
logística **Envíos Rápidos GT**, desarrollado como **Examen Final** del curso
**Análisis de Sistemas I**.

| | |
|---|---|
| **Alumno** | Luis Carlos Lima Pérez |
| **Carné** | 0907-23-20758 |
| **Catedrático** | Ing. Marco Tulio Valdez |
| **Sección** | A — Séptimo ciclo |
| **Fecha** | 13/Jun/2026 |

> **Nombre de repositorio solicitado por el enunciado:** `0907-23-20758_ANALISISA2026FINAL`

## 📚 Documentación del proyecto

- 📋 [Historias de Usuario](HISTORIAS_USUARIO.md) — 12 historias con criterios de aceptación medibles.
- 📈 [Diagramas](DIAGRAMAS.md) — flujo, estados, secuencia y modelo de datos.
- 🤖 [Informe de uso de IA](INFORME_IA.md) — prompts, reflexión y correcciones.

---

## 🧱 Tecnologías

- **.NET 8 / ASP.NET Core** (Web API + MVC)
- **Entity Framework Core 8** con **SQLite**
- **Swagger / OpenAPI** para documentación y pruebas interactivas
- **xUnit** + EF Core InMemory para pruebas unitarias y de integración
- **Docker** para el despliegue en **Render.com**

---

## 📁 Estructura

```
ExamenFinalAnalisis/                 ← solución (.sln)
├── ExamenFinalAnalisis/             ← proyecto API REST
│   ├── Controllers/                 ← ClientesController, EnviosController, ReportesController
│   ├── Models/                      ← Cliente, Envio, HistorialEstado, EstadoEnvio (enum)
│   ├── Dtos/                        ← objetos de entrada/salida
│   ├── Data/                        ← AppDbContext (EF Core + SQLite)
│   ├── Services/                    ← lógica de negocio (tarifa, estados, código, servicio)
│   ├── Program.cs                   ← configuración, DI, Swagger, manejo de errores
│   └── Dockerfile                   ← imagen para Render
└── ExamenFinalAnalisis.Test/        ← pruebas xUnit
    ├── PruebasUnitarias/            ← lógica de negocio aislada
    └── PruebasDeIntegracion/        ← endpoints con WebApplicationFactory
```

### Modelos (3)
1. **Cliente** — remitente/destinatario (incluye NIT para el descuento).
2. **Envio** — paquete con tarifa, estado, intentos y código de rastreo.
3. **HistorialEstado** — bitácora inmutable de cada cambio de estado.

---

## ▶️ Cómo ejecutar localmente

**Requisitos:** [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)

```bash
# 1. Ubicarse en la carpeta de la solución
cd ExamenFinalAnalisis

# 2. Restaurar y compilar
dotnet build

# 3. Ejecutar la API
dotnet run --project ExamenFinalAnalisis

# La API queda disponible (puerto local 8080 por defecto, o el de launchSettings):
#   Swagger UI:  http://localhost:8080/swagger
```

> La base de datos SQLite (`enviosgt.db`) se crea automáticamente al iniciar.
> Se puede cambiar su ruta con la variable de entorno `SQLITE_PATH`.

---

## 🧪 Cómo ejecutar las pruebas

```bash
cd ExamenFinalAnalisis
dotnet test
```

Resultado esperado: **60 pruebas pasan** (lógica de negocio + integración de endpoints).

| Suite | Qué valida |
|---|---|
| `CalculadoraTarifaTests` | Tarifa por peso (Regla 1), validación de NIT y descuento 5% (Regla 7). |
| `MaquinaEstadosTests` | Transiciones válidas/inválidas y estados finales (Regla 3). |
| `GeneradorCodigoTests` | Formato `ENV-YYYYMMDD-XXXX` (Regla 5). |
| `EnvioServiceTests` | Flujo completo: creación, descuento, correlativo, intentos y devolución automática. |
| `IntegrationTests` | Endpoints HTTP reales (201/400/404, flujo end-to-end). |

---

## 🌐 Endpoints de la API

Base: `/api`

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/clientes` | Crear cliente (remitente o destinatario). |
| `GET` | `/api/clientes` | Listar clientes. |
| `GET` | `/api/clientes/{id}` | Obtener cliente por Id. |
| `POST` | `/api/envios` | Registrar envío (calcula tarifa, descuento y código). |
| `GET` | `/api/envios` | Listar todos los envíos. |
| `GET` | `/api/envios/{codigo}` | Rastrear un envío por su código. |
| `GET` | `/api/envios/{codigo}/historial` | Historial de estados del envío. |
| `POST` | `/api/envios/{codigo}/estado` | Actualizar estado (requiere ubicación). |
| `POST` | `/api/envios/{codigo}/intentos-fallidos` | Registrar intento fallido (devolución al 3°). |
| `GET` | `/api/reportes/eficiencia` | Reporte de eficiencia de entrega. |

### Ejemplo de uso (cURL)

```bash
# 1) Crear remitente y destinatario (el destinatario tiene NIT válido)
curl -X POST http://localhost:8080/api/clientes -H "Content-Type: application/json" \
  -d '{"nombre":"Juan Perez","nit":"CF"}'
curl -X POST http://localhost:8080/api/clientes -H "Content-Type: application/json" \
  -d '{"nombre":"Tienda La Bodega","nit":"1234567"}'

# 2) Registrar un envío de 7 kg (tarifa Q75, -5% por NIT = Q71.25)
curl -X POST http://localhost:8080/api/envios -H "Content-Type: application/json" \
  -d '{"remitenteId":1,"destinatarioId":2,"descripcionContenido":"Electrodomestico",
       "pesoKg":7,"departamentoDestino":"Jalapa","oficinaOrigen":"Central GT"}'
# -> { "codigoRastreo":"ENV-20260613-0001", "tarifaBase":75.00, "descuento":3.75, "tarifaFinal":71.25, ... }

# 3) Avanzar estados
curl -X POST http://localhost:8080/api/envios/ENV-20260613-0001/estado \
  -H "Content-Type: application/json" -d '{"nuevoEstado":"EnTransito","ubicacion":"Bodega Central"}'
curl -X POST http://localhost:8080/api/envios/ENV-20260613-0001/estado \
  -H "Content-Type: application/json" -d '{"nuevoEstado":"EnReparto","ubicacion":"Oficina Jalapa"}'

# 4) Registrar intentos fallidos (al 3° pasa automáticamente a EnDevolucion)
curl -X POST http://localhost:8080/api/envios/ENV-20260613-0001/intentos-fallidos \
  -H "Content-Type: application/json" -d '{"ubicacion":"Jalapa","notas":"Nadie en casa"}'

# 5) Rastrear y ver reporte
curl http://localhost:8080/api/envios/ENV-20260613-0001
curl http://localhost:8080/api/reportes/eficiencia
```

> **La forma más simple de probar todo:** abrir **`/swagger`** en el navegador.

---

## ⚙️ Reglas de negocio implementadas

| # | Regla | Dónde |
|---|---|---|
| 1 | Tarifa automática por peso | `CalculadoraTarifa.CalcularTarifaBase` |
| 2 | Máx. 3 intentos → devolución automática | `EnvioService.RegistrarIntentoFallidoAsync` |
| 3 | Estados avanzan en una sola dirección | `MaquinaEstados` |
| 4 | Cada cambio incluye la ubicación | `ActualizarEstadoDto.Ubicacion` (obligatorio) |
| 5 | Código `ENV-YYYYMMDD-XXXX` | `GeneradorCodigo.Generar` |
| 6 | Historial con estado, ubicación, timestamp y notas | `HistorialEstado` |
| 7 | 5% de descuento por NIT válido | `CalculadoraTarifa.Calcular` |

---

## 🚀 Despliegue en Render.com

El proyecto se despliega con **Docker** (`ExamenFinalAnalisis/ExamenFinalAnalisis/Dockerfile`).

**Pasos en Render:**
1. *New → Web Service* y conectar este repositorio de GitHub.
2. **Language / Runtime:** `Docker`.
3. **Root Directory / Docker Context:** `ExamenFinalAnalisis/ExamenFinalAnalisis`
   (carpeta donde está el `Dockerfile` y el `.csproj`).
4. Render detecta el puerto mediante la variable `PORT` (ya manejada en `Program.cs`).
5. *Create Web Service* → al terminar el build, la API queda en
   `https://<tu-servicio>.onrender.com/swagger`.

> **Nota:** SQLite usa el sistema de archivos del contenedor; en el plan gratuito de
> Render el almacenamiento es efímero (los datos se reinician al redeplegar). Es
> suficiente para un prototipo. Para persistencia se puede montar un *Disk* y apuntar
> `SQLITE_PATH` a esa ruta.

Alternativamente, el archivo [`render.yaml`](render.yaml) permite el despliegue como *Blueprint*.
