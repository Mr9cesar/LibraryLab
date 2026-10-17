# LibraryLab

API educativa de biblioteca para registrar usuarios, libros y equipos, consultar disponibilidad y gestionar préstamos y devoluciones. La solución apunta a **.NET 8** para ejecutarse en los equipos de la universidad.

## Estructura

- `src/LibraryLab.Domain`: entidades, estados e invariantes de biblioteca.
- `src/LibraryLab.Application`: casos de uso y contratos de persistencia.
- `src/LibraryLab.Infrastructure`: EF Core 8, mapeos y persistencia MySQL.
- `src/LibraryLab.Api`: controladores REST, DTO de entrada y composición.
- `tests`: pruebas de dominio, aplicación, API, arquitectura, persistencia y ejercicios.
- `specs`: especificaciones funcionales y técnicas.

Las dependencias apuntan hacia el dominio: `Api -> Application -> Domain <- Infrastructure`. Los controladores no contienen reglas de préstamo.

## Requisitos

- SDK de .NET 8.
- MySQL 8.0.16 o posterior instalado localmente (también compatible con MySQL 8.4).
- NuGet disponible para restaurar los paquetes al preparar el equipo.

No se usan SQL Server, PostgreSQL, SQLite ni persistencia InMemory.

## Preparar MySQL local

Instala e inicia MySQL localmente y crea una base de datos llamada `library_lab` y un usuario con permisos sobre esa base. El proyecto no incluye contenedores ni herramientas de orquestación.

Desde MySQL Workbench o el cliente MySQL, con una cuenta administradora, puedes preparar una cuenta propia de desarrollo. Sustituye `CAMBIAR` por tu contraseña local:

```sql
CREATE DATABASE IF NOT EXISTS library_lab CHARACTER SET utf8mb4;
CREATE USER 'library_lab'@'localhost' IDENTIFIED BY 'CAMBIAR';
GRANT ALL PRIVILEGES ON library_lab.* TO 'library_lab'@'localhost';
```

## Restaurar, migrar e iniciar

Desde la raíz de esta carpeta:

```powershell
dotnet restore
dotnet tool restore
```

Configura la conexión mediante variables de entorno de .NET. Ejemplo para PowerShell con credenciales locales de desarrollo:

```powershell
$env:ConnectionStrings__LibraryLab = "Server=localhost;Port=3306;Database=library_lab;User=library_lab;Password=CAMBIAR;"
$env:Database__ServerVersion = "8.0.46"
```

Usa la versión que muestra `SELECT VERSION()` en tu servidor. Mantén las credenciales en variables de entorno o secretos de desarrollo, fuera del código y de Git.

Como alternativa para este equipo, en entorno Development puedes guardar la conexión con el administrador de secretos de .NET:

```powershell
dotnet user-secrets set "ConnectionStrings:LibraryLab" "Server=localhost;Port=3306;Database=library_lab;User=library_lab;Password=CAMBIAR;" --project src/LibraryLab.Api
```

El proyecto API ya tiene `UserSecretsId`. Los secretos se guardan en el perfil del usuario; no viajan al copiar el proyecto a otro computador. Una variable `ConnectionStrings__LibraryLab` tiene prioridad sobre esos secretos.

Aplica la migración inicial:

```powershell
dotnet ef database update --project src/LibraryLab.Infrastructure --startup-project src/LibraryLab.Api
```

Para insertar los datos educativos de ejemplo al iniciar la API, activa la inicialización:

```powershell
$env:Database__InitializeOnStartup = "true"
dotnet run --project src/LibraryLab.Api
```

La inicialización aplica migraciones pendientes y agrega un usuario, un libro y un equipo de demostración solo si sus tablas están vacías. Swagger queda disponible en `/swagger` en entorno Development.

Con el perfil HTTP predeterminado, abre `http://localhost:5109/swagger`. Los tipos de equipo son `1` portátil, `2` tableta, `3` proyector y `4` otro. Una fecha límite de préstamo se envía en formato ISO 8601, por ejemplo con sufijo `Z` para UTC.

Para crear otra migración después de cambiar el modelo:

```powershell
dotnet ef migrations add NombreDelCambio --project src/LibraryLab.Infrastructure --startup-project src/LibraryLab.Api --output-dir Persistence/Migrations
```

## Endpoints

| Método | Ruta | Uso |
|---|---|---|
| GET, POST | `/api/users` | Consultar y registrar usuarios |
| GET, POST | `/api/books` | Consultar y registrar libros |
| GET | `/api/books/{id}` | Consultar libro y disponibilidad |
| GET, POST | `/api/equipment` | Consultar y registrar equipos |
| GET | `/api/equipment/{id}` | Consultar equipo y disponibilidad |
| GET, POST | `/api/loans` | Consultar préstamos y prestar un recurso |
| GET | `/api/loans/{id}` | Consultar un préstamo |
| POST | `/api/loans/{id}/return` | Devolver un préstamo activo |

El préstamo recibe `userId`, `resourceId` y `dueAt`. El recurso puede ser libro o equipo; el dominio conserva su tipo. La disponibilidad se protege con una transacción y control de concurrencia.

## Pruebas

```powershell
dotnet test tests/LibraryLab.Domain.Tests
dotnet test tests/LibraryLab.Application.Tests
dotnet test tests/LibraryLab.Api.Tests
dotnet test tests/LibraryLab.Architecture.Tests
dotnet test tests/LibraryLab.Infrastructure.Tests
```

Las pruebas anteriores cubren reglas normales y el modelo relacional. Las cinco pruebas en `LibraryLab.Patterns.Tests` deben fallar mientras el estudiante no resuelva los ejercicios:

```powershell
dotnet test tests/LibraryLab.Patterns.Tests
```

No se omiten, ignoran ni debilitan. Consulta `specs/exercises.md` para ver qué comportamiento observa cada una sin instrucciones de solución.

Con la API iniciada y conectada a MySQL, ejecuta la verificación de integración real:

```powershell
powershell -NoProfile -File .\scripts\Verify-LibraryLab.ps1 -ApiBaseUrl http://localhost:5109
```

Comprueba registros, consultas, préstamos, devoluciones, conflictos y dos solicitudes concurrentes para el mismo recurso. Crea y conserva registros con prefijo `VERIFY`; úsalo en una base de desarrollo. Las pruebas HTTP de xUnit usan repositorios de prueba y no sustituyen esta comprobación de MySQL.

## Actividad académica

Factory Method, Builder, Adapter, Decorator y Facade están intencionalmente pendientes. `AGENTS.md`, los `TODO(STUDENT)` y sus pruebas preservan esos límites. No completes esos ejercicios automáticamente.

## Especificaciones

- [Arquitectura](specs/architecture.md)
- [Dominio](specs/domain.md)
- [API](specs/api.md)
- [MySQL](specs/database.md)
- [Ejercicios](specs/exercises.md)
- [Propuesta OpenSpec](openspec/changes/create-librarylab/proposal.md)
- [Tareas OpenSpec](openspec/changes/create-librarylab/tasks.md)
