
.. _ManyToMany:

[ManyToMany]
============

Used to specify a Many to Many relationship. Because EF core does not
support automatic intermediate mapping tables, this field is used to
allow for direct reference of the many-to-many collections from the
ViewModel.

The named specified in the attribute will be used as the name of a collection of the objects on the other side of the relationship in the generated :ref:`TypeScriptViewModels`. 

Example Usage
-------------

.. code-block:: c#

    public class Person
    {
        public int PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [ManyToMany("Appointments")]
        public ICollection<PersonAppointment> PersonAppointments { get; set; }
    }

Properties
----------

`public string CollectionName { get; }` :ctor:`1`
    The name of the collection that will contain the set of objects on the other side of the many-to-many relationship.