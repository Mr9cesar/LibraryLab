# Tasks

## 1. Solución aislada y límites arquitectónicos

- [x] 1.1 Crear LibraryLab directamente en `E:\actividades\actividad 2 urgente` con `LibraryLab.sln`, destino `net8.0`, un `global.json` compatible con SDK .NET 8 y gestión local de paquetes; añadir los proyectos Domain, Application, Infrastructure, API y Tests. Verificar que todos apuntan a `net8.0` y compilan con el SDK disponible.
- [x] 1.2 Añadir las instrucciones académicas a `AGENTS.md` y documentar propósito, estructura y límites en `README.md` y `specs/architecture.md`; verificar que una lectura de cada archivo deja claro que los ejercicios no se resuelven automáticamente.

## 2. Dominio y casos de uso

- [x] 2.1 Modelar usuarios, libros, equipos y préstamos con los campos y estados requeridos, validaciones e invariantes de disponibilidad; añadir pruebas normales de dominio para datos inválidos, préstamos y devoluciones, y escribir `specs/domain.md`; verificar que esas pruebas pasan.
- [x] 2.2 Definir los contratos de aplicación y casos de uso para registrar y consultar usuarios, libros y equipos, consultar disponibilidad, crear préstamos y devolverlos; probar resultados válidos y errores con dependencias controladas que no oculten las reglas; verificar que los casos de uso mantienen los ejercicios académicos sin resolver.

## 3. Persistencia MySQL

- [x] 3.1 Configurar EF Core 8 y `Pomelo.EntityFrameworkCore.MySql` 8.0.3, cadena `ConnectionStrings:LibraryLab` y mapeo relacional de Users, Resources, Books, Equipment y Loans; escribir `specs/database.md` con tablas, columnas, relaciones, restricciones, índices y conexión; verificar que la configuración solo ofrece MySQL y no incluye credenciales reales.
- [x] 3.2 Añadir la migración inicial y datos semilla mínimos y repetibles; verificar contra una base MySQL local 8.0.16 o posterior vacía que la migración crea el esquema completo y que las FKs y la relación exclusiva entre préstamo y recurso rechazan datos incompatibles.
- [x] 3.3 Implementar persistencia de consultas, alta, préstamo y devolución con transacción y protección frente a préstamos concurrentes duplicados; añadir pruebas de integración contra MySQL real y verificar el cambio consistente del préstamo y la disponibilidad.
- [x] 3.4 Documentar configuración de MySQL instalado localmente, creación de base, variables de conexión y comandos estándar de migración en README, sin Docker ni configuración de contenedores. Verificar los comandos documentados en un entorno limpio con MySQL.

## 4. API REST

- [x] 4.1 Exponer DTO y rutas para listar/crear usuarios, libros y equipos, consultar detalle de libro/equipo, listar/consultar/crear préstamos y devolverlos; escribir `specs/api.md`; verificar contratos para éxito, validación, recursos inexistentes y disponibilidad mediante pruebas de API.
- [x] 4.2 Completar la composición de dependencias y configuración de arranque para MySQL sin decisiones de negocio por tipo de recurso en controladores; documentar inicio de API y rutas en README, y verificar el recorrido de alta, consulta, préstamo y devolución con MySQL.

## 5. Ejercicios académicos en RED

- [x] 5.1 Preparar los ejercicios Factory Method y Builder con contratos, `TODO(STUDENT)` y suites `FactoryMethodExerciseTests` y `BuilderExerciseTests`; documentar problema, comportamiento esperado y prueba asociada en `specs/exercises.md`; verificar que sus pruebas fallan inicialmente y que no contienen implementación, pseudocódigo ni pasos de solución.
- [x] 5.2 Preparar los ejercicios Adapter, Decorator y Facade con contratos, `TODO(STUDENT)` y suites `AdapterExerciseTests`, `DecoratorExerciseTests` y `FacadeExerciseTests`; completar la lista exacta en `specs/exercises.md`; verificar que cada suite falla inicialmente por su contrato pendiente y no por fallas de configuración.
- [x] 5.3 Distribuir avisos académicos en README, cada `TODO(STUDENT)` y cada prueba de patrón, sin repetir texto innecesario; documentar cómo distinguir y ejecutar pruebas normales y de ejercicios; verificar que no haya pruebas omitidas, ignoradas, debilitadas ni mocks triviales para hacerlas pasar.

## 6. Integración final

