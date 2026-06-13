# Historias de Usuario — Envíos Rápidos GT

**Asignatura:** Análisis de Sistemas I
**Alumno:** Luis Carlos Lima Pérez
**Carné:** 0907-23-20758
**Catedrático:** Ing. Marco Tulio Valdez
**Fecha:** 13/Jun/2026

Formato utilizado para cada historia:

> **Como** \<rol\> **quiero** \<funcionalidad\> **para** \<beneficio\>.

Cada historia incluye **criterios de aceptación medibles** (verificables) y su **prioridad**.
Las historias cubren las 7 reglas de negocio del enunciado.

---

## HU-01 — Registrar cliente

**Como** operador de oficina
**quiero** registrar a un cliente (remitente o destinatario) con su nombre, teléfono, dirección y NIT
**para** poder asociarlo a los envíos y aplicar descuentos cuando corresponda.

**Criterios de aceptación**
- [ ] Se puede crear un cliente enviando al menos el nombre (obligatorio).
- [ ] El NIT es opcional; si no se proporciona se asume consumidor final (CF).
- [ ] Al crear un cliente el sistema devuelve su `id` único y código HTTP `201 Created`.
- [ ] El nombre no puede exceder 150 caracteres.

**Prioridad:** Alta — **Endpoint:** `POST /api/clientes`

---

## HU-02 — Registrar un envío con tarifa automática

**Como** operador de oficina
**quiero** registrar un envío indicando remitente, destinatario, contenido, peso y departamento destino
**para** que el sistema calcule la tarifa automáticamente y evite errores de cobro manual.

**Criterios de aceptación** (Regla 1)
- [ ] La tarifa base se calcula automáticamente según el peso:
  - `≤ 1 kg` → **Q25.00**
  - `1.01 – 5 kg` → **Q45.00**
  - `5.01 – 10 kg` → **Q75.00**
  - `> 10 kg` → **Q100.00**
- [ ] El peso debe ser mayor a 0; de lo contrario se rechaza con `400 Bad Request`.
- [ ] El envío se crea con estado inicial **Registrado**.
- [ ] Si el remitente o el destinatario no existen, se devuelve `400` con mensaje claro.

**Prioridad:** Alta — **Endpoint:** `POST /api/envios`

---

## HU-03 — Aplicar descuento por NIT válido

**Como** cliente con NIT
**quiero** que se aplique un descuento cuando tengo un NIT válido
**para** pagar una tarifa preferencial.

**Criterios de aceptación** (Regla 7)
- [ ] Si el remitente **o** el destinatario tienen un NIT válido, se aplica **5% de descuento** sobre la tarifa base.
- [ ] "CF", vacío o NIT con formato inválido **no** generan descuento.
- [ ] La respuesta muestra `tarifaBase`, `descuento` y `tarifaFinal = tarifaBase - descuento`.
- [ ] El descuento se redondea a 2 decimales.

**Prioridad:** Alta — **Endpoint:** `POST /api/envios`

---

## HU-04 — Generar código de rastreo automático

**Como** sistema
**quiero** generar un código de rastreo único al registrar cada envío
**para** que el cliente pueda identificar y dar seguimiento a su paquete.

**Criterios de aceptación** (Regla 5)
- [ ] El código sigue el formato **`ENV-YYYYMMDD-XXXX`** (p. ej. `ENV-20260613-0001`).
- [ ] `XXXX` es un correlativo de 4 dígitos que reinicia cada día.
- [ ] El código es único: no se repite para dos envíos distintos.

**Prioridad:** Alta — **Endpoint:** `POST /api/envios`

---

## HU-05 — Rastrear un envío en tiempo real

**Como** cliente
**quiero** consultar el estado actual de mi envío con su código de rastreo
**para** saber dónde está mi paquete sin llamar a la oficina.

**Criterios de aceptación**
- [ ] Consultando por el código se devuelve el envío con su estado actual, ubicación y datos de tarifa.
- [ ] Si el código no existe se devuelve `404 Not Found` con mensaje descriptivo.

**Prioridad:** Alta — **Endpoint:** `GET /api/envios/{codigo}`

---

## HU-06 — Actualizar el estado de un envío con ubicación

**Como** operador de oficina
**quiero** actualizar el estado de un envío indicando la oficina donde ocurre el cambio
**para** mantener la trazabilidad actualizada en todo momento.

