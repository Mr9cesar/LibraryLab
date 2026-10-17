# Spec Delta

## Purpose

Definir el comportamiento de la API y del dominio de una biblioteca educativa independiente que presta libros y equipos tecnológicos a usuarios registrados.

## ADDED Requirements

### Requirement: Solución independiente con Clean Architecture
LibraryLab MUST entregarse como una solución independiente con raíz `E:\actividades\actividad 2 urgente`, destino `net8.0`, C# 12 y proyectos separados Domain, Application, Infrastructure, API y Tests. Domain MUST permanecer independiente de los marcos web y de persistencia, y los controladores HTTP MUST delegar el comportamiento de negocio a los casos de uso de Application.

#### Scenario: Los límites de los proyectos se pueden verificar
- **WHEN** las comprobaciones de arquitectura inspeccionan los proyectos de LibraryLab
- **THEN** las dependencias siguen API → Application → Domain ← Infrastructure y Domain no depende de API, Infrastructure, ASP.NET Core ni del ORM

### Requirement: Catálogo de usuarios, libros y equipos
La API MUST permitir registrar y listar usuarios, libros y equipos, además de consultar un libro o equipo individual. Los usuarios MUST exponer identificador, nombre, correo electrónico y estado activo. Los libros MUST exponer identificador, ISBN, título, autor, estado y disponibilidad. Los equipos MUST exponer identificador, código de inventario, nombre, tipo, estado y disponibilidad.

#### Scenario: Registrar y listar datos del catálogo
- **WHEN** se envía un registro válido de usuario, libro o equipo y se consulta la colección correspondiente
- **THEN** la API devuelve el registro guardado con los campos requeridos del dominio

#### Scenario: Consultar un recurso del catálogo
- **WHEN** un cliente solicita un libro o equipo existente por su identificador
- **THEN** la API devuelve sus detalles y disponibilidad actual

#### Scenario: Rechazar datos inválidos o duplicados
- **WHEN** un usuario, libro o equipo enviado incumple campos obligatorios o una regla de unicidad
- **THEN** la API informa un resultado de validación o conflicto y no guarda el registro

### Requirement: Creación de préstamos y disponibilidad
La API MUST permitir que un usuario activo solicite un libro o equipo disponible mediante `POST /api/loans`. Cada préstamo MUST identificar al usuario, el recurso, el tipo de recurso, la fecha del préstamo, la fecha límite, la fecha de devolución que admite valores nulos y el estado del préstamo. El sistema MUST impedir que se preste un recurso no disponible y MUST reflejar que el recurso deja de estar disponible después de un préstamo exitoso.

#### Scenario: Crear un préstamo para un libro o equipo disponible
- **WHEN** un usuario activo solicita un recurso disponible con una fecha límite válida
- **THEN** la API crea un préstamo activo e informa que el recurso ya no está disponible

#### Scenario: Rechazar un préstamo para un usuario inactivo o un recurso no disponible
- **WHEN** una solicitud de préstamo indica un usuario inactivo o inexistente, un recurso inexistente o un recurso no disponible
- **THEN** la API informa el resultado aplicable de recurso no encontrado, validación o conflicto, y no crea el préstamo

#### Scenario: Listar préstamos y consultar uno
- **WHEN** un cliente solicita `GET /api/loans` o `GET /api/loans/{id}`
- **THEN** la API devuelve los datos guardados del préstamo, incluidos tipo de recurso, fechas y estado

### Requirement: Devolución de préstamos
La API MUST permitir devolver un préstamo activo mediante `POST /api/loans/{id}/return`. Una devolución exitosa MUST registrar la fecha de devolución, cambiar el préstamo al estado devuelto y dejar disponible nuevamente el recurso asociado.

#### Scenario: Devolver un préstamo activo
- **WHEN** un cliente devuelve un préstamo activo existente
- **THEN** la API registra la devolución y el recurso asociado vuelve a estar disponible

#### Scenario: Devolver un préstamo inexistente o ya devuelto
- **WHEN** un cliente intenta devolver un préstamo inexistente o que ya no está activo
- **THEN** la API informa que no existe o que hay un conflicto, y no genera una segunda transición de devolución

### Requirement: Rutas REST disponibles para los clientes
La API MUST exponer rutas `GET` y `POST` para usuarios, libros y equipos; `GET /api/books/{id}` y `GET /api/equipment/{id}`; y las rutas de consulta, detalle, creación y devolución de préstamos descritas anteriormente. Esta actividad educativa no requiere autenticación compleja.

#### Scenario: Consultar y utilizar las rutas principales
- **WHEN** un cliente revisa la documentación de la API o llama a cada ruta principal
- **THEN** encuentra disponibles las rutas y sus contratos de solicitud y respuesta sin necesitar autenticación compleja


