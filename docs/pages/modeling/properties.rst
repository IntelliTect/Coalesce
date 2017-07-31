
.. _ModelProperties:

Properties
==========

Models in a Coalesce application are just EF Core POCOs. The properties defined on your models should fit within the constraints of EF Core.

Coalesce currently has a few more restrictions than what EF Core allows, but hopefully over time some of these restrictions can be relaxed as Coalesce grows in capability.


The following kinds of properties may be declared on your models.

    Primary Key
        To work with Coalesce, your model must have a single property for a primary key. By convention, this property should be named the same as your model class with :code:`Id` appended to that name. Currently, only :csharp:`int` is supported as a primary key type.

    TODO: finish writing this. I stopped when the idea for Models 2.0 came to mind.




| 

Calculated Fields
^^^^^^^^^^^^^^^^^

Calculated fields can be easily added to your model. These do not get
stored in the database and should be marked with the [NotMapped]
attribute. See example above.