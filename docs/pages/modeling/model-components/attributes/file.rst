.. _FileAttribute

[File]
========

Coalesce supports the uploading and downloading of files and images through the :csharp:`IntelliTect.Coalesce.DataAnnotations.File` annotation. 

Example Usage
-------------
    .. code-block:: c#

        public class Product
        {
            public int ProductId { get; set; }

            [Search]
            public string ProductName { get; set; }

            [File("text/plain")]
            public byte[] ProductDescription { get; set; }

            [File("image/jpeg")]
            public byte[] ProductThumbnail { get; set;}

            [File]
            public byte[] BinaryDriverFile { get; set;}

            //...
        
Further we may specify file information in the optional parameters on the attribute tag, or even specify security levels with the appropriate :ref:`SecurityAttribute`. In the example below the file can be viewed/downloaded by any user, but only users with the role of :csharp:`"Admin"` or :csharp:`"Vendor"` will be able to upload a new file.

    
    .. code-block:: c#

            //...
            [Read]
            [Edit(Roles = "Admin,Vendor")]
            [File("video/mp4", nameof(VideoName), nameof(VideoHash), nameof(VideoSize))]
            public byte[] InstallationVideo { get; set; }
            public string VideoName { 
                get {
                    return $"Product{ProductId}.mp4";
                }
            }
            public long VideoSize { get; set; }
            public string VideoHash { get; set; }
        }

Properties
----------

    .. _MimeTypeReference: https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types#applicationoctet-stream
    __ MimeTypeReference_

    :csharp:`public string MimeType { get; set; }`
        The system will identify the nature and format of the file using the type described in the 'MimeType' property. By default it is set to :csharp:`"application/octet-stream"` to read/write the file as an `unknown binary file`__. 
        
        An image preview will be displayed for properties annotated with the :csharp:`File` attribute whose MIME type contains :csharp:`"image"`. Other MIME types will result in a download button being displayed.

        .. tip::
            If a filename exists on the uploaded file, the MIME type may inferred from the file extension and :csharp:`MimeType` need not be specified.

    :csharp:`public string NameProperty { get; set; }`
        If set, will specify the filename. If this is readonly this will return a computed filename. If the property is settable, the value of the property will be set upon file upload.

    :csharp:`public string HashProperty{ get; set; }`
        The name of the property to store the hash of :csharp:`Byte[]`. This is set upon file upload.

    :csharp:`public string SizeProperty { get; set; }`
        A property to store the size of the file into. This is set upon file upload.