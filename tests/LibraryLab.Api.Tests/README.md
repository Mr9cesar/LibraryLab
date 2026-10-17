# Pruebas HTTP de la API

`RequestValidationTests` conserva la prueba inicial de validación. `LibraryHttpWorkflowTests` usa el servidor de pruebas de ASP.NET Core para ejecutar solicitudes HTTP contra los controladores, DTO, middleware, casos de uso y entidades reales.

Cada prueba tiene repositorios independientes que guardan las entidades producidas por la aplicación. La unidad de trabajo registra las llamadas de guardado y confirmación; no simula las garantías de una transacción SQL. La inicialización automática de MySQL se desactiva únicamente en este servidor de pruebas.

Esta suite cubre registros, consultas, préstamos de libros y equipos, devoluciones, disponibilidad, respuestas 400/404/409 y correo duplicado. **No verifica persistencia MySQL, claves foráneas, rollback SQL ni concurrencia entre conexiones.** Esas comprobaciones requieren una base MySQL real.
