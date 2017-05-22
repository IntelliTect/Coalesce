

Annotations
-----------

Annotations Overview
~~~~~~~~~~~~~~~~~~~~

Annotations are simple attributes called DataAnnotationss

Annotation List
~~~~~~~~~~~~~~~

-  `Client Validation <#ClientValidation>`__
-  `Controller <#Controller>`__
-  `Create <#Create>`__
-  `CreateController <#CreateController>`__
-  `Date Type <#DateType>`__
-  `Default Order By <#DefaultOrderBy>`__
-  `Delete <#Delete>`__
-  `Detail <#Detail>`__
-  `Display <#Display>`__
-  `DtoExcludes <#DtoExcludes>`__
-  `DtoIncludes <#DtoIncludes>`__
-  `Edit <#Edit>`__
-  `Execute <#Execute>`__
-  `File Download <#FileDownload>`__
-  `Hidden <#Hidden>`__
-  `Inject <#Inject>`__
-  `Internal Use <#InternalUse>`__
-  `List Group <#ListGroup>`__
-  `List Text <#ListText>`__
-  `Many to Many <#ManytoMany>`__
-  `Read <#Read>`__
-  `Read Only Api <#ReadOnlyApi>`__
-  `Search <#Search>`__
-  `SelectFilter <#SelectFilter>`__
-  `TypeScriptPartial <#TypeScriptPartial>`__


Controller
~~~~~~~~~~

Allows for control over the generated MVC Controllers. Properties
include:

ApiRouted
^^^^^^^^^

Use cases include:

-  Wanting to define your routes through IRouteBuilder in Startup.cs
   instead
-  Not wanting API controllers to be exposed by default.
-  Wanting to route to your own custom controller that inherits from a
   generated API controller in order to implement more granular or
   complex authorization logic.

::


        [Controller(ApiRouted = false)]
        public class Person
        {
            public int PersonId { get; set; }

            ...
        }

Create
~~~~~~

Controls whether creation of a new object of this type is allowed from
the client side.

-  bool Allow = true

::


        [Create(Allow = false)]
        [Delete(Allow = false)]
        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

Create Controller
~~~~~~~~~~~~~~~~~

By default an API and View controller are both created. This allows for
supressing the creation of either or both of these.

-  bool WillCreateView
-  bool WillCreateApi

::


        [CreateController(view: false)]
        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

Date Type
~~~~~~~~~

Specifies whether a DateTime type will have a date and a time or only a
date.

| Class defaults to DateTypes.DateTime
| Date Types:

-  DateTime = 0
-  DateOnly = 1

::


        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            [DateType(DateTypeAttribute.DateTypes.DateOnly)]
            public DateTime? LastWellCheck { get; set; }
        }

Default Order By
~~~~~~~~~~~~~~~~

Allows setting of the default manner in which the data returned to the
client will be sorted. Multiple fields can be used on the sort by
specifying an index.

Order By Directions

-  Ascending = 0
-  Descending = 1

Options
^^^^^^^

-  OrderByDirections OrderByDirection
-  int FieldOrder

::


        public class Person
        {
            public int PersonId { get; set; }
            [DefaultOrderBy(2)]    
            public string FirstName { get; set; }
            
            [DefaultOrderBy(1)]
            public string LastName { get; set; }
        }
        public class LoginHistory
        {
            public int LoginHistoryId {get; set;}
            
            [DefaultOrderBy(OrderByDirection = DefaultOrderByAttribute.OrderByDirections.Descending)]
            public DateTime Date {get; set;}
        }

Delete
~~~~~~

Controls whether an object of this type can be deleted from the client
side.

-  bool Allow = true

::


        [Delete(Allow = false)]
        [Create(Allow = false)]
        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

Detail
~~~~~~

Provides a detail field. Can be used to expose a tooltip or hint.

::


        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            [Detail("i.e., asthma, reflux, eczema")]
            public string ChronicHealthProblems { get; set; }
        }

Display
~~~~~~~

