using Microsoft.VisualStudio.Web.CodeGeneration;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public interface IGenerationOutput
    {
        bool ShouldPersist(IFileSystem fileSystem);

        void Persist(IFileSystem fileSystem);
    }
}
