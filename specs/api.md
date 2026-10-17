# API REST

Base: `/api`. Todas las rutas reciben y responden JSON; los identificadores son GUID.

| Método y ruta | Entrada | Resultado |
|---|---|---|
| `GET /users` | — | Lista de usuarios |
| `POST /users` | `name`, `email` | Usuario creado, HTTP 201 |
| `GET /books` | — | Libros con estado y disponibilidad |
| `GET /books/{id}` | GUID | Detalle del libro |
| `POST /books` | `isbn`, `title`, `author` | Libro creado, HTTP 201 |
| `GET /equipment` | — | Equipos con estado y disponibilidad |
| `GET /equipment/{id}` | GUID | Detalle del equipo |
| `POST /equipment` | `inventoryCode`, `name`, `type` | Equipo creado, HTTP 201 |
| `GET /loans` | — | Lista de préstamos |
| `GET /loans/{id}` | GUID | Detalle del préstamo |
| `POST /loans` | `userId`, `resourceId`, `dueAt` | Préstamo creado, HTTP 201 |
| `POST /loans/{id}/return` | GUID | Préstamo devuelto |

ASP.NET Core valida los DTO de entrada. Los errores de dominio se responden con HTTP 400, los conflictos de disponibilidad con HTTP 409 y los identificadores inexistentes con HTTP 404. La API no decide por tipo de recurso dentro de los controladores.
