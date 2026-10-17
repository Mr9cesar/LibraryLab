# Ejercicios de patrones

Los cinco ejercicios están pendientes deliberadamente. Cada clase tiene `TODO(STUDENT)` y lanza `NotImplementedException`; su prueba de comportamiento está en `LibraryLab.Patterns.Tests`. No hay soluciones, pseudocódigo ni pasos de implementación aquí. Las pruebas no tienen `Skip`, `Ignore` ni aserciones debilitadas.

## Creacionales

| Patrón | Problema del escenario | Comportamiento esperado | Prueba |
|---|---|---|---|
| Factory Method | El ejercicio debe producir un préstamo que represente el comando y el tipo de recurso solicitados. | Conserva usuario, recurso y tipo de recurso en el préstamo resultante. | `FactoryMethodExerciseTests.Creates_a_loan_with_the_requested_resource_type` |
| Builder | El recibo de préstamo requiere los datos que serán presentados después del registro. | Conserva identificador del préstamo, recurso y fecha límite recibidos. | `BuilderExerciseTests.Builds_a_receipt_that_preserves_loan_details` |

## Estructurales

| Patrón | Problema del escenario | Comportamiento esperado | Prueba |
|---|---|---|---|
| Adapter | El catálogo antiguo entrega registros con un contrato distinto al de importación de LibraryLab. | El resultado conserva código, nombre y categoría, y cuenta con identificador. | `AdapterExerciseTests.Converts_a_legacy_catalog_record_to_the_library_import_contract` |
| Decorator | El envío de avisos de préstamo requiere registro de auditoría adicional. | Entrega el mismo aviso al emisor y al registro de auditoría. | `DecoratorExerciseTests.Sends_the_loan_notice_to_the_configured_sender` |
| Facade | Consultar el resumen de un miembro requiere datos del miembro, de sus préstamos y de los recursos relacionados. | Devuelve nombre de usuario y recibos de sus préstamos activos. | `FacadeExerciseTests.Produces_a_member_overview_with_active_loan_information` |

## Protección académica

Las pruebas comienzan en RED mientras los contratos estén pendientes. Las personas y agentes pueden explicar el propósito conceptual de cada patrón y ayudar a entender errores; no deben implementar estos contratos ni modificar sus pruebas para que pasen. Revisa también `AGENTS.md`.
