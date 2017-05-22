
File Download
=============

Specifies that this property should be exposed as a file download.

Example Usage
-------------

    .. code-block:: c#

        public class Signature
        {
            public int SignatureId { get; set; }
            
            public byte[] Content { get; set; }
            
            [NotMapped] [FileDownload]
            public Image Image
            {
                get
                {
                    MemoryStream ms = new MemoryStream(Content);
                    Image returnImage = Image.FromStream(ms);
                    return returnImage;
                }
                set
                {
                    MemoryStream ms = new MemoryStream();
                    value.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                    Content = ms.ToArray();
                }
            }
        }