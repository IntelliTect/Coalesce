
.. _GenDTOs:

Data Transfer Objects (DTOs)
----------------------------

Data Transfer Objects, or DTOs, allow for transformations of data from the data store into a format more suited for transfer and use on the client side. This often means trimming properties and flattening structures to provide a leaner over-the-wire experience. Coalesce aims to support this as seamlessly as possible.

Coalesce supports several types of DTOs:

-  DTOs that are automatically generated for each POCO database object.
   These are controlled via :ref:`ModelAttributes` on the POCO. These are outlined below.
-  DTOs that you create with IClassDto and create unique ViewModels. These are outlined at :ref:`CustomDTOs`.


Automatically Generated DTOs
~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Every class that is exposed through Coalesce's generated API will have a corresponding DTO generated for it. These DTOs are used to
shuttle data back and forth to the client. They are generated classes
that have nullable versions of all the properties on the POCO class, and are the mechanism by which almost all the techniques described in :ref:`ControllingLoading` function.

:ref:`DtoIncludesExcludesAttr` and the :ref:`Includes` infrastructure can be used to indicate which properties should be transferred to the client in which cases, and :ref:`IncludeTree` is used to dictate how these DTOs are constructed from POCOs retrieved from the database.

