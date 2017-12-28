
.. _DTOs:

Custom DTOs
-----------

In addition to the generated :ref:`DTOs` that Coalesce will create for you, you may also create your own implementations of :csharp:`IClassDto`. These types are first-class citizens in Coalesce - you will get a full suite of features surrounding them as if they were entities

Data Transfer Objects, or DTOs, allow for transformations
of data from the data store into a format more suited for transfer and
use on the client side. This often means trimming properties and
flattening structures to provide a leaner over-the-wire experience. The
goal for Coalesce is to support this as seamlessly as possible.

Coalesce supports several types of DTOs:

-  DTOs that are automatically generated for each POCO database object.
   These are controlled via :ref:`ModelAttributes` on the POCO.
-  DTOs that are created with IClassDto and create unique ViewModels.
-  DTOs based on database views.

IClassDtos
~~~~~~~~~~

This option for DTOs allows you to create an arbitrary class that is
related one-to-one with a POCO database class. The IClassDto provides
the mechanism to create the DTO and update the underlying database
object. The client ViewModels as build based on the DTO class and
therefore can be significantly modified from the original. These support
full list and editing capability with a complete set of views: list,
cards, create/edit.

Note: Because DTOs of this type are updatable, a primary key must be
used. Select the same primary key name as the underlying object for this
to be detected automatically.

The Update method passes in a database object for you to update. You can
do security trimming with the user and get Includes information from the
includes parameter.

The CreateInstance method creates the DTO from the database object.
Again you have the user and includes properties. However, this time you
also have a collection of objects. The purpose of this object is to
track what has already been serialized. This same interface is used for
the automatically generated DTOs. This serves two purposes.

-  A way to only transform objects once and then reuse the results where
   the object is referenced again.
-  A way to prevent recursion when transforming from the database
   objects to the DTOs.

The collection is keyed by the original object and the value is the DTO.

To get Coalesce to recognize the DTO, it must be added to the context as
an IEnumerable. This is simply an indicator for Coalesce and does not
impact the database. This may be replaced by other less obtrusive
methods in the future.

ListDataSource is supported from the POCO database object, but is
transferred to the DTO. All querying must be done on the original object
and not the DTO since only fields in the database can be queried via
Entity Framework.

    .. code-block:: c#

        public class AppDbContext : DbContext
        {
            public DbSet<Case> Cases { get; set; }
            public IEnumerable<CaseDto> CaseDtos { get; set; }
        }


    .. code-block:: c#

        public class CaseDto : IClassDto<Case, CaseDto>
        {
            [Key]
            public int CaseId { get; set; }

            public string Title { get; set; }

            [Read]
            public string AssignedToName { get; set; }

            public void Update(Case obj, IMappingContext context)
            {
                obj.Title = Title;
            }

            public CaseDto CreateInstance(Case obj, IMappingContext context, IncludeTree tree = null)
            {
                var dto = new CaseDto();
                dto.CaseId = obj.CaseKey;
                dto.Title = obj.Title;
                if (obj.AssignedTo != null)
                {
                    dto.AssignedToName = obj.AssignedTo.Name;
                }
                return dto;
            }
        }
            
Database Views
~~~~~~~~~~~~~~

A view can manually be created in the database that will map to a regular POCO object. The typical approach is to create a POCO that has all the desired fields. Build an EF migration for this POCO, but don't apply it to the database. Replace the generated migration code with the :csharp:`migrationBuilder.Sql(...)` calls to generate/remove the view instead so that the view will exist for anyone who runs the migrations. This approach is ideal for situations where you want to filter at the database level on calculated fields.

Read-only views are fine, but you'll want to annotate the class with the proper :ref:`SecurityAttributes` (deny edit, create, and delete) to ensure the generated code matches the available behavior as accurately as possible.
