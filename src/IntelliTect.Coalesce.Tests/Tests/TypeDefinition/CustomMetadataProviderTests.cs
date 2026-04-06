using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Testing.Util;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Tests.TypeDefinition;

public class CustomMetadataProviderTests
{
    [Test]
    [ClassViewModelData(typeof(ComplexModel))]
    public async Task ClassLevel_ExtractsConstructorAndNamedArgs(ClassViewModelData data)
    {
        ClassViewModel vm = data;
        var results = vm.GetCustomMetadata().ToList();

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0].Key).IsEqualTo("customMetadataTarget");
        await Assert.That(results[0].Properties.Any(kv =>
            string.Equals(kv.Key, "name", StringComparison.OrdinalIgnoreCase)
            && (string)kv.Value! == "ClassLevel")).IsTrue();
        await Assert.That(results[0].Properties.Any(kv =>
            string.Equals(kv.Key, "value", StringComparison.OrdinalIgnoreCase)
            && kv.Value is int and 42)).IsTrue();
    }

    [Test]
    [ClassViewModelData(typeof(ComplexModel))]
    public async Task PropertyLevel_ExtractsMultipleAttributes(ClassViewModelData data)
    {
        ClassViewModel vm = data;
        var results = vm.PropertyByName(nameof(ComplexModel.DateTimeOffset))!
            .GetCustomMetadata().ToList();

        await Assert.That(results).Count().IsEqualTo(2);

        var target = results.Single(r => r.Key == "customMetadataTarget");
        await Assert.That(target.Properties.Any(kv =>
            string.Equals(kv.Key, "name", StringComparison.OrdinalIgnoreCase)
            && (string)kv.Value! == "PropLevel")).IsTrue();

        var marker = results.Single(r => r.Key == "customMetadataMarker");
        await Assert.That(marker.Properties).IsEmpty();
    }

    [Test]
    [ClassViewModelData(typeof(ComplexModel))]
    public async Task MethodLevel_ExtractsAttributes(ClassViewModelData data)
    {
        ClassViewModel vm = data;
        var results = vm.MethodByName(nameof(ComplexModel.MethodWithManyParams))!
            .GetCustomMetadata().ToList();

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0].Key).IsEqualTo("customMetadataTarget");
        await Assert.That(results[0].Properties.Any(kv =>
            string.Equals(kv.Key, "name", StringComparison.OrdinalIgnoreCase)
            && (string)kv.Value! == "MethodLevel")).IsTrue();
        await Assert.That(results[0].Properties.Any(kv =>
            string.Equals(kv.Key, "value", StringComparison.OrdinalIgnoreCase)
            && kv.Value is int and 99)).IsTrue();
    }

    [Test]
    [ClassViewModelData(typeof(ComplexModel))]
    public async Task ParameterLevel_ExtractsAttributes(ClassViewModelData data)
    {
        ClassViewModel vm = data;
        var results = vm.MethodByName(nameof(ComplexModel.MethodWithManyParams))!
            .Parameters.Single(p => p.Name == "strParam")
            .GetCustomMetadata().ToList();

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0].Key).IsEqualTo("customMetadataTarget");
        await Assert.That(results[0].Properties.Any(kv =>
            string.Equals(kv.Key, "name", StringComparison.OrdinalIgnoreCase)
            && (string)kv.Value! == "ParamLevel")).IsTrue();
    }
}
