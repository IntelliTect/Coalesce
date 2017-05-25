
Data Modeling
=============

Overview
--------

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

**The generated admin views have documentation pages that include all
view model documentation specific to each class.**


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
