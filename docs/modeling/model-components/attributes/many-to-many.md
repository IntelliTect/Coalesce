
# [ManyToMany]

`IntelliTect.Coalesce.DataAnnotations.ManyToManyAttribute`

Used to specify a Many to Many relationship. Because EF core does not
support automatic intermediate mapping tables, this field is used to
allow for direct reference of the many-to-many collections from the
ViewModel.

The named specified in the attribute will be used as the name of a collection of the objects on the other side of the relationship in the generated [TypeScript ViewModels](/stacks/vue/layers/viewmodels.md#model-data-properties). 

## Example Usage
In this example, we have a Person entity and an Appointment entity that share a many-to-many relationship. The PersonAppointment entity serves as the required middle table.
``` c#
public class Person
{
    public int PersonId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    [ManyToMany("Appointments")]
    public ICollection<PersonAppointment> PersonAppointments { get; set; }
}

public class Appointment
{
    public int AppointmentId { get; set; }
    public DateTime AppointmentDate { get; set; }

    [ManyToMany("People")]
    public ICollection<PersonAppointment> PersonAppointments { get; set; }
}

public class PersonAppointment
{
    public int PersonAppointmentId { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; }

    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; }
}
```

## Properties

<Prop def="public string CollectionName { get; }" ctor="1" />

The name of the collection that will contain the set of objects on the other side of the many-to-many relationship.


<Prop def="public string FarNavigationProperty { get; set; }" />

The name of the navigation property on the middle entity that points at the far side of the many-to-many relationship. Use this to resolve ambiguities when the middle table of the many-to-many relationship has more than two reference navigation properties on it.

``` c#
public class Person
{
    ...
    
    [ManyToMany("Appointments", FarNavigationProperty = nameof(PersonAppointment.Appointment))]
    public ICollection<PersonAppointment> PersonAppointments { get; set; }
}

public class Appointment
{
    ...

    [ManyToMany("People", FarNavigationProperty = nameof(PersonAppointment.Person))]
    public ICollection<PersonAppointment> PersonAppointments { get; set; }
}

public class PersonAppointment
{
    ...

    // Adding a third reference navigation property in the middle table requires 
    // the use of FarNavigationProperty in order to resolve ambiguity.
    public int WaiverId { get; set; }
    public Waiver Waiver { get; set; }
}
```