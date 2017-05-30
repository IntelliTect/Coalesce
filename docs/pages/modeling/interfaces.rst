

Interfaces
==========

Coalesce provides a set of interfaces that aid in the implementation of business rules.
These can be implemented in the model or in the controllers. The models
use interfaces, the controllers use override methods. One advantage to
Coalesce is that by default all changes to tables come through a common
set of APIs allowing for centralized control of business rules.

The Before methods return ValidateResult_ objects. The default
constructor creates a successful version of this object making a
successful return result easy.

|
ISaveCustomizable
-------------------

The :csharp:`ISaveCustomizable<T, TContext> where TContext : DbContext` interface is simply a wrapper for both :csharp:`IBeforeSave<T, TContext>` and :csharp:`IAfterSave<T, TContext>`.

If you only need one of these interfaces, reference them individually.


IBeforeSave
-----------

    .. code-block:: c#

        public interface IBeforeSave<T, TContext> where TContext: DbContext
        {
            ValidateResult<T> BeforeSave(T original, TContext db, ClaimsPrincipal user, string includes);
        }

The :csharp:`IBeforeSave` interface exposes a :csharp:`BeforeSave` method that is called before changes are persisted to the database. It must return a ValidateResult_ object, allowing for the save to be canceled and a message to be passed back to the client.

This method is called on the current instance of the item after it has been mapped from the incoming DTO. Included in the call is a shallow copy of the original object before the DTO values are set. Also supplied is the :csharp:`DbContext`, the current user's :csharp:`ClaimsPrincipal`, and the :csharp:`includes` string (see :ref:`Includes`). This allows for virtually limitless customization of validation rules.


IAfterSave
----------

    .. code-block:: c#

        public interface IAfterSave<T, TContext> where TContext : DbContext
        {
            void AfterSave(T original, TContext db, ClaimsPrincipal user, string includes);
        }

This method is called after the save is complete. It is called on an instance of the newly saved object. Included in the call is a shallow copy of the original object before the DTO values are set. Also supplied is the :csharp:`DbContext`, the current user's :csharp:`ClaimsPrincipal`, and the :csharp:`includes` string (see :ref:`Includes`).

A typical use would be to update related data in other parts of the model.


|
IDeleteCustomizable
-------------------

The :csharp:`IDeleteCustomizable<TContext> where TContext : DbContext` interface is simply a wrapper for both :csharp:`IBeforeDelete<TContext>` and :csharp:`IAfterDelete<TContext>`.

If you only need one of these interfaces, reference them individually.

IBeforeDelete
-------------

    .. code-block:: c#

        public interface IBeforeDelete<TContext> where TContext : DbContext
        {
            ValidateResult BeforeDelete(TContext db, ClaimsPrincipal user);
        }

Called before the delete is attempted, this method allows for delete
validation and related object cleanup. This method is called on the
object being deleted and includes the :csharp:`DbContext` and the current user's :csharp:`ClaimsPrincipal`. This is useful for not allowing deletes in certain
circumstances.

It provides an opportunity to remove related objects from the
model to ensure the delete is not blocked by key constraints when
cascading deletes are turned off. This returns a ValidateResult_, allowing for the delete to be canceled and a message to be passed back to the client.

IAfterDelete
------------

    .. code-block:: c#

        public interface IAfterDelete<TContext> where TContext : DbContext
        {
            void AfterDelete(TContext db, ClaimsPrincipal user);
        }

Like IAfterSave, this method is called after the delete was successful.
It is called on the newly deleted object and includes the :csharp:`DbContext` and the current user's :csharp:`ClaimsPrincipal`. This can be used for additional cleanup of related
objects.

|
.. _ValidateResult:

ValidateResult
--------------

:csharp:`ValidateResult`, and its generic cousin :csharp:`ValidateResult<T>` are used by both of the 'Before' interfaces, as well as some other extension points throughout Coalesce.

Its purpose is to provide both feedback to Coalesce and feedback to the user. By setting :csharp:`WasSuccessful = false`, Coalesce will stop going forward with its current action, whether that be a create, update, or delete. By setting its :csharp:`Message` property, feedback can be passed back to the user to explain what the issue was.

For convenience, there are implicit conversions to :csharp:`ValidateResult` from both :csharp:`bool` and :csharp:`string`. Returning a :csharp:`string` from :csharp:`IBeforeSave` will set :csharp:`WasSuccessful = false` and set :csharp:`Message` to that string. Simply returning :csharp:`true` will indicate that everything went as planned and that Coalesce may continue with the current action.