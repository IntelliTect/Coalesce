
.. _HiddenAttribute:

[Hidden]
========

Mark an property as hidden from the edit, List or All areas.

.. caution::

    This attribute is **not a security attribute** - it only affects the rendering of the admin pages. It has no impact on data visibility in the API.

    Do not use it to keep certain data private - use the :ref:`SecurityAttribute` family of attributes for that.
   

Example Usage
-------------

.. code-block:: c#

    public class Person
    {
        public int PersonId { get; set; }

        [Hidden(HiddenAttribute.Areas.All)]
        public int? IncomeLevelId { get; set; }
    }

Properties
----------
:csharp:`public Areas Area { get; set; }` :ctor:`1`
    The areas in which the property should be hidden.

    Enum values are:
        - :csharp:`HiddenAttribute.Areas.All` Hide from all generated views
        - :csharp:`HiddenAttribute.Areas.List` Hide from generated list views only (Table/Cards)
        - :csharp:`HiddenAttribute.Areas.Edit` Hide from generated editor only (CreateEdit)

