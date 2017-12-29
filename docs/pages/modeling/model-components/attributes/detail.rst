
[Detail]
========

Provides a detail field. Can be used to expose a tooltip or hint in HTML.

Example Usage
-------------

    .. code-block:: c#

        public class Person
        {
            public int PersonId { get; set; }
            
            [Detail("i.e., asthma, reflux, eczema")]
            public string ChronicHealthProblems { get; set; }
        }