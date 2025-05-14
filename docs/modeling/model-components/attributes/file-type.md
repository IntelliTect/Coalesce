
# [FileType]

`IntelliTect.Coalesce.DataAnnotations.FileTypeAttribute`

Specify the allowed file types for a file [method parameter](../methods.md#parameters). The value is passed through to the "accept" attribute of an [HTML File Input](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference/Elements/input/file#accept). This does not implement any server-side validation of file content.


## Example Usage

``` c#
public class Person
{
    public int PersonId { get; set; }

    public async Task UploadProfilePicture([FileType("image/*")] IFile file) 
    {

    }
}
```

## Properties
<Prop def="public string FileTypes { get; set; }" ctor=1 />
    
Comma-delimited list of [Unique file type specifiers](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference/Elements/input/file#unique_file_type_specifiers).

Examples:
- `"image/*"` - Common image formats
- `.pdf,application/pdf` - PDF Files
- `.doc,.docx,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document` - Microsoft Word Documents

