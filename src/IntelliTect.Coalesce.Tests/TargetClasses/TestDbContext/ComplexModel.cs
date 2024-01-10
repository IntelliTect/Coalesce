using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    public class ComplexModel
    {
        [DefaultOrderBy(FieldOrder = 2)]
        public int ComplexModelId { get; set; }

        [InverseProperty(nameof(Test.ComplexModel))]
        [Search]
        public ICollection<Test> Tests { get; set; }

        /// <summary>
        /// Test case for foreign keys without a reference navigation prop.
        /// This configuration *will* be picked up by EF conventions.
        /// </summary>
        [ForeignKey(nameof(ComplexModelDependent.ParentId))]
        public ICollection<ComplexModelDependent> ChildrenWithoutRefNavProp { get; set; }

        public int SingleTestId { get; set; }
        public Test SingleTest { get; set; }

        public EnumPkId EnumPkId { get; set; }
        public EnumPk EnumPk { get; set; }

        [Search]
        public DateTimeOffset DateTimeOffset { get; set; }

        public DateTimeOffset? DateTimeOffsetNullable { get; set; }

        [Search]
        public DateTime DateTime { get; set; }

        public DateTime? DateTimeNullable { get; set; }

        public DateOnly SystemDateOnly { get; set; }
        public TimeOnly SystemTimeOnly { get; set; }

        [DateType(DateTypeAttribute.DateTypes.DateOnly)]
        public DateTime DateOnlyViaAttribute { get; set; }


        internal string InternalProperty { get; set; }

        [InternalUse]
        public string InternalUseProperty { get; set; }

        [NotMapped]
        public string UnmappedSettableString { get; set; }

        [Read(RoleNames.Admin)]
        public string AdminReadableString { get; set; }

        [Restrict<AuthenticatedRestriction>]
        public string RestrictedString { get; set; }

        [Restrict<AuthenticatedRestriction>]
        public string RestrictInit { get; init; }

        [Read(RoleNames.Admin)]
        public int? AdminReadableReferenceNavigationId { get; set; }

        [Read(Roles = RoleNames.Admin)]
        [ForeignKey(nameof(AdminReadableReferenceNavigationId))]
        public ComplexModel AdminReadableReferenceNavigation { get; set; }

        public int? ReferenceNavigationId { get; set; }
        [ForeignKey(nameof(ReferenceNavigationId))]
        public ComplexModel ReferenceNavigation { get; set; }


        public int? NoAutoIncludeReferenceNavigationId { get; set; }
        [Read(NoAutoInclude = true)]
        [ForeignKey(nameof(NoAutoIncludeReferenceNavigationId))]
        public ComplexModel NoAutoIncludeReferenceNavigation { get; set; }


        public int? NoAutoIncludeByClassReferenceNavigationId { get; set; }
        [ForeignKey(nameof(NoAutoIncludeByClassReferenceNavigationId))]
        public Company NoAutoIncludeByClassReferenceNavigation { get; set; }


        // Default searchable property
        [DefaultOrderBy(FieldOrder = 1)]
        public string Name { get; set; }

        public byte[] ByteArrayProp { get; set; }

        public string String { get; set; }

        [DefaultValue("Inigo")]
        public string StringWithDefault { get; set; }

        [DefaultValue(42)]
        public int IntWithDefault { get; set; }

        [DefaultValue(Math.PI)]
        // Double's min/max are the biggest magnitude values that can occur in Range.
        // This ensures we don't have overflows or incorrect parsing to smaller numeric types.
        [Range(double.MinValue, double.MaxValue)] 
        public double DoubleWithDefault { get; set; }

        [DefaultValue(EnumPkId.Value10)]
        public EnumPkId EnumWithDefault{ get; set; }

        [DataType("Color")]
        public string Color { get; set; }

        [Search(SearchMethod = SearchAttribute.SearchMethods.Equals)]
        public string StringSearchedEqualsInsensitive { get; set; }

        [Search(SearchMethod = SearchAttribute.SearchMethods.EqualsNatural)]
        public string StringSearchedEqualsNatural { get; set; }

        public int Int { get; set; }
        public int? IntNullable { get; set; }
        public decimal? DecimalNullable { get; set; }
        public long Long { get; set; }
        public Guid Guid { get; set; }
        public Guid? GuidNullable { get; set; }

        [Range(1, 100)]
        public int NonNullNonZeroInt { get; set; }

        [ClientValidation(MinValue = 0, MaxValue = 10)]
        public int ClientValidationInt { get; set; }

        [ClientValidation(MinLength = 0, MaxLength = 10)]
        public string ClientValidationString { get; set; }


        public Case.Statuses? EnumNullable { get; set; }

        [NotMapped]
        public IReadOnlyCollection<string> ReadOnlyPrimitiveCollection { get; set; }
        [NotMapped]
        public ICollection<string> MutablePrimitiveCollection { get; set; }
        [NotMapped]
        public IEnumerable<string> PrimitiveEnumerable { get; set; }

        // Add other kinds of properties, relationships, etc... as needed.

        [Coalesce, Execute]
        public ExternalParent MethodWithManyParams(
            ExternalParent singleExternal,
            ICollection<ExternalParent> collectionExternal,
            IFile file,
            string strParam,
            string[] stringsParam,
            DateTime dateTime,
            int integer,
            bool boolParam,
            Case.Statuses enumParam,
            Company model,
            Company[] modelCollection
        )
        {
            return collectionExternal.FirstOrDefault() ?? singleExternal;
        }

        [Coalesce, Execute]
        public ExternalParentAsOutputOnly MethodWithExternalTypesWithSinglePurpose(
            ExternalParentAsInputOnly single,
            ICollection<ExternalParentAsInputOnly> collection
        )
        {
            return new ExternalParentAsOutputOnly();
        }

        [Coalesce, Execute]
        public OutputOnlyExternalTypeWithoutDefaultCtor MethodWithOutputOnlyExternalType() => null;

        [Coalesce, Execute]
        public OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties MethodWithOutputOnlyExternalType2() => null;

        [Coalesce, Execute]
        public OutputOnlyExternalTypeWithRequiredEntityProp MethodWithOutputOnlyExternalType3() => null;

        [Coalesce, Execute]
        public InputOutputOnlyExternalTypeWithRequiredNonscalarProp MethodWithInputOutputOnlyExternalTypeWithRequiredNonscalarProp(InputOutputOnlyExternalTypeWithRequiredNonscalarProp i) => i;

        [Coalesce, Execute]
        public void MethodWithSingleFileParameter(IFile file) { }

        [Coalesce, Execute]
        public void MethodWithMultiFileParameter(ICollection<IFile> files) { }

        [Coalesce, Execute]
        public static string[] MethodWithStringArrayParameterAndReturn(string[] strings)
        {
            return strings;
        }

        [Coalesce]
        public IFile DownloadAttachment() => new File(ByteArrayProp) { Name = Name };

        [Coalesce]
        [ControllerAction(HttpMethod.Get, VaryByProperty = nameof(ByteArrayProp))]
        public IFile DownloadAttachment_VaryByteArray() => new File(ByteArrayProp) { Name = Name };

        [Coalesce]
        [ControllerAction(HttpMethod.Get, VaryByProperty = nameof(DateTimeOffset))]
        public IFile DownloadAttachment_VaryDate() => new File(ByteArrayProp) { Name = Name };

        [Coalesce]
        [ControllerAction(HttpMethod.Get, VaryByProperty = nameof(Name))]
        public IFile DownloadAttachment_VaryString() => new File(ByteArrayProp) { Name = Name };

        [Coalesce]
        [ControllerAction(HttpMethod.Get, VaryByProperty = nameof(Int))]
        public IFile DownloadAttachment_VaryInt() => new File(ByteArrayProp) { Name = Name };

        [Coalesce]
        [ControllerAction(HttpMethod.Get, VaryByProperty = nameof(Guid))]
        public IFile DownloadAttachment_VaryGuid() => new File(ByteArrayProp) { Name = Name };

        [Coalesce]
        public ItemResult<IFile> DownloadAttachmentItemResult()
        {
            return new File(ByteArrayProp) { Name = Name };
        }

        [Coalesce]
        public static ItemResult<IFile> DownloadAttachmentStatic()
        {
            return new File(new byte[] { 0x42 }) { Name = "42.png" };
        }

        [ControllerAction(Method = HttpMethod.Get)]
        [Coalesce]
        public Task MethodWithOptionalCancellationToken(string q, CancellationToken cancellationToken = default) => Task.CompletedTask;

        [Coalesce]
        public Task MethodWithOptionalEnumParam(Case.Statuses status = Case.Statuses.Open) => Task.CompletedTask;

        [Coalesce]
        public ExternalTypeWithDtoProp ExternalTypeWithDtoProp(ExternalTypeWithDtoProp input) => input;

        [Coalesce]
        public CaseDtoStandalone CustomDto(CaseDtoStandalone input) => input;

        [Coalesce]
        [ControllerAction(Method = HttpMethod.Post)]
        public static ItemResult HasTopLevelParamWithSameNameAsObjectProp(
            int complexModelId,
            ComplexModel model) => true;

        [Coalesce]
        public PositionalRecord MethodWithPositionRecord(PositionalRecord rec) => new PositionalRecord("a", 42);

        [Coalesce]
        public InitRecordWithDefaultCtor MethodWithInitRecord(InitRecordWithDefaultCtor rec) => new InitRecordWithDefaultCtor { String = "a", Num = 42 };

        [Coalesce, Execute(ValidateAttributes = false)]
        public ItemResult MethodWithValidationExplicitOff([Required] ValidationTarget target) => true;

        [Coalesce, Execute(ValidateAttributes = true)]
        public ItemResult MethodWithValidationExplicitOn([Required] ValidationTarget target) => true;
    }

    [Create(SecurityPermissionLevels.DenyAll)]
    [Edit(SecurityPermissionLevels.DenyAll)]
    public class ReadOnlyEntityUsedAsMethodInput
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Coalesce]
        public static void StaticCreate(ReadOnlyEntityUsedAsMethodInput foo) { }
    }

    public class RequiredAndInitModel
    {
        public int Id { get; set; }

#if NET7_0_OR_GREATER
        public required string RequiredRef { get; set; }
        public required int RequiredValue { get; set; }
        public required string RequiredInitRef { get; init; }
        public required int RequiredInitValue { get; init; }
        public string InitRef { get; init; }
        public int InitValue { get; init; }
#endif
    }
}
