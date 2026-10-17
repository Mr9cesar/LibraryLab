# Spec Delta

## Purpose

Establecer MySQL como único motor de persistencia de LibraryLab y definir la configuración de base de datos necesaria para preparar y ejecutar la API educativa desde cero.

## ADDED Requirements

### Requirement: MySQL es el único motor de persistencia
Infrastructure de LibraryLab MUST utilizar exclusivamente MySQL local 8.0.16 o posterior, incluida la línea 8.4, para la persistencia de la aplicación y MUST permitir configurar la versión real del servidor. El proyecto MUST NOT configurar SQL Server, PostgreSQL, SQLite ni una base en memoria como alternativa o motor principal.

#### Scenario: Iniciar con una instancia MySQL configurada
- **WHEN** la API recibe una cadena de conexión MySQL válida
- **THEN** Infrastructure se conecta a la base configurada y persiste allí los datos de la aplicación

#### Scenario: No se configura otro proveedor de persistencia
- **WHEN** una persona revisa las dependencias y la configuración de ejecución de LibraryLab
- **THEN** MySQL es el único motor de base de datos admitido

### Requirement: La configuración de conexión no incluye credenciales reales
LibraryLab MUST obtener la cadena de conexión MySQL desde la configuración o variables de entorno y MUST NOT guardar credenciales reales en el repositorio. El nombre lógico sugerido para la base es `library_lab`.

#### Scenario: Configurar la conexión fuera del código fuente
- **WHEN** una persona proporciona el ajuste de conexión documentado mediante configuración o variables de entorno
- **THEN** la API utiliza esa conexión para acceder a MySQL sin credenciales incrustadas en archivos fuente

### Requirement: El esquema relacional representa el dominio de biblioteca
El esquema MySQL MUST incluir Users, Resources, Books, Equipment y Loans con claves primarias, columnas obligatorias, restricciones de unicidad e índices apropiados. Books y Equipment MUST referenciar su identidad compartida en Resources mediante herencia TPT. Loans MUST referenciar Users y mantener una FK compuesta `(ResourceId, ResourceKind)` a la clave alternativa `(Id, Kind)` de Resources para garantizar existencia y tipo del recurso prestado.

#### Scenario: Guardar un préstamo con referencias válidas
- **WHEN** se guarda un préstamo para un usuario existente y exactamente un libro o equipo existente del tipo indicado
- **THEN** MySQL conserva el préstamo y sus relaciones de forma consistente

#### Scenario: Rechazar referencias inválidas en un préstamo
- **WHEN** un préstamo hace referencia a un usuario inexistente o a una combinación inválida de recurso y tipo
- **THEN** la persistencia rechaza la relación y no deja una fila inconsistente del préstamo

### Requirement: La migración inicial crea el esquema desde cero
El proyecto MUST incluir una migración ORM para MySQL que cree desde una base vacía todas las tablas, relaciones, restricciones e índices necesarios. Los datos iniciales MUST ser mínimos y repetibles.

#### Scenario: Aplicar la migración inicial
- **WHEN** se ejecuta el comando de migración documentado contra una base MySQL vacía
- **THEN** se crean Users, Books, Equipment, Loans y los objetos adicionales estrictamente necesarios, con las restricciones relacionales requeridas

#### Scenario: Aplicar los datos iniciales de forma consistente
- **WHEN** el mecanismo de datos iniciales se ejecuta más de una vez
- **THEN** deja un conjunto mínimo de datos de demostración útil sin generar duplicados incompatibles

### Requirement: La documentación explica configuración y migraciones MySQL
El README MUST explicar cómo disponer de MySQL instalado localmente, configurar la conexión, aplicar migraciones e iniciar la API. MUST indicar que MySQL debe estar activo localmente. LibraryLab MUST NOT incluir Docker, Compose ni configuración de contenedores.

#### Scenario: Seguir las instrucciones de configuración
- **WHEN** una persona sigue el README desde una copia limpia y tiene MySQL disponible
- **THEN** puede configurar la conexión, crear el esquema mediante migraciones e iniciar la API

