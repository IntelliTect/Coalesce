
Search
======

Coalesce supports searching through the generated API in its various implementations, including the generated list views (Table & Cards), in Select2 dropdowns, and directly through the TypeScript ListViewModels' :ts:`search` property.

The :ts:`search` parameter of the API can also be formatted as ``PropertyName:SearchTerm`` in order to search on an arbitrary property of a model. For example, a value of ``Nickname:Steve-o`` for a search term would search the :csharp:`Nickname` property, even through it is not marked as searchable using this attribute.

By default,
the system will search any field with the name 'Name'. If this doesn't
exist, the ID is used as the only searchable field.

To customize the default behavior of searching, the Search attribute can be placed on any property on your model to make that field searchable.

Search attributes can even be placed on objects. In this case, the
searchable fields of the child object will be used in the search.

Example Usage
-------------

    .. code-block:: c#

        public class Person
        {
            public int PersonId { get; set; }

            [Search(IsSplitOnSpaces = true)]
            public string FirstName { get; set; }

            [Search(IsSplitOnSpaces = true)]
            public string LastName { get; set; }

            public string Nickname { get; set; }
        }

Properties
----------

    :csharp:`public bool IsSplitOnSpaces { get; set; } = true;`
        If set to true (the default), each word in the search terms will be searched for in each searchable field independently.
        
        This is useful when searching for a full name across two or more fields. In the above example, using :csharp:`IsSplitOnSpaces = true` would provide more intuitive behavior since it will search both first name and last name for each word entered into the search field.

    :csharp:`public SearchMethods SearchMethod { get; set; } = SearchMethods.BeginsWith;`
        Specifies whether the value of the field will be checked using :csharp:`Contains` or using :csharp:`BeginsWith`.
        
        Note that standard database indexing can be used to speed up :csharp:`BeginsWith` searches. 