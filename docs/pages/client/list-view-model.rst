
.. _TypeScriptListViewModel:


TypeScript ListViewModels
-------------------------

In addition to :ref:`TypeScriptViewModel` for interacting with instances of your data classes in TypeScript, Coalesce will also generated a List ViewModel for loading searched, sorted, pagninated data from the server.

.. _Knockout: http://knockoutjs.com/

These ListViewModels, like the ViewModels, are dependent on Knockout_, and are designed to be used directly from Knockout bindings in your HTML.


Base Members
============


.. _TypeScriptListViewModelOrderBy:

- make sure to note that orderBy("none") will suppress default behavior.


THIS SEGMENT IS FROM THE SEARCH ATTRIBUTE DOCS. CONSIDER REMOVING IT FROM THERE AND MAKEING IT LIVE ONLY ON THIS PAGE.
- The :ts:`search` parameter of the API can also be formatted as ``PropertyName:SearchTerm`` in order to search on an arbitrary property of a model. For example, a value of ``Nickname:Steve-o`` for a search term would search the :csharp:`Nickname` property, even through it is not marked as searchable using this attribute.


Model-Specific Members
======================

    Static Method Members
        For each :ref:`Static Method <ModelMethods>` on your POCO, the members outlined in :ref:`Methods - Generated TypeScript <ModelMethodTypeScript>` will be created.