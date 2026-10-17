# Persistencia

**Database Engine: MySQL**

## Configuración

EF Core 8 usa `Pomelo.EntityFrameworkCore.MySql` 8.0.3 y MySQL 8.0.16 o posterior instalado localmente (incluido MySQL 8.4). La cadena se obtiene de `ConnectionStrings:LibraryLab` o de `ConnectionStrings__LibraryLab`. La versión del servidor se configura con `Database:ServerVersion` o `Database__ServerVersion`. No se incluyen credenciales reales ni configuración de contenedores.

## Esquema

| Tabla | Campos principales | Restricciones |
|---|---|---|
| `Users` | `Id`, `Name`, `Email`, `Status` | PK; correo único |
| `Resources` | `Id`, `Name`, `Kind`, `Status`, `IsAvailable`, `AvailabilityVersion` | PK; clave alterna `(Id, Kind)`; versión funciona como token de concurrencia |
| `Books` | `Id`, `Isbn`, `Author` | PK y FK a `Resources`; ISBN único |
| `Equipment` | `Id`, `InventoryCode`, `Type` | PK y FK a `Resources`; código único |
| `Loans` | `Id`, `UserId`, `ResourceId`, `ResourceKind`, `BorrowedAt`, `DueAt`, `ReturnedAt`, `Status` | PK; FK a `Users` y `Resources`; checks de tipo, rango de fechas y estado de devolución |

`Books` y `Equipment` comparten su identidad y disponibilidad en `Resources` usando herencia TPT. La FK compuesta `Loans(ResourceId, ResourceKind)` referencia `Resources(Id, Kind)`: el recurso debe existir y su tipo debe coincidir con el préstamo. Los índices de préstamos permiten consultar por recurso/estado y usuario/estado. Las fechas se guardan en UTC con precisión de microsegundos.

## Migraciones y datos iniciales

`dotnet ef database update --project src/LibraryLab.Infrastructure --startup-project src/LibraryLab.Api` crea el esquema en una base MySQL. Al activar `Database__InitializeOnStartup=true`, la API aplica las migraciones pendientes e inserta un usuario, un libro y un equipo de demostración si cada tabla está vacía.

La migración inicial es `20261003001904_InitialCreate`. `mysql-initial.sql` contiene su SQL generado mediante `dotnet ef migrations script`; el procedimiento habitual sigue siendo aplicar la migración con EF Core.

La operación de préstamo/devolución usa una transacción EF Core. Si otra operación modifica la disponibilidad, el token de concurrencia impide que ambas terminen como préstamo exitoso.
