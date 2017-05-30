
.. _DTOs:

Data Transfer Objects (DTOs)
----------------------------

Data Transfer Objects, or DTOs, allow for transformations
of data from the data store into a format more suited for transfer and
use on the client side. This often means trimming properties and
flattening structures to provide a leaner over-the-wire experience. The
goal for Coalesce is to support this as seamlessly as possible.

Coalesce supports several types of DTOs:

-  DTOs that are automatically generated for each POCO database object.
   These are controlled via :ref:`ModelAttributes` on the POCO.
-  DTOs that support the standard ViewModels.
-  DTOs that are created with IClassDto and create unique ViewModels.
-  DTOs based on database views.

Automatically Generated DTOs
~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Every POCO database class in Coalesce gets a DTO. These DTOs are used to
shuttle data back and forth to the client. They are generated classes
that have nullable versions of all the properties on the POCO class, and are the mechanism by which almost all the techniques described in :ref:`ControllingLoading` function.

:ref:`DtoIncludesExcludesAttr` and the :ref:`Includes` infrastructure can be used to indicate which properties should be transferred to the client in which cases, and :ref:`IncludeTree` is used to dictate how these DTOs are constructed from POCOs populated from the database.


DTOs with Standard View Models
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

TODO: Dan fills this in.

::


        Example needed

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

ListDataSource is supported from the POCO database object, but it
transferred to the DTO. All querying must be done on the original object
and not the DTO since only fields in the database can be queried via
Entity Framework.

::


        public IEnumerable<CaseDto> CaseDtos { get; set; }

Methods are not yet supported on DTOs.

::


        public class CaseDto: IClassDto<Case, CaseDto>
        {
            [Key]
            public int CaseId { get; set; }
            public string Title { get; set; }
            [ReadOnly(true)]
            public string AssignedToName { get; set; }
            public void Update(Case obj, ClaimsPrincipal user, string includes)
            {
                obj.Title = Title;
            }
            public CaseDto CreateInstance(Case obj, ClaimsPrincipal user = null, string includes = null, Dictionary<object, object> objects = null, IncludeTree tree = null)
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

A view can manually be created in the database that will map to a
regular POCO object. The typical approach is to create a POCO that has
all the desired fields. Build an EF migration for this POCO, but don't
apply it to the database. Replace the generated migration code with the
code to generate/remove the view instead. This will all the view to be
automatically created. This approach is ideal for situations where you
want to filter at the database level on calculated fields. Be wary of
unintentionally creating views that are read-only. This is fine, but the
data will not be updatable like it is with a regular table based POCO.
