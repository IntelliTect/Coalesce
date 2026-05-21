using System.IO;
using System.Threading.Tasks;
using System.Text;
using System;

namespace IntelliTect.Coalesce.CodeGeneration.Generation;

public abstract class StringBuilderFileGenerator<TModel> : FileGenerator, IFileGenerator<TModel>
{
    public StringBuilderFileGenerator(GeneratorServices services) : base(services) { }

    public TModel Model { get; set; }

    public abstract Task<string> BuildOutputAsync();

    public sealed override async Task<Stream> GetOutputAsync()
    {
        var output = await BuildOutputAsync();
        output = PrependHeaderComment(output);
        return new MemoryStream(Encoding.UTF8.GetBytes(output));
    }

    private string PrependHeaderComment(string output)
    {
        if (string.IsNullOrWhiteSpace(HeaderComment))
        {
            return output;
        }

        var commentPrefix = GetCommentPrefix();
        var lines = HeaderComment.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var commentedLines = new StringBuilder();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                commentedLines.AppendLine(commentPrefix.TrimEnd());
            }
            else
            {
                commentedLines.AppendLine(commentPrefix + line);
            }
        }

        commentedLines.AppendLine();
        return commentedLines.ToString() + output;
    }

    private string GetCommentPrefix()
    {
        var extension = Path.GetExtension(EffectiveOutputPath).ToLowerInvariant();
        return extension switch
        {
            ".cs" => "// ",
            ".ts" => "// ",
            ".js" => "// ",
            ".tsx" => "// ",
            ".jsx" => "// ",
            ".py" => "# ",
            ".java" => "// ",
            ".kt" => "// ",
            ".go" => "// ",
            ".rs" => "// ",
            ".rb" => "# ",
            ".php" => "// ",
            _ => "// " // Default to C-style comments
        };
    }

    public override string ToString()
    {
        if (EffectiveOutputPath != null)
        {
            return $"{GetType().FullName} => {EffectiveOutputPath}";
        }
        return GetType().FullName;
    }
}
