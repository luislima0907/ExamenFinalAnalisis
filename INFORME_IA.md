# Informe de Utilización de IA

**Proyecto:** Envíos Rápidos GT — API REST
**Alumno:** Luis Carlos Lima Pérez · **Carné:** 0907-23-20758
**Herramienta de IA utilizada:** Claude (asistente de programación)

Este informe documenta cómo se utilizó la inteligencia artificial como apoyo para
resolver el examen final, los *prompts* enviados, la reflexión sobre los resultados
y las correcciones que fue necesario aplicar.

---

## 1. Objetivo del uso de IA

Utilicé la IA como **par de programación** para acelerar la construcción del prototipo,
manteniendo siempre el control sobre las decisiones de diseño y validando cada
resultado con pruebas. La IA **no** sustituyó el análisis: las reglas de negocio,
las historias de usuario y la arquitectura se derivaron del enunciado del examen.

---

## 2. Prompts enviados (resumen)

A continuación los prompts más relevantes y su propósito:

### Prompt 1 — Comprensión del problema
> "Tengo un enunciado para una empresa de paquetería (Envíos Rápidos GT) con 7 reglas
> de negocio: tarifa por peso, máximo 3 intentos con devolución automática, estados
> unidireccionales, ubicación en cada cambio, código de rastreo ENV-YYYYMMDD-XXXX,
> historial de estados y 5% de descuento por NIT. Necesito una API REST en C# con SQLite."

**Propósito:** que la IA estructurara el dominio (modelos, endpoints y servicios) a partir de las reglas.

### Prompt 2 — Modelado y lógica de negocio
> "Diseña los modelos (Cliente, Envío, HistorialEstado) y separa la lógica de negocio
> en clases puras y testeables: cálculo de tarifa, validación de NIT, generación de
> código de rastreo y la máquina de estados."

**Propósito:** obtener una separación limpia entre la lógica (testeable) y el acceso a datos.

### Prompt 3 — Controladores y manejo de errores
> "Crea los controladores REST con al menos 7 endpoints y un manejo centralizado de
> errores que devuelva 400 para reglas de negocio y 404 para recursos no encontrados."

### Prompt 4 — Pruebas unitarias
> "Escribe pruebas con xUnit que cubran: los rangos de tarifa (incluyendo los límites
> 1, 5 y 10 kg), el descuento por NIT, el formato del código, todas las transiciones
> válidas e inválidas, y el paso automático a EnDevolucion al tercer intento."

### Prompt 5 — Documentación y despliegue
> "Genera las historias de usuario con criterios de aceptación medibles, los diagramas
> (flujo, estados, secuencia), el README con instrucciones y la configuración de Docker
> para desplegar en Render."

---

## 3. Reflexión

**Lo que funcionó bien:**
- La IA fue muy eficaz para **generar código repetitivo** (DTOs, mapeos, configuración
  de EF Core) y para proponer una **estructura por capas** clara (Models / Data / Services / Controllers).
- Ayudó a no olvidar **casos límite** en las pruebas: los valores exactos de 1, 5 y 10 kg
  (frontera de los rangos de tarifa) y el redondeo del descuento.
- Aceleró la redacción de documentación (historias, diagramas Mermaid y README).

**Lo que requirió criterio propio:**
- La **interpretación del flujo de estados** del enunciado era ambigua. Decidí el modelo
  `Registrado → EnTransito → EnReparto → Entregado` y la rama de devolución
  `EnReparto → EnDevolucion → Devuelto`, y lo documenté explícitamente.
- Verifiqué que cada **regla de negocio** quedara cubierta por al menos una historia y
  una prueba; la IA tiende a generar mucho código, pero la *cobertura real* hay que comprobarla.

---

## 4. Correcciones realizadas

Durante el desarrollo fue necesario corregir varios resultados de la IA:

1. **Versión del framework.** El código se generó inicialmente para **.NET 9**, pero el
   entorno solo tenía el **SDK 8.0**. Se ajustaron los `.csproj` y el `Dockerfile` a
   **net8.0** y se fijaron las versiones de los paquetes (EF Core 8.0.11, etc.).

2. **Base de datos en pruebas de integración.** Las primeras pruebas con
   `WebApplicationFactory` + EF Core InMemory **no compartían datos entre peticiones**
   (los clientes creados "desaparecían" al registrar el envío → error 400). Se corrigió
   usando un **`InMemoryDatabaseRoot` compartido** por instancia de fábrica.

3. **Decimales en SQLite.** SQLite no tiene tipo `decimal` nativo; se añadió una
   conversión explícita en el `DbContext` para evitar advertencias y pérdida de precisión.

4. **Serialización de enums.** Por defecto los estados se serializaban como números
   (0,1,2…). Se configuró `JsonStringEnumConverter` para devolver los nombres
   ("Registrado", "EnReparto", …) y hacer la API más legible.

5. **Limpieza del proyecto base.** El proyecto traía pruebas E2E con **Selenium** que
   dependían de un navegador y no aplicaban al alcance del examen; se eliminaron y se
   reemplazaron por pruebas unitarias y de integración pertinentes.

---

## 5. Conclusión

La IA fue una herramienta de productividad valiosa, especialmente para el *andamiaje*
del proyecto y la documentación. Sin embargo, **el resultado final dependió de la
validación humana**: ajustar versiones, corregir el comportamiento de las pruebas,
interpretar correctamente las reglas del enunciado y confirmar —ejecutando los 60 tests
y probando los endpoints reales— que todo funcionara según lo pedido.