Controls the display name and the ordering on the admin pages.

::


        public class Person
        {
            public int PersonId { get; set; }
            [Display(Name = "Name", Order = 1)]
            public string TheFullName { get; set; }
            
        }

DtoExcludes
~~~~~~~~~~~

Allows for easily controlling when data gets set to the client. When
requesting data from the generated client-side list view models you can
specify an "Includes" property. This property controls two areas:

-  If your underlying class implements IIncludable then the Include
   method will be called before querying the database, and you can
   choose which properties to include based on the passed view.
-  When the database entries are returned to the client they will be
   trimmed based on the requested view and the values in DtoExcludes and
   DtoIncludes.

-  string ContentViews = comma-separated list of views this property
   will not be included on.

::


        <script>
        [... additional code ...]
        personList.includes = "Editor";
        personList.load();
        </script>
        

::


        // Don't include CreatedBy when editing - will be included for all other views
        [DtoExcludes("Editor")]
        public AppUser CreatedBy { get; set; }
        // LastName will be included in all views
        public string LastName { get; set; }

DtoIncludes
~~~~~~~~~~~

| This is the opposite of DtoExcludes. See
  `DtoExcludes <#DtoExcludes>`__ for more information.
| Controls when a property is visible on the client.

-  string ContentViews = comma-separated list of views this property
   will not be included on.

::


        <script>
        [... additional code ...]
        personList.includes = "AllInfo";
        personList.load();
        </script>
        

::


        // Include Title only when all information is being displayed
        [DtoIncludes("AllInfo")]
        public string Title { get; set; }
        // LastName will be included in all views
        public string LastName { get; set; }

Edit
~~~~

Controls what permissions are needed to edit a model

::


        [Edit(Roles = AppRoles.Admin)]
        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

Execute
~~~~~~~

Specifies what permissions are needed to execute static or instance
methods

::


        [Execute(Roles = AppRoles.Admin)]
        public Person CreatePerson(string firstName, string lastName)
        {
            var newPerson = new Person() {FirstName = firstName, LastName = lastName}
            return newPerson;
        }

File Download
~~~~~~~~~~~~~

Specifies that this property should be exposed as a file download.

::


            public class Signature: TrackingBase
        {
            public int SignatureId { get; set; }
            public byte[] Content { get; set; }
            
            [NotMapped] [FileDownload]
            public Image Image
            {
                get
                {
                    MemoryStream ms = new MemoryStream(Content);
                    Image returnImage = Image.FromStream(ms);
                    return returnImage;
                }
                set
                {
                    MemoryStream ms = new MemoryStream();
                    value.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                    Content = ms.ToArray();
                }
            }
        }

Hidden
~~~~~~

Mark an property as hidden from the edit, List or All areas.

Areas
^^^^^

-  All = 0
-  List = 1
-  Edit = 2

Options
^^^^^^^

-  Areas Area

::


        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            [Hidden(HiddenAttribute.Areas.All)]
            public int? IncomeLevelId { get; set; }
        }

Inject
~~~~~~

Used to mark a method parameter for depencency injection from the
application's IServiceProvider.

::


        
        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public string GetFullName([Inject] ILogger<Person> logger)
            {
                logger.LogInformation(0, $"Person {PersonId}'s full name was requested");
                return $"{FirstName} {LastName}";
            }
        }

Internal Use
~~~~~~~~~~~~

Used to mark a property or method for internal use. Internal use methods
are not exposed via the API.

::


        public class Picture
        {
            public int PictureId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public byte[] Original { get; set; }
            public byte[] Thumbnail { get; set; }
            [InternalUse]
            public Image OriginalImage()
            {
                var ms = new MemoryStream(Original);
                return new Bitmap(ms);
            }
            [InternalUse]
            public Image ThumbnailImage()
            {
                var ms = new MemoryStream(Original);
                return new Bitmap(ms);
            }
        }

List Group
~~~~~~~~~~

