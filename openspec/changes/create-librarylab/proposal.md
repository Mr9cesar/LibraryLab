# Proposal

## Why

El espacio de trabajo contiene una actividad distinta, PaymentOrders. LibraryLab necesita una solución independiente en .NET 8 para enseñar Clean Architecture mediante el préstamo de libros y equipos. Se implementará directamente en `E:\actividades\actividad 2 urgente`, con MySQL obligatorio y cinco ejercicios de patrones que los estudiantes resolverán después.

## What Changes

- Añadir una solución educativa autónoma en la raíz `E:\actividades\actividad 2 urgente`, basada en .NET 8 y C#, con capas Domain, Application, Infrastructure, API y Tests, y recorridos REST para usuarios, libros, equipos y préstamos.
- Especificar reglas del dominio para disponibilidad, creación y devolución de préstamos, y su persistencia exclusiva en MySQL, incluidas configuración, migración inicial y datos semilla.
- Preparar exactamente cinco ejercicios sin implementar: dos creacionales (Factory Method y Builder) y tres estructurales (Adapter, Decorator y Facade). Sus pruebas identificables quedan intencionalmente en RED; las pruebas normales de la API y el dominio deben pasar.
- Incluir `AGENTS.md`, `README.md`, `specs/` y `openspec/` en la raíz de LibraryLab para explicar ejecución, MySQL local y los contratos esperados de los ejercicios sin revelar soluciones. Mantener intactos PaymentOrders y sus instrucciones.

## Capabilities

### New Capabilities
- `librarylab-backend`: API y reglas de dominio para usuarios, libros, equipos, disponibilidad y préstamos.
- `librarylab-mysql-persistence`: almacenamiento exclusivamente MySQL, migraciones, configuración y datos iniciales.
- `librarylab-pattern-exercises`: cinco ejercicios académicos protegidos, con pruebas RED etiquetadas y documentación de sus comportamientos esperados.

### Modified Capabilities

Ninguna.

## Impact

El alcance de implementación se limita a la raíz `E:\actividades\actividad 2 urgente`, sus proyectos .NET 8, dependencias de persistencia MySQL, pruebas y documentación. No modifica la solución PaymentOrders existente. La entrega inicial debe poder compilar y ejecutar la API con el SDK .NET 8 y MySQL configurado localmente, sin contenedores; solo las pruebas asociadas a los cinco ejercicios quedan fallidas deliberadamente.


