# Spec Delta

## Purpose

Incluir cinco ejercicios protegidos y comprobables de patrones de diseño en LibraryLab para que los estudiantes los implementen después, mientras el resto de la API y el dominio siguen siendo utilizables.

## ADDED Requirements

### Requirement: Se incluyen exactamente cinco ejercicios de patrones
El proyecto educativo MUST incluir exactamente dos ejercicios creacionales—Factory Method y Builder—y exactamente tres estructurales—Adapter, Decorator y Facade. El inventario MUST NOT añadir otro ejercicio de patrones ni entregar una implementación equivalente a cualquiera de estos cinco.

#### Scenario: Revisar el inventario de ejercicios
- **WHEN** se revisan la documentación y las pruebas etiquetadas de los ejercicios
- **THEN** aparecen únicamente Factory Method, Builder, Adapter, Decorator y Facade, con dos clasificados como creacionales y tres como estructurales

### Requirement: El ejercicio Factory Method queda a cargo del estudiante
El ejercicio Factory Method MUST describir el problema de producir un préstamo que represente el comando recibido y el tipo de recurso solicitado, sin proporcionar una solución. Su prueba asociada MUST poder identificarse como `FactoryMethodExerciseTests` y MUST fallar en la entrega inicial porque el ejercicio continúa incompleto.

#### Scenario: Verificar el contrato de creación del préstamo
- **WHEN** se evalúa el contrato Factory Method con un comando de préstamo y un tipo de recurso
- **THEN** la prueba asociada comprueba que el préstamo conserva el usuario, el recurso y el tipo solicitado, mientras el ejercicio inicial permanece fallido

### Requirement: El ejercicio Builder queda a cargo del estudiante
El ejercicio Builder MUST describir la dificultad de reunir los datos de un recibo de préstamo y el comportamiento observable esperado, sin proporcionar una solución. Su prueba asociada MUST poder identificarse como `BuilderExerciseTests` y MUST fallar en la entrega inicial porque el ejercicio continúa incompleto.

#### Scenario: Verificar el contrato del recibo de préstamo
- **WHEN** se evalúa el contrato Builder con el identificador del préstamo, el recurso y la fecha límite
- **THEN** la prueba asociada comprueba que el recibo conserva esos datos, mientras el ejercicio inicial permanece fallido

### Requirement: El ejercicio Adapter queda a cargo del estudiante
El ejercicio Adapter MUST describir la incompatibilidad entre un registro de catálogo antiguo y el contrato de importación de LibraryLab, sin proporcionar una solución. Su prueba asociada MUST poder identificarse como `AdapterExerciseTests` y MUST fallar en la entrega inicial porque el ejercicio continúa incompleto.

#### Scenario: Verificar la compatibilidad del catálogo antiguo
- **WHEN** se evalúa el contrato Adapter con un registro del catálogo antiguo
- **THEN** la prueba asociada observa un resultado de importación con identificador y con el código, nombre y categoría del registro recibido, mientras el ejercicio inicial permanece fallido

### Requirement: El ejercicio Decorator queda a cargo del estudiante
El ejercicio Decorator MUST describir la necesidad de agregar auditoría al envío de un aviso de préstamo y conservar el aviso entregado al emisor, sin proporcionar una solución. Su prueba asociada MUST poder identificarse como `DecoratorExerciseTests` y MUST fallar en la entrega inicial porque el ejercicio continúa incompleto.

#### Scenario: Verificar que la responsabilidad adicional conserva el resultado base
- **WHEN** se evalúa el contrato Decorator con un aviso de préstamo
- **THEN** la prueba asociada observa que el emisor y el registro de auditoría reciben el mismo aviso, mientras el ejercicio inicial permanece fallido

### Requirement: El ejercicio Facade queda a cargo del estudiante
El ejercicio Facade MUST describir la complejidad que enfrenta un cliente al consultar datos del usuario, sus préstamos y los recursos relacionados para obtener un resumen del miembro, sin proporcionar una solución. Su prueba asociada MUST poder identificarse como `FacadeExerciseTests` y MUST fallar en la entrega inicial porque el ejercicio continúa incompleto.

#### Scenario: Verificar el resumen del miembro
- **WHEN** un cliente utiliza el contrato Facade para consultar un miembro
- **THEN** la prueba asociada observa el nombre del usuario y los recibos de sus préstamos activos mediante ese contrato, mientras el ejercicio inicial permanece fallido

### Requirement: La protección académica está distribuida sin revelar soluciones
`AGENTS.md`, `README.md`, `specs/exercises.md`, las especificaciones de patrones y cada TODO y prueba asociada MUST indicar que los cinco ejercicios pertenecen al estudiante. Las instrucciones MUST identificar el problema, el comportamiento esperado y la prueba relacionada, pero MUST NOT incluir implementación, pseudocódigo, algoritmos paso a paso ni una solución equivalente. Las pruebas MUST NOT omitirse, ignorarse, debilitarse, eliminarse ni satisfacerse con aserciones triviales o simulaciones creadas solo para hacerlas pasar.

#### Scenario: Revisar un ejercicio por separado
- **WHEN** un agente o estudiante abre un TODO o una prueba de patrón sin haber leído antes las instrucciones generales del repositorio
- **THEN** el aviso académico local deja claro que el ejercicio no debe resolverse automáticamente e identifica su prueba

### Requirement: Las verificaciones normales son independientes de las pruebas fallidas de ejercicios
La entrega MUST separar las pruebas normales de dominio, API y persistencia razonable de las cinco pruebas de ejercicios de patrones. Las pruebas normales MUST pasar en la entrega inicial; las cinco pruebas de ejercicios MUST ser el único grupo que falle intencionalmente. La salida de las pruebas y el README MUST explicar esta diferencia.

#### Scenario: Ejecutar las pruebas normales antes de implementar los ejercicios
- **WHEN** una persona ejecuta el grupo de pruebas normales en la entrega inicial
- **THEN** esas pruebas pasan y cubren el comportamiento principal del dominio, la API y la persistencia

#### Scenario: Ejecutar las pruebas de ejercicios antes de su implementación
- **WHEN** una persona ejecuta el grupo etiquetado de ejercicios de patrones en la entrega inicial
- **THEN** cada uno de los cinco contratos aparece visiblemente fallido porque está pendiente, sin desactivar ni simular ninguna prueba

