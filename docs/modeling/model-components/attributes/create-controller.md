
[CreateController]
==================

By default an API and View controller are both created. This allows for
suppressing the creation of either or both of these.


Example Usage
-------------

.. code-block:: c#

    [CreateController(view: false, api: true)]
    public class Person
    {
        public int PersonId { get; set; }
        
        ...
    }


Properties
----------

`public bool WillCreateView { get; set; }` :ctor:`1`

`public bool WillCreateApi { get; set; }` :ctor:`2`
