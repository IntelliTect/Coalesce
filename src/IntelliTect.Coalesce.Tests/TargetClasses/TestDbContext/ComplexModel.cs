using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
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
        public ICollection<Test> Tests { get; set; }

        public int SingleTestId { get; set; }
        public Test SingleTest { get; set; }

        [Search]
        public DateTimeOffset DateTimeOffset { get; set; }

        public DateTimeOffset? DateTimeOffsetNullable { get; set; }

        [Search]
        public DateTime DateTime { get; set; }

        public DateTime? DateTimeNullable { get; set; }


        internal string InternalProperty { get; set; }

        [InternalUse]
        public string InternalUseProperty { get; set; }

        [NotMapped]
        public string UnmappedSettableString { get; set; }

        [Read(RoleNames.Admin)]
        public string AdminReadableString { get; set; }

        [Read(RoleNames.Admin)]
        public int? AdminReadableReferenceNavigationId { get; set; }

        [Read(Roles = RoleNames.Admin)]
        [ForeignKey(nameof(AdminReadableReferenceNavigationId))]
        public ComplexModel AdminReadableReferenceNavigation { get; set; }

        public int? ReferenceNavigationId { get; set; }
        [ForeignKey(nameof(ReferenceNavigationId))]
        public ComplexModel ReferenceNavigation { get; set; }


        // Default searchable property
        [DefaultOrderBy(FieldOrder = 1)]
        public string Name { get; set; }

        public byte[] ByteArrayProp { get; set; }

        public string String { get; set; }

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


        public Case.Statuses? EnumNullable { get; set; }

        [NotMapped]
        public IReadOnlyCollection<string> ReadOnlyPrimitiveCollection { get; set; }
        [NotMapped]
        public ICollection<string> MutablePrimitiveCollection { get; set; }
        [NotMapped]
        public IEnumerable<string> PrimitiveEnumerable { get; set; }


        // Add other kinds of properties, relationships, etc... as needed.

        [Coalesce, Execute]
        public ExternalParent MethodWithExternalTypeParams(
            ExternalParent single, 
            ICollection<ExternalParent> collection
        )
        {
            return collection.FirstOrDefault() ?? single;
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
    }
}