- [x] 6.1 Ejecutar restauración y compilación de la solución LibraryLab con .NET 8, aplicar migraciones y arrancar la API con MySQL; verificar las rutas principales y la dirección de dependencias Clean Architecture.
- [x] 6.2 Ejecutar el grupo normal de pruebas y el grupo de ejercicios por separado; confirmar que las pruebas normales pasan y que solo los cinco contratos académicos fallan intencionalmente.
- [x] 6.3 Revisar documentación, dependencias, código y pruebas para confirmar que MySQL es el único proveedor, existe la migración inicial, los cinco ejercicios son los únicos patrones académicos y ninguno está implementado; verificar que PaymentOrders y sus instrucciones no cambiaron.

## Evidencia de ejecución

Las 16 tareas están completadas. Se verificaron la aplicación y sus restricciones contra MySQL local 8.0.46, además de ejecutar por separado las pruebas normales y los cinco ejercicios académicos pendientes.

| Comprobación realizada | Resultado |
|---|---|
| `dotnet build LibraryLab.sln --no-restore` | Compilación correcta, cero advertencias y cero errores; destino `net8.0` en todos los proyectos. En este equipo se seleccionó SDK 10 mediante la política de `global.json`; el proyecto admite SDK .NET 8. |
| Grupo normal, con filtro `FullyQualifiedName!~LibraryLab.Patterns.Tests` | 34 pruebas aprobadas: Domain 7, Application 5, API 18, Architecture 2 y modelo Infrastructure 2; ninguna omitida. |
| Grupo `LibraryLab.Patterns.Tests` por separado | Cinco fallos previstos, uno por Factory Method, Builder, Adapter, Decorator y Facade. Todos lanzan `NotImplementedException` con `TODO(STUDENT)`; ninguna prueba omitida. |
| `dotnet ef migrations script` | Generación estándar correcta de `specs/mysql-initial.sql` sin conexión a un servidor: cinco tablas del dominio, FK compuesta, restricciones e índices de MySQL. |
| `dotnet ef database update` contra MySQL local 8.0.46 | La migración `20261003001904_InitialCreate` creó desde cero `library_lab` con las cinco tablas del dominio, índices, restricciones y claves foráneas. |
| Inicialización y reinicio de la API | Se insertó un usuario, un libro y un equipo de demostración. El siguiente arranque no duplicó los datos semilla. |
| Restricciones verificadas directamente en MySQL | Recurso de tipo incompatible y usuario inexistente rechazados con error 1452; fecha límite igual a fecha de préstamo y estado devuelto sin fecha rechazados con error 3819. Ningún préstamo inválido quedó guardado. |
| `scripts/Verify-LibraryLab.ps1` con MySQL real | Ejecución correcta, salida 0, 76 aserciones y 38 solicitudes HTTP: altas, listas, detalles de ambos recursos, respuestas 400/404/409, préstamos y devoluciones. |
| Préstamos concurrentes contra MySQL real | Dos solicitudes simultáneas para el mismo recurso produjeron un 201 y un 409, con un único préstamo activo. La devolución dejó disponible el recurso. |
| ISBN duplicado contra MySQL real | El alta duplicada se rechazó como conflicto de aplicación a partir del error MySQL 1062 y no dejó un recurso huérfano. |
| Revisión de los ejercicios y dependencias | Los cinco contratos siguen sin implementación; MySQL es el único proveedor configurado y no se incluyeron contenedores. |
| Arranque de la API | Escucha en `localhost:5109` en Development; Swagger devuelve HTTP 200 y expone el esquema de rutas. Los recorridos de integración anteriores accedieron a MySQL local. |

La migración inicial generada es `20261003001904_InitialCreate`. La versión del servidor se configura mediante `Database:ServerVersion` o `Database__ServerVersion`, con valor predeterminado `8.0.46` y posibilidad de indicar `8.4.0`.

La auditoría de rutas de escritura confirma que implementación, pruebas, migración y documentación de LibraryLab se modificaron únicamente dentro de `actividad 2 urgente`. En la raíz del proyecto padre solo se trasladó el cambio OpenSpec `create-librarylab`, manteniendo su configuración y los otros cambios. No se editaron PaymentOrders ni el `AGENTS.md` padre. No hay un repositorio Git disponible en `E:\actividades`, por lo que esta revisión se basa en los archivos y las operaciones realizadas, sin comparación de diferencias de Git.

La conexión real se guardó en los secretos de usuario locales de .NET para Development, fuera del repositorio. Los artefactos no contienen esa contraseña. El cambio OpenSpec se conserva visible en `openspec/changes/create-librarylab` y no se archivó.



