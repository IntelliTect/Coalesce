@{ ViewBag.Title = "Coalesce"; Layout = "\_DocsLayout"; }

Code Generation
---------------

UI Components
~~~~~~~~~~~~~

User interfaces created with Coalesce are very flexible. From the
pre-built admin screens, to various helpers, to straight HTML there is a
way to easily construct your page. These helpers use Knockout and
Bootstrap as the primary UI elements. Select2 and the Bootstrap Date
Time Picker are used for select and date inputs respectively.

Coalesce comes with a library of easy-to-use data binding helpers. These
allow you to quickly create high-quality user experiences. These are
found in the IntelliTect.Coalesce.Helpers namespace.

Display
~~~~~~~

The Display helper class has a high level method for displaying
properties. When called, this method will return a display or an editor
for the specified property. By default this uses attributes on the model
to determine how to display the values. This internally uses the
Knockout helper library.

Knockout
~~~~~~~~

The Knockout helper class has a wide range of helpers to display
different property types. They have many options for display,
functionality, and responsive design. Most of them have several
versions.

-  A pure version that is just the control. ``TextInput``
-  A version that is combined with a label with spacing options.
   ``TextInputWithLabel``
-  A version that is combined with a label and uses a property selector.
   ``InputWithLabelFor<Person>(f=>f.FirstName)``
-  A pure version that uses a property selector.
   ``InputFor<Person>(f=>f.FirstName)``

Some versions of the extensions take a PropertyViewModel. This is an
internal type definition class that uses reflection to enumerate your
object model. See the Server View Models section for more information.

Available Extensions
~~~~~~~~~~~~~~~~~~~~

The follow are general types of extensions provided.

-  Text input boxes
-  Selection boxes for objects with dynamic search
-  Selection boxes for many-to-many relationships
-  Selection boxes for text input that search for existing values
-  Selection boxes for enumerations
-  Date input boxes that use a date and time picker
-  Boolean selection check boxes
-  Expand buttons for child view models
-  Showing modified date in human readable time like '2 hours ago'
-  Display of text, objects, and many-to-many data

Additional Helpers
^^^^^^^^^^^^^^^^^^

The AddFormGroup extension wraps your other helper calls in a Bootstrap
Form Group.

AddLabel gets a Bootstrap formatted label

The Knockout class has two properties to control the default number of
columns given to label and display/edit areas. They are:
DefaultLabelCols and DefaultInputCols.
