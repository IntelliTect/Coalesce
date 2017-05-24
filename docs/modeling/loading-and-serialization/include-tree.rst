


.. _IncludeTree:

Include Tree
============

When Coalesce maps from the your POCO objects that are returned from EF Core queries, it will follow a structure called an :csharp:`IncludeTree` to determine what relationships to follow and how deep to go in re-creating that structure in the mapped DTOs.

Purpose
-------

Without an :csharp:`IncludeTree` present, Coalesce will map the entire object graph that is reachable from the root object.

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
            employee.dataSource = employee.dataSources.WithProjectsAndMembers;
            employee.load(1);

    If you're already familiar with the fact that an :csharp:`IncludeTree` is implicitly created in this scenario, then imagine for a moment that this is not the case (if you're not familiar with this fact, then keep reading!).

    After Coalesce has called your :ref:`CustomDataSources` and evaluated the EF IQueryable, there are now 35 objects loaded into the current :csharp:`DbContext` being used to handle this request - the 5 employees, 5 projects, and 25 relationships.

    To map these objects to DTOs for serialization to JSON, we start with the root, employee #1, and expand outward from there until the entire object graph has been faithfully re-created with DTO objects, including all navigation properties.

    The root DTO object (employee #1) then evetually is passed to the JSON serializer by ASP.NET Core to formulate the response to the request. As the object is serialized to JSON, the only objects that are not serialized are those that were already serialized as an ancestor of itself. What this ultimately means is that the structure of the serialized JSON with our example scenario ends up following a pattern like this (the vast majority of items have been omitted):

        .. code-block:: none

            Employee#1
                EmployeeProject#1
                    Project#1
                        EmployeeProject#6
                            Employee#2
                                EmployeeProject#7
                                    Project#2
                                EmployeeProject#8
                                    Project#3
                                EmployeeProject#9
                                    Project#4
                                EmployeeProject#10
                                    Project#5
                        EmployeeProject#11
                            Employee#3
                        EmployeeProject#16
                            Employee#4
                        EmployeeProject#21
                            Employee#5
                EmployeeProject#2
                    Project#2
                EmployeeProject#3
                    Project#3
                EmployeeProject#4
                    Project#4
                EmployeeProject#5
                    Project#5
    
    See how the structure includes the EmployeeProjects of Employee#2? We didn't write our custom data source calls to :csharp:`.Include` in such a way that indicated that we wanted the root employee, their projects, the employees of those projects, and then **the projects of those employees**. But, because the JSON serializer blindly follows the object graph, that's what gets serialized. It turns out that the depth of the JSON increases like :code:`O(n!)`.

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
                        EmployeeProject#16
                            Employee#4
                        EmployeeProject#21
                            Employee#5
                EmployeeProject#2
                    Project#2
                EmployeeProject#3
                    Project#3
                EmployeeProject#4
                    Project#4
                EmployeeProject#5
                    Project#5

    No more extra data trailing off the end of the projects' employees!


Usage
-----

IIncludable & Custom Data Sources
.................................

In most cases, you don't have to worry about creating an :csharp:`IncludeTree`. When using :ref:`CustomDataSource`, the structure of the :csharp:`.Include` and :csharp:`.ThenInclude` calls will be captured automatically and be turned into an :csharp:`IncludeTree`. Similarly, when using :ref:`IIncludable`, Coalesce will build an :csharp:`IncludeTree` from the returned :csharp:`IQueryable`.

However, there are sometimes cases where you perform complex loading in these methods that involves loading data into the current :csharp:`DbContext` outside of the :csharp:`IQueryable` that is returned from the method. The most common situation for this is needing to conditionally load related data - for example, load all children of an object where the child has a certain value of a Status property.

In these cases, Coalesce provides a pair of extension methods, :csharp:`.IncludedSeparately` and :csharp:`.ThenIncluded`, that can be used to merge in the structure of the data that was loaded separately from the main :csharp:`IQueryable`.

For example:

    .. code-block:: c#

        public static IQueryable<Employee> WithCompleteProjectsAndMembers(AppDbContext db, ClaimsPrincipal user)
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

If you have instance or static methods on your models that return objects, you may also want to control the structure of the returned data when it is serialized. Fortunately, you can also use :csharp:`IncludeTree` in these situations. Without an :csharp:`IncludeTree`, the entire object graph is traversed and serialized blindly.

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
                    // IncludedSeparately is required - the EF Core .Include() method won't work without starting from a DbSet
                    .IncludedSeparately(e => e.Supervisor) 
                    .GetIncludeTree();

                return ret;
            }
        }

.. tip:: 

    An :csharp:`IncludeTree` can be obtained from any :csharp:`IQueryable` by calling the :csharp:`GetIncludeTree` extension method (:csharp:`using IntelliTect.Coalesce.Helpers.IncludeTree`).

    In situations where your root object isn't on your :csharp:`DbContext` (see :ref:`ExternalTypes`), you can use :csharp:`Enumerable.Empty<MyNonDbClass>().AsQueryable()` to get an :csharp:`IQueryable` to start from.

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