List groups are used for string fields that should provide a dropdown
list. This allows for multiple properties to contribute values to a
common list. This is a simple solution to using a linked table where
adding items is really easy, but it is also easy to select existing
items.

::


        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            
                [ListGroup("School")]
            public string Education1School { get; set; }
                [ListGroup("School")]
            public string Education2School { get; set; }
        }

List Text
~~~~~~~~~

When a dropdown list is used to select a related object, this controls
the text shown in the dropdown by default. When using these dropdown,
only the key and this field are returned as search results.

::


        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            [ListText]
            [Hidden]
            [NotMapped]
            public string Name
            {
                get { return $"{FirstName} {LastName}"; }
            }
        }

Many to Many
~~~~~~~~~~~~

Used to specify a Many to Many relationship. Because EF core does not
support automatic intermediate mapping tables, this field is used to
allow for direct reference of the many-to-many collections from the
ViewModel.

::


        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            [ManyToMany("Appointments")]
            public ICollection PersonAppointments { get; set; }
        }

Read
~~~~

Specify the role needed for read access

::


        [Read(Roles = AppRoles.Admin)]
        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

Read Only Api
~~~~~~~~~~~~~

Specify a property is not able to be modified through the Api.

::


        public class Person
        {
            public int PersonId { get; set; }
            [ReadOnlyApi]
            public string FirstName { get; set; }
            [ReadOnlyApi]
            public string LastName { get; set; }
        }

Search
~~~~~~

Coalesce supports searching in various properties. First is for a list
of items in a table. Second is in a drop down selection list. In both
these cases, it is important to know which fields to search. By default,
the system will search any field with the name 'Name'. If this doesn't
exist, the ID is used. Additionally the [Search] attribute can be used
on any fields to be searched.

Search attributes can also be placed on objects. In this case the
searchable fields of the child object will be used in the search. Note
that due to a feature/bug in EF Core, objects that are nullable are not
supported.

::


        public enum SearchMethods
        {
            BeginsWith = 1,
            Contains = 2
        };

Options

-  bool IsSplitOnSpaces = true
-  SearchMethods SearchMethod = SearchMethods.BeginsWith

::


        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            [Search]
            public string LastName { get; set; }
        }

The Search attribute has two optional parameters: SearchMethod and
IsSplitOnSpaces. SearchMethod specifies whether the search will be a
contains or a begins with. The default is begins with. Note that
standard indexing can be used to speed up begins with searches. If
IsSplitOnSpaces is true, each word will be searched independently. This
is useful when searching for a full name across two or more fields. In
the above example, using IsSplitOnSpaces: true, would likely provide
more intuitive behavior as it will search both first name and last name
for each value entered.

Additionally, you can add the Search annotation to a child object. This
will search the searchable fields of child.

Select Filter
~~~~~~~~~~~~~

Specify a property to restrict dropdown menus by. Values presented will
be only those where the value of the foreign property matches the value
of the local property.

The local property name defaults to the same value of the foreign
property.

Additionally, in place of a ``LocalPropertyName`` to check against, you
may instead specify a static value using ``StaticPropertyValue`` to
filter by a constant.

::


        public class Employee
        {
            public int EmployeeId { get; set; }
            public int EmployeeTypeId { get; set; }
            public EmployeeType EmployeeType { get; set; }
            public int EmployeeRankId { get; set; }
        
            [SelectFilter(ForeignPropertyName = nameof(EmployeeTypeId), LocalPropertyName = nameof(EmployeeTypeId))]
            public EmployeeRank EmployeeRank { get; set; }
        }
        
        public class EmployeeRank
        {
            public int EmployeeRankId { get; set; }
            public int EmployeeTypeId { get; set; }
            public EmployeeType EmployeeType { get; set; }
        }
        

TypeScript Partial
~~~~~~~~~~~~~~~~~~

If defined on a model, a typescript file will be generated in
./Scripts/Partials if one does not already exist. This will allow you to
extend the behavior of the generated TypeScript view models.
