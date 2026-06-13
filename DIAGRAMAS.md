# Diagramas — Envíos Rápidos GT

> Los diagramas están en formato **Mermaid**; GitHub los renderiza automáticamente.

---

## 1. Diagrama de flujo — Proceso de un envío

Flujo completo desde el registro hasta la entrega o devolución.

```mermaid
flowchart TD
    A([Inicio]) --> B[Registrar remitente y destinatario]
    B --> C[Registrar envío: contenido, peso, destino]
    C --> D[/Calcular tarifa base según peso/]
    D --> E{¿Remitente o destinatario<br/>con NIT válido?}
    E -- Sí --> F[Aplicar 5% de descuento]
    E -- No --> G[Sin descuento]
    F --> H[Generar código ENV-YYYYMMDD-XXXX]
    G --> H
    H --> I[Estado = Registrado<br/>+ registrar en historial]
    I --> J[Actualizar a EnTransito]
    J --> K[Actualizar a EnReparto]
    K --> L{¿Entrega exitosa?}
    L -- Sí --> M[Estado = Entregado]
    L -- No --> N[Registrar intento fallido]
    N --> O{¿Intentos = 3?}
    O -- No --> K
    O -- Sí --> P[Estado = EnDevolucion<br/>automático]
    P --> Q[Estado = Devuelto]
    M --> R([Fin])
    Q --> R
```

---

## 2. Diagrama de estados — Ciclo de vida del envío

```mermaid
stateDiagram-v2
    [*] --> Registrado
    Registrado --> EnTransito
    EnTransito --> EnReparto
    EnReparto --> Entregado
    EnReparto --> EnDevolucion : 3er intento fallido
    EnDevolucion --> Devuelto
    Entregado --> [*]
    Devuelto --> [*]
```

---

## 3. Diagrama de secuencia — Registrar un envío

```mermaid
sequenceDiagram
    actor Op as Operador
    participant API as EnviosController
    participant Svc as EnvioService
    participant Calc as CalculadoraTarifa
    participant Gen as GeneradorCodigo
    participant DB as AppDbContext (SQLite)

    Op->>API: POST /api/envios (remitente, destinatario, peso, destino)
    API->>Svc: CrearEnvioAsync(dto)
    Svc->>DB: Buscar remitente y destinatario
    DB-->>Svc: Clientes
    Svc->>Calc: Calcular(peso, nitRem, nitDest)
    Calc-->>Svc: tarifaBase, descuento, tarifaFinal
    Svc->>DB: Contar envíos del día
    DB-->>Svc: correlativo
    Svc->>Gen: Generar(fecha, correlativo)
    Gen-->>Svc: ENV-YYYYMMDD-XXXX
    Svc->>DB: Guardar envío + historial (Registrado)
    DB-->>Svc: OK
    Svc-->>API: EnvioDto
    API-->>Op: 201 Created (envío con código y tarifa)
```

---

## 4. Diagrama de secuencia — Intento fallido y devolución automática

```mermaid
sequenceDiagram
    actor Rep as Repartidor
    participant API as EnviosController
    participant Svc as EnvioService
    participant Maq as MaquinaEstados
    participant DB as AppDbContext (SQLite)

    Rep->>API: POST /api/envios/{codigo}/intentos-fallidos (ubicación)
    API->>Svc: RegistrarIntentoFallidoAsync(codigo, dto)
    Svc->>DB: Cargar envío por código
    DB-->>Svc: Envío (estado EnReparto)
    Svc->>Svc: IntentosFallidos++
    Svc->>DB: Registrar intento en historial
    alt IntentosFallidos >= 3
        Svc->>Maq: ¿Puede EnReparto -> EnDevolucion?
        Maq-->>Svc: true
        Svc->>DB: Estado = EnDevolucion + historial
    end
    Svc-->>API: EnvioDto actualizado
    API-->>Rep: 200 OK
```

---

## 5. Modelo de datos (entidades)

```mermaid
erDiagram
    CLIENTE ||--o{ ENVIO : "remitente"
    CLIENTE ||--o{ ENVIO : "destinatario"
    ENVIO ||--o{ HISTORIAL_ESTADO : "tiene"

    CLIENTE {
        int Id PK
        string Nombre
        string Telefono
        string Nit
        string Direccion
    }
    ENVIO {
        int Id PK
        string CodigoRastreo UK
        int RemitenteId FK
        int DestinatarioId FK
        string DescripcionContenido
        decimal PesoKg
        string DepartamentoDestino
        decimal TarifaBase
        decimal Descuento
        decimal TarifaFinal
        enum Estado
        int IntentosFallidos
        datetime FechaRegistro
    }
    HISTORIAL_ESTADO {
        int Id PK
        int EnvioId FK
        enum Estado
        string Ubicacion
        datetime FechaHora
        string Notas
    }
```
