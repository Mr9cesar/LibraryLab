# Arquitectura

## Objetivo y límites

LibraryLab administra miembros, libros, equipos y préstamos mediante una API ASP.NET Core 8. La separación en proyectos permite estudiar el dominio sin acoplarlo al transporte HTTP ni a EF Core.

## Dirección de dependencias

`Api -> Application -> Domain <- Infrastructure`

- **Domain** contiene entidades, estados e invariantes. No referencia frameworks web ni persistencia.
- **Application** define casos de uso, contratos de repositorio y resultados de lectura. Solo depende de Domain.
- **Infrastructure** implementa repositorios, transacciones y mapeos EF Core/MySQL para los contratos de Application.
- **Api** convierte DTO HTTP en comandos de Application, registra dependencias y traduce errores a respuestas HTTP.

La API delega reglas de préstamo y devolución a Application y Domain. El control de concurrencia se apoya en una versión de disponibilidad persistida como token de concurrencia y en una transacción.

## Proyectos

`src/LibraryLab.Domain`, `src/LibraryLab.Application`, `src/LibraryLab.Infrastructure` y `src/LibraryLab.Api` forman la solución. Los proyectos `tests/LibraryLab.*.Tests` separan pruebas normales, de arquitectura y ejercicios académicos.

## Límite pedagógico

Los ejercicios Factory Method, Builder, Adapter, Decorator y Facade son contratos pendientes. El producto base usa flujos ordinarios de dominio y persistencia; no contiene la solución de esos ejercicios. Las suites académicas empiezan en RED por sus `TODO(STUDENT)`.
