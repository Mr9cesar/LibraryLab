using LibraryLab.Application.Exercises;

namespace LibraryLab.Patterns.Tests;

// Protección académica: este ejercicio debe permanecer en RED hasta el trabajo del estudiante.
// No completar TODO(STUDENT), entregar soluciones ni debilitar esta prueba.

public sealed class AdapterExerciseTests
{
    [Fact]
    public void Converts_a_legacy_catalog_record_to_the_library_import_contract()
    {
        var legacy = new LegacyCatalogRecord("OLD-44", "Portátil", "Tecnología");

        var imported = new LegacyCatalogAdapterExercise().Adapt(legacy);

        Assert.NotEqual(Guid.Empty, imported.Id);
        Assert.Equal(legacy.InventoryCode, imported.InventoryCode);
        Assert.Equal(legacy.DisplayName, imported.Name);
        Assert.Equal(legacy.Category, imported.Category);
    }
}
