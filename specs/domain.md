# Dominio

## Entidades

- **Usuario**: identificador, nombre, correo normalizado y estado activo/suspendido.
- **Recurso de biblioteca**: identificador, nombre, estado y disponibilidad. Se especializa en libro o equipo.
- **Libro**: ISBN y autor, además de los atributos comunes del recurso.
- **Equipo**: código de inventario y tipo (portátil, tableta, proyector u otro).
- **Préstamo**: usuario, recurso, tipo de recurso, fechas de préstamo/límite/devolución y estado.

## Reglas

- Un ISBN y un código de inventario son obligatorios; la persistencia impone su unicidad.
- Un usuario suspendido no puede recibir préstamos.
- Solo un recurso activo y disponible se puede prestar.
- La fecha límite debe ser futura y posterior al inicio del préstamo.
- Un préstamo activo solo se devuelve una vez. La devolución libera el recurso en la misma transacción.
- Un recurso en mantenimiento o retirado no se presta.
- Los cambios concurrentes de disponibilidad se detectan mediante `AvailabilityVersion`.

Los estados se guardan como enteros y se exponen en los DTO correspondientes. Los tiempos del servidor se generan en UTC.