**Criterios de aceptación** (Reglas 4 y 6)
- [ ] Cada actualización **exige** la ubicación (oficina); sin ella se rechaza con `400`.
- [ ] Cada cambio registra automáticamente: nuevo estado, ubicación, timestamp y notas opcionales.

**Prioridad:** Alta — **Endpoint:** `POST /api/envios/{codigo}/estado`

---

## HU-07 — Validar que los estados solo avancen en una dirección

**Como** administrador del sistema
**quiero** que los estados solo avancen según el flujo permitido
**para** evitar inconsistencias (retrocesos o saltos de estado).

**Criterios de aceptación** (Regla 3)
- [ ] Transiciones válidas:
  `Registrado → EnTransito → EnReparto → Entregado`
  `EnReparto → EnDevolucion → Devuelto`
- [ ] Cualquier retroceso o salto (p. ej. `Registrado → Entregado`) se rechaza con `400` indicando los estados válidos.
- [ ] `Entregado` y `Devuelto` son estados finales: no admiten más cambios.

**Prioridad:** Alta — **Endpoint:** `POST /api/envios/{codigo}/estado`

---

## HU-08 — Registrar intentos de entrega fallidos

**Como** repartidor
**quiero** registrar un intento de entrega fallido con la ubicación
**para** dejar constancia de cada intento y de los paquetes con problemas.

**Criterios de aceptación** (Regla 2)
- [ ] Solo se permiten intentos cuando el envío está en estado **EnReparto**.
- [ ] Cada intento incrementa el contador de intentos fallidos y queda en el historial.
- [ ] Se permiten como máximo **3 intentos**.

**Prioridad:** Alta — **Endpoint:** `POST /api/envios/{codigo}/intentos-fallidos`

---

## HU-09 — Devolución automática al tercer intento fallido

**Como** administrador del sistema
**quiero** que el envío pase automáticamente a "En Devolución" al fallar el tercer intento
**para** procesar la devolución sin intervención manual.

**Criterios de aceptación** (Regla 2)
- [ ] Al registrar el **tercer** intento fallido, el estado cambia automáticamente a **EnDevolucion**.
- [ ] El cambio automático queda registrado en el historial con su nota explicativa.

**Prioridad:** Alta — **Endpoint:** `POST /api/envios/{codigo}/intentos-fallidos`

---

## HU-10 — Consultar el historial de un envío

**Como** cliente o supervisor
**quiero** ver el historial completo de cambios de estado de un envío
**para** auditar la trazabilidad del paquete.

**Criterios de aceptación** (Regla 6)
- [ ] El historial lista cada cambio con: estado, ubicación, fecha/hora y notas.
- [ ] Los registros se muestran en orden cronológico.

**Prioridad:** Media — **Endpoint:** `GET /api/envios/{codigo}/historial`

---

## HU-11 — Generar reporte de eficiencia de entrega

**Como** gerente de operaciones
**quiero** un reporte de eficiencia de entregas
**para** medir el desempeño y tomar decisiones.

**Criterios de aceptación**
- [ ] El reporte muestra: total de envíos, entregados, devueltos, en proceso y con intentos fallidos.
- [ ] Calcula el **porcentaje de entrega exitosa** y el **porcentaje de devolución**.
- [ ] Si no hay envíos, los porcentajes son 0 (sin error de división por cero).

**Prioridad:** Media — **Endpoint:** `GET /api/reportes/eficiencia`

---

## HU-12 — Listar todos los envíos

**Como** operador de oficina
**quiero** listar todos los envíos registrados
**para** tener una vista general de la operación.

**Criterios de aceptación**
- [ ] Devuelve todos los envíos ordenados del más reciente al más antiguo.
- [ ] Cada elemento incluye código, estado, tarifa e intentos fallidos.

**Prioridad:** Baja — **Endpoint:** `GET /api/envios`

---

### Cobertura de reglas del enunciado

| Regla del enunciado | Historia(s) que la cubren |
|---|---|
| 1. Tarifa automática por peso | HU-02 |
| 2. Máximo 3 intentos / devolución automática | HU-08, HU-09 |
| 3. Estados avanzan en una sola dirección | HU-07 |
| 4. Cada cambio incluye la ubicación | HU-06 |
| 5. Código de rastreo `ENV-YYYYMMDD-XXXX` | HU-04 |
| 6. Historial (estado, ubicación, timestamp, notas) | HU-06, HU-10 |
| 7. Descuento 5% por NIT válido | HU-03 |
