# Design

## Context

El cambio crea una solución nueva en `E:\actividades\actividad 2 urgente`; la solución existente `PaymentOrders` permanece fuera del alcance. LibraryLab tiene destino .NET 8, usa EF Core 8 y Pomelo para MySQL, y los computadores de la universidad aceptan .NET 8. Todas las rutas relativas de estos artefactos parten de esa raíz. Las especificaciones asociadas definen los contratos funcionales y la protección de los ejercicios.

## Goals / Non-Goals

**Goals:**
- Mantener una solución .NET 8 independiente con límites Clean Architecture comprobables.
- Usar EF Core 8 con un proveedor compatible con MySQL, migraciones y pruebas contra MySQL real.
- Hacer que las cinco pruebas académicas sean claramente distinguibles de la verificación normal.
- Mantener las instrucciones contra la resolución automática cerca de la solución LibraryLab y de cada ejercicio.

**Non-Goals:**
- Cambiar, migrar o reutilizar entidades de PaymentOrders.
- Exigir .NET 10 o una versión posterior a los equipos de la universidad.
- Añadir autenticación compleja, cobros, reservas, renovaciones o notificaciones.
- Implementar cualquiera de los cinco ejercicios o debilitar las pruebas que los documentan.

## Decisions

### Solución y versiones aisladas

LibraryLab es una solución .NET 8/C# 12 con su propio `global.json` y gestión central de paquetes. Usa EF Core 8 y `Pomelo.EntityFrameworkCore.MySql` 8.0.3. El archivo `global.json` declara SDK 8.0.100 y permite versiones posteriores mediante `rollForward: latestMajor`; todos los proyectos conservan el destino `net8.0`. Los computadores de la universidad pueden compilar con SDK .NET 8 y no necesitan .NET 10. La gestión de paquetes propia conserva LibraryLab separada de las decisiones de PaymentOrders.

### Límites de capas

La solución tendrá proyectos Domain, Application, Infrastructure y API, más proyectos de pruebas de dominio, API/contrato, arquitectura y persistencia según convenga. Domain contendrá entidades, estados e invariantes sin referencias a marcos web ni ORM. Application expresará casos de uso y contratos; Infrastructure implementará persistencia MySQL y detalles externos; API expondrá DTO y endpoints y será el punto de composición. Los controladores solo traducirán HTTP a llamadas de aplicación y respuestas.

### Modelo relacional para el recurso del préstamo

La API expone `ResourceId` y `ResourceKind` en el préstamo. El catálogo usa herencia TPT: `Resources` conserva identidad, tipo y disponibilidad compartidos; `Books` y `Equipment` contienen los campos propios de cada variante y referencian su identidad base. `Resources` tiene una clave alternativa `(Id, Kind)`, y `Loans` una FK compuesta `(ResourceId, ResourceKind)` a esa clave. Así MySQL exige que el recurso exista y que su tipo coincida con el préstamo. Los préstamos también tienen FK a `Users`. El esquema y sus índices se documentan en `specs/database.md`.

### Préstamo y devolución consistentes

La creación y devolución actualizarán en una transacción tanto el préstamo como la disponibilidad del recurso. La reserva de disponibilidad deberá ser segura ante solicitudes concurrentes para impedir dos préstamos activos del mismo recurso. El estado del usuario determina si puede pedir un préstamo; el estado del inventario describe la condición del libro o equipo y la disponibilidad refleja si puede prestarse en ese momento.

### Proveedor único y migraciones explícitas

Infrastructure configura únicamente MySQL local 8.0.16 o posterior, incluida la línea 8.4. La versión del servidor se configura con `Database:ServerVersion` o `Database__ServerVersion`; el valor predeterminado es `8.0.46`. La conexión viene de `ConnectionStrings:LibraryLab` o de la variable `ConnectionStrings__LibraryLab`, sin credenciales reales versionadas. Las migraciones EF Core residen en Infrastructure y crean `Users`, `Resources`, `Books`, `Equipment`, `Loans`, índices y restricciones. README documenta su aplicación mediante la herramienta estándar de EF. El inicializador de la API aplica migraciones y datos semilla solo cuando se activa explícitamente `Database__InitializeOnStartup=true`; está desactivado por defecto. La solución no incluye Docker ni configuración de contenedores.

### Pruebas académicas visibles

Las pruebas ordinarias de dominio, API y persistencia verificarán los contratos de LibraryLab y deberán pasar en la entrega inicial con MySQL disponible. Las cinco suites etiquetadas por patrón fallarán inicialmente sobre los comportamientos especificados, sin ser omitidas, debilitadas o satisfechas mediante pruebas triviales. README y `/specs/exercises.md` explicarán cómo reconocer ambos grupos y cómo ejecutar el subconjunto normal. Cada TODO y cada prueba de ejercicio llevará el aviso académico local exigido por las especificaciones. No se escribirán instrucciones algorítmicas ni una implementación equivalente de ningún patrón.

## Risks / Trade-offs

- **[Los equipos pueden tener versiones distintas del SDK .NET 8] →** Documentar SDK .NET 8 o posterior, conservar `net8.0` y C# 12, y permitir la selección de un SDK instalado compatible mediante `global.json`.
- **[El tipo de recurso del préstamo puede diferir del catálogo] →** Mantener la FK compuesta al identificador y tipo del recurso para que MySQL rechace combinaciones inválidas.
- **[Una prueba fallida puede confundirse con una regresión] →** Etiquetar las cinco suites, mantenerlas separadas de las verificaciones normales y explicar el resultado esperado en README.
- **[Las migraciones del proveedor pueden variar entre versiones] →** Fijar las versiones compatibles de EF Core 8 y Pomelo en LibraryLab, configurar la versión real del servidor y comprobar creación desde cero en MySQL local 8.0.16 o posterior antes de entregar.

## Migration Plan

No hay migración de datos desde PaymentOrders. Crear LibraryLab como solución nueva; preparar MySQL y ejecutar la migración inicial; cargar datos semilla mínimos; iniciar la API y verificar los recorridos principales; ejecutar las pruebas normales y, de manera separada, confirmar que fallan los cinco contratos académicos. Para retirar la propuesta antes de implementarla, borrar únicamente el futuro directorio LibraryLab. Una vez aplicada la migración, la vuelta atrás de cambios de esquema se realizará desde una base de desarrollo desechable o con una migración inversa revisada, no cambiando el proveedor.




