
Modeling
========

Model Overview
--------------

Models are the core business objects of your application. More specifically, they
are the fundamental representation of data in your application. The design of your
models is very important. In `Entity Framework Core`_, data models are just
Plain Old CLR Classes (POCOs).

.. _Entity Framework Core:
.. _EF Core:
.. _EF:
    https://docs.microsoft.com/en-us/ef/core/



Building a Data Model
---------------------

To build your data model that Coalesce will generate code for, follow the best practices for `EF Core`_.

Guidance on this topic is available in abundance in the `Entity Framework Core`_ documentation.

Don't worry about querying or saving data for now - Coalesce will provide a lot of that functionality for you. To get started, just build your POCOs and :csharp:`DbContext` classes.


Customizing Your Data Model
---------------------------

Once you have built out a simple POCO data model, you can get started on the fun part - customizing it.

Coalesce includes a number of ways in which you can cutomize your data model. Cutomizations affect the generated API and the generated views.


.. toctree::
    :maxdepth: 2

    properties
    attributes
    interfaces
    methods
    loading-and-serialization

| 

Controlling Object Loading
----------------

Static List Methods
^^^^^^^^^^^^^^^^^^^


Using Includable
^^^^^^^^^^^^^^^^



| 

**The generated admin views have documentation pages that include all
view model documentation specific to each class.**

Model Business Rules
~~~~~~~~~~~~~~~~~~~~


--------------

Other Attributes
================

Validation
^^^^^^^^^^

Validation is handled using standard MVC attributes. These attributes
are not only enforced on the server side in the database, but are also
passed to the client and enforced using the KnockoutValidation_ library.
There is also flexible annotation-based validation for the client side.
Full validation documentation is in the Annotations section

| 

Display and Order
^^^^^^^^^^^^^^^^^

The `display </Docs/Annotations#Display>`__ name of a field can be set
via the DisplayName attribute. The display name and field order can be
set via the Display attribute using the Name and Order properties. This
only impacts the order of fields in the admin pages and pop-up editors.
By default, the fields will be in the order they are found in the class.

::


        [Display(Name = "Name", Order = 1)]
        public string TheFullName { get; set; }

| 


| 

Calculated Fields
^^^^^^^^^^^^^^^^^

Calculated fields can be easily added to your model. These do not get
stored in the database and should be marked with the [NotMapped]
attribute. See example above.

| 

Security
^^^^^^^^

Security is handled via attributes on the class, properties, and
methods.

Methods
~~~~~~~

Any public methods you place on your POCO classes will get built into
your view models. There are two main types of methods: instance and
static.

For each method several TypeScript methods will be created. Additionally
if there are arguments, an class with the type of [name]Args will be
created. The main TypeScript will have a property for each of these
classes that can be easily bound to.

The following additional parameters can be added to your methods:

-  If a parameter is marked with the [Inject] attribute, it will be
   injected from the application's IServiceProvider.
-  If the method has a parameter of type ClaimsPrincipal, the current
   user will be added to the method call automatically. This does not
   appear on the client side.
-  If the method has a parameter of the same type as your DbContext
   class, the current DbContext will be passed to the method.
-  If the method has an out parameter of type IncludeTree, then the
   IncludeTree that is passed out will be used to control serialization.
   See `the DTO docs for more information about
   IncludeTree. </Docs/Dtos>`__

Instance Methods
^^^^^^^^^^^^^^^^

Instance methods exist on an instance of an object and should use
information contained in the object during the execution of the method.
These methods are created as functions on the object's view model on the
client side. The client view model will actually have several observable
properties created for you. The example below is for a method called
Move that takes a single parameter 'int feet' and returns a string.

::

        public string Move(int feet){
            return "I moved " + feet.ToString();
        }

move
    Function that takes a number and a callback. This callback function
    is called when the call is complete.
moveWithArgs
    Function that takes an object that contains all the parameters.
    Object is of type [Name]Args which is included as a sub-class on
    this class. If null, the built in instance of this class will be
    used. This is named [name]Args
moveResult
    Observable with the result of the method call. This can be data
    bound to show the result.
moveIsLoading
    Observable boolean which is true while the call to the server is
    happening.
moveMessage
    If the method was not successful, this contains exception
    information.
moveWasSuccessful
    Observable boolean with true if the method was successful, or false
    if an exception occurred.
moveUi
    Simple interface using JavaScript input boxes to prompt the user for
    the required data for the method call. The call is then made with
    the data provided.
moveModal
    Simple modal interface to prompt the user for the required data for
    the method call. The call is then made with the data provided.

| 

Static Methods
^^^^^^^^^^^^^^

Static methods exist on a class without respect to an instance of that
class. These methods are created as functions on the object's **list**
view model on the client side. The example below is for a method called
NameStartingWith that takes a single parameter 'string characters' and
returns a list of strings. The DbContext parameter is injected
automatically by the controller.

::

        public static IEnumerable<string> NamesStartingWith(string characters, DbContext db)
        {
            return db.People.Where(f => f.FirstName.StartsWith(characters)).Select(f => f.FirstName).ToList();
        }

namesStartingWith
    Function that takes a string and a callback. This callback function
    is called when the call is complete.
namesStartingWithWithArgs
    Function that takes an object that contains all the parameters.
    Object is of type [Name]Args which is included as a sub-class on
    this class. If null, the built in instance of this class will be
    used. This is named [name]Args
namesStartingWithResult
    Observable with the result of the method call. This can be data
    bound to show the result.
namesStartingWithIsLoading
    Observable boolean which is true while the call to the server is
    happening.
namesStartingWithMessage
    If the method was not successful, this contains exception
    information.
namesStartingWithWasSuccessful
    Observable boolean with true if the method was successful, or false
    if an exception occurred.
namesStartingWithUi
    Simple interface to prompt the user for the required data for the
    method call. The call is then made with the data provided.
namesStartingWithModal
    Simple modal interface to prompt the user for the required data for
    the method call. The call is then made with the data provided.
