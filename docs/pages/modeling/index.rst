
Intro to Modeling
=================

At its most fundamental level, Coalesce takes your data model as an input and produces a slew of generated code that facilitates interaction with this model as an output.

Coalesce supports a few different kinds of types that will comprise your model, but none of these are more essential than :ref:`EntityModels`. These are the types that EF Core maps your data to-and-from when interacting with your database, and they are the first models that you will create when building any application with Coalesce.

To get started building your EF Core :csharp:`DbContext` and related models, head on over to :ref:`EntityModels`.

Model Types
-----------
   
EF POCOs
~~~~~~~~

As mentioned above, EF Core database-mapped POCOs are the main type of model you will create, but they're certainly not the only one.
EF POCO objecs aren't the only kind of model that Coalesce will generate from.

