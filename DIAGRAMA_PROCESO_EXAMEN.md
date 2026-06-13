# Diagrama de Flujo — Proceso de Resolución del Examen

**Alumno:** Luis Carlos Lima Pérez · **Carné:** 0907-23-20758

Diagrama del proceso que seguí para resolver la evaluación (no el proceso del sistema).

```mermaid
flowchart TD
    A([Inicio]) --> B[Leer y analizar el enunciado]
    B --> C[Identificar las 7 reglas de negocio]
    C --> D[Redactar las historias de usuario]
    D --> E[Diseñar modelos, endpoints y lógica]
    E --> F[Implementar la API REST en C# con SQLite]
    F --> G[Escribir pruebas unitarias xUnit]
    G --> H{¿Todas las pruebas pasan?}
    H -- No --> F
    H -- Sí --> I[Documentar: README, diagramas e informe de IA]
    I --> J[Desplegar en Render con Docker]
    J --> K{¿Despliegue funciona?}
    K -- No --> J
    K -- Sí --> L[Subir el repositorio a GitHub]
    L --> M([Fin])
```
