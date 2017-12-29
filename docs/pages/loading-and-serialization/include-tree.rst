


.. _IncludeTree:

Include Tree
============

When Coalesce maps from the your POCO objects that are returned from EF Core queries, it will follow a structure called an :csharp:`IncludeTree` to determine what relationships to follow and how deep to go in re-creating that structure in the mapped DTOs.


.. contents:: Contents
    :local:

Purpose
-------

Without an :csharp:`IncludeTree` present, Coalesce will map the entire object graph that is reachable from the root object. This can often spiral out of control if there aren't any rules defining how far to go while turning this graph into a tree.

    For example, suppose you had the following model with a many-to-many relationship (key properties omitted for brevity):

        .. code-block:: c#

            public class Employee
            {
                [ManyToMany("Projects")]
                public ICollection<EmployeeProject> EmployeeProjects { get; set; }
                        
                public static IQueryable<Employee> WithProjectsAndMembers(AppDbContext db, ClaimsPrincipal user)
                {
                    // Load all projects of an employee, as well as all members of those projects.
                    return db.Employees.Include(e => e.EmployeeProjects)
                                       .ThenInclude(ep => ep.Project.EmployeeProjects)
                                       .ThenInclude(ep => ep.Employee);
                }
            }

            public class Project
            {
                [ManyToMany("Employees")]
                public ICollection<EmployeeProject> EmployeeProjects { get; set; }
            }

            public class EmployeeProject
            {
                public Employee Employee { get; set; }
                public Project Project { get; set; }
            }

    Now, imagine that you have five employees and five projects, with every employee being a member of every project (i.e. there are 25 EmployeeProject rows).

    Your client code makes a call to the Coalesce-generated API to load Employee #1 using the custom data source:

        .. code-block:: typescript

            var employee = new ViewModels.Employee();
            employee.dataSource = new employee.dataSources.WithProjectsAndMembers();
            employee.load(1);

    If you're already familiar with the fact that an :csharp:`IncludeTree` is implicitly created in this scenario, then imagine for a moment that this is not the case (if you're not familiar with this fact, then keep reading!).

    After Coalesce has called your :ref:`CustomDataSources` and evaluated the EF IQueryable returned, there are now 35 objects loaded into the current :csharp:`DbContext` being used to handle this request - the 5 employees, 5 projects, and 25 relationships.

    To map these objects to DTOs, we start with the root (employee #1) and expand outward from there until the entire object graph has been faithfully re-created with DTO objects, including all navigation properties.

    The root DTO object (employee #1) then eventually is passed to the JSON serializer by ASP.NET Core to formulate the response to the request. As the object is serialized to JSON, the only objects that are not serialized are those that were already serialized as an ancestor of itself. What this ultimately means is that the structure of the serialized JSON with our example scenario ends up following a pattern like this (the vast majority of items have been omitted):

        .. code-block:: none

            Employee#1
                EmployeeProject#1
                    Project#1
                        EmployeeProject#6
                            Employee#2
                                EmployeeProject#7
                                    Project#2
                                        ... continues down through all remaining employees and projects.
                                ...
                        EmployeeProject#11
                            Employee#3
                        ...
                EmployeeProject#2
                    Project#2
                ...
    
    See how the structure includes the EmployeeProjects of Employee#2? We didn't write our custom data source calls to :csharp:`.Include` in such a way that indicated that we wanted the root employee, their projects, the employees of those projects, and then **the projects of those employees**. But, because the JSON serializer blindly follows the object graph, that's what gets serialized. It turns out that the depth of the tree increases on the order of :code:`O(n^2)`, and the total size increases on the order of :code:`Ω(n!)`.

    This is where :csharp:`IncludeTree` comes in. When you use a custom data source like we did above, Coalesce automatically captures the structure of the calls to :csharp:`.Include` and :csharp:`.ThenInclude`, and uses this to perform trimming during creation of the DTO objects.

    With an :csharp:`IncludeTree` in place, our new serialized structure looks like this:

        .. code-block:: none

            Employee#1
                EmployeeProject#1
                    Project#1
                        EmployeeProject#6
                            Employee#2
                        EmployeeProject#11
                            Employee#3
                        ...
                EmployeeProject#2
                    Project#2
                ...

    No more extra data trailing off the end of the projects' employees!


Usage
-----

Custom Data Sources
...................

In most cases, you don't have to worry about creating an :csharp:`IncludeTree`. When using :ref:`CustomDataSources`, the structure of the :csharp:`.Include` and :csharp:`.ThenInclude` calls will be captured automatically and be turned into an :csharp:`IncludeTree`.

However, there are sometimes cases where you perform complex loading in these methods that involves loading data into the current :csharp:`DbContext` outside of the :csharp:`IQueryable` that is returned from the method. The most common situation for this is needing to conditionally load related data - for example, load all children of an object where the child has a certain value of a Status property.

In these cases, Coalesce provides a pair of extension methods, :csharp:`.IncludedSeparately` and :csharp:`.ThenIncluded`, that can be used to merge in the structure of the data that was loaded separately from the main :csharp:`IQueryable`.

For example:

    .. code-block:: c#

        public override IQueryable<Employee> GetQuery()
        {
            // Load all projects that are complete, and their members, into the db context.
            db.Projects
                .Include(p => p.EmployeeProjects).ThenInclude(ep => ep.Employee)
                .Where(p => p.Status == ProjectStatus.Complete)
                .Load();

            // Return an employee query, and notify Coalesce that we loaded the projects in a different query.
            return db.Employees.IncludedSeparately(e => e.EmployeeProjects)
                               .ThenIncluded(ep => ep.Project.EmployeeProjects)
                               .ThenIncluded(ep => ep.Employee);
        }


Model Methods
.............

If you have instance or static methods on your models that return objects, you may also want to control the structure of the returned data when it is serialized. Fortunately, you can also use :csharp:`IncludeTree` in these situations. Without an :csharp:`IncludeTree`, the entire object graph is traversed and serialized without limit.

To tell Coalesce about the structure of the data returned from a model method, simply add :csharp:`out IncludeTree includeTree` to the signature of the method. Inside your method, set :csharp:`includeTree` to an instance of an :csharp:`IncludeTree`. Obtaining an :csharp:`IncludeTree` is easy - take a look at this example:

    .. code-block:: c#

        public class Employee
        {
            public ICollection<Employee> GetChainOfCommand(AppDbContext db, out IncludeTree includeTree)
            {
                var ret = new List<Employee>();
                var current = this;
                while (current.Supervisor != null)
                {
                    ret.Push(current);
                    current = db.Employees
                        .Include(e => e.Supervisor)
                        .FirstOrDefault(e => e.EmployeeId == current.SupervisorId);
                }

                includeTree = db.Employees
                    .IncludedSeparately(e => e.Supervisor) 
                    .GetIncludeTree();

                return ret;
            }
        }

.. tip:: 

    An :csharp:`IncludeTree` can be obtained from any :csharp:`IQueryable` by calling the :csharp:`GetIncludeTree` extension method (:csharp:`using IntelliTect.Coalesce.Helpers.IncludeTree`).

    In situations where your root object isn't on your :csharp:`DbContext` (see :ref:`ExternalTypes`), you can use :csharp:`Enumerable.Empty<MyNonDbClass>().AsQueryable()` to get an :csharp:`IQueryable` to start from. When you do this, you **must** use :csharp:`IncludedSeparately` - the regular EF :csharp:`Include` method won't work without a :csharp:`DbSet`.

Without the outputted :csharp:`IncludeTree` in this scenario, the object graph recieved by the client would have ended up looking like this:
    
    .. code-block:: none

            - Steve's manager
                - District Supervisor
                    - VP
                        - CEO

            - District Supervisor
                - VP
                    - CEO

            - VP
                - CEO

            - CEO

Instead, with the :csharp:`IncludeTree`, we get the following, which is only the data we actually wanted:

    .. code-block:: none

            - Steve's manager
                - District Supervisor

            - District Supervisor
                - VP

            - VP
                - CEO

            - CEO

If you wanted to get even simpler, you could simply set the :csharp:`out includeTree` to a :csharp:`new IncludeTree()`, which would give you only the top-most level of data:

    .. code-block:: none

            - Steve's manager
            - District Supervisor
            - VP
            - CEO


.. _ExternalTypeIncludeTreeCaveats:

External Type Caveats
.....................

One important point remains regarding :csharp:`IncludeTree` - it is not used to control the serialization of objects which are not mapped to the database, known as :ref:`ExternalTypes`. External Types are always put into the DTOs when encountered (unless otherwise prevented by :ref:`DtoIncludesExcludesAttr` or :ref:`SecurityAttribute`), with the assumption that because these objects are created by you (as opposed to Entity Framework), you are responsible for preventing any undesired circular references.

By not filtering unmapped properties, you as the developer don't need to account for them in every place throughout your application where they appear - instead, they 'just work' and show up on the client as expected.

Note also that this statement does not apply to database-mapped objects that hang off of unmapped objects - any time a database-mapped object appears, it will be controlled by your include tree. If no include tree is present (because nothing was specified for the unmapped property), these mapped objects hanging off of unmapped objects will be serialized freely and with all circular references, unless you include some calls to :csharp:`.IncludedSeparately(m => m.MyUnmappedProperty.MyMappedProperty)` to limit those objects down.