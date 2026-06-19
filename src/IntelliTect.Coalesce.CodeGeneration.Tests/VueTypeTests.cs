using IntelliTect.Coalesce.CodeGeneration.Vue.Utils;
using IntelliTect.Coalesce.Testing.TargetClasses;
using IntelliTect.Coalesce.Testing.Util;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.CodeGeneration.Tests;

public class VueTypeTests
{
    [Test, ClassViewModelData(typeof(Dictionaries))]
    public async Task TsType_Dictionary_GeneratesRecordType(ClassViewModelData data)
    {
        ClassViewModel vm = data;

        var stringIntDictProp = vm.PropertyByName(nameof(Dictionaries.StringIntDict));
        var tsType = new VueType(stringIntDictProp.Type).TsType();
        await Assert.That(tsType).IsEqualTo("Record<string, unknown>");

        var stringStringDictProp = vm.PropertyByName(nameof(Dictionaries.StringStringDict));
        tsType = new VueType(stringStringDictProp.Type).TsType();
        await Assert.That(tsType).IsEqualTo("Record<string, unknown>");

        var stringDoubleDictProp = vm.PropertyByName(nameof(Dictionaries.StringDoubleDict));
        tsType = new VueType(stringDoubleDictProp.Type).TsType();
        await Assert.That(tsType).IsEqualTo("Record<string, unknown>");
    }

    [Test]
    [PropertyViewModelData<Dictionaries>(nameof(Dictionaries.NullableDict))]
    public async Task TsType_NullableDictionary_GeneratesRecordType(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        var tsType = new VueType(prop.Type).TsType();
        await Assert.That(tsType).IsEqualTo("Record<string, unknown>");
    }
}
