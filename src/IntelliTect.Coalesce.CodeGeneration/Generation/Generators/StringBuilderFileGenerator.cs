using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Generation;

public abstract class StringBuilderFileGenerator<TModel> : FileGenerator, IFileGenerator<TModel>
{
    public StringBuilderFileGenerator(GeneratorServices services) : base(services) { }

    public TModel Model { get; set; }

    public abstract Task<string> BuildOutputAsync();

    public sealed override async Task<Stream> GetOutputAsync()
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(await BuildOutputAsync()));
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
