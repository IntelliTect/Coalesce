.. _ControllerActionAttribute:

[ControllerAction]
==================

Specifies how a :ref:`custom method <ModelMethods>` is exposed via HTTP. Can be used to customize the HTTP method/verb for the method, as well as caching behavior.


Example Usage
-------------

.. code-block:: c#

    public class Person
    {
        public int PersonId { get; set; }
        public string LastName { get; set; }

        public string PictureHash { get; set; }

        [Coalesce]
        [ControllerAction(Method = HttpMethod.Get)]
        public static long PersonCount(AppDbContext db, string lastNameStartsWith = "")
        {
            return db.People.Count(f => f.LastName.StartsWith(lastNameStartsWith));
        }

        [Coalesce]
        [ControllerAction(HttpMethod.Get, VaryByProperty = nameof(PictureHash))]
        public IFile GetPicture(AppDbContext db)
        {
            return new IntelliTect.Coalesce.Models.File(db.PersonPictures
                .Where(x => x.PersonId == this.PersonId)
                .Select(x => x.Content)
            )
            {
                ContentType = "image/jpg",
            };
        }
    }

Properties
----------

:csharp:`public HttpMethod Method { get; set; }` :ctor:`1`
    The HTTP method to use on the generated API Controller.

    Enum values are:
        - :csharp:`HttpMethod.Post` Use the POST method. (default)
        - :csharp:`HttpMethod.Get` Use the GET method.
        - :csharp:`HttpMethod.Put` Use the PUT method.
        - :csharp:`HttpMethod.Delete` Use the DELETE method.
        - :csharp:`HttpMethod.Patch` Use the PATCH method.

:csharp:`public string VaryByProperty { get; set; }`
    For HTTP GET model instance methods, if :csharp:`VaryByProperty` is set to the name of a property on the parent model class, `ETag headers <https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/ETag>`_ based on the value of this property will be used to implement caching. If the client provides a matching `If-None-Match` Header with the request, the method will not be invoked and HTTP Status `304 Not Modified`` will be returned.

    Additionally, if the :csharp:`VaryByProperty` is set to a client-exposed :ref:`property <ModelProperties>`, the value of the property will be included in the query string when performing API calls to invoke the method. If the query string value matches the current value on the model, a long-term `Cache-Control` header will be set on the response, allowing the client to avoid making future invocations to the same method while the value of the :csharp:`VaryByProperty` remains the same.