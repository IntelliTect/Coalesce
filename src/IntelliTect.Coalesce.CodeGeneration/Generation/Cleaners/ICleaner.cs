using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public interface ICleaner
    {
        IGenerator Owner { get; set; }

        string TargetPath { get; set; }

        /// <summary>
        /// Perform a cleanup with the consideration that the provided list of absolute paths to files should not be removed.
        /// </summary>
        /// <param name="generators">
        /// A collection of absolute paths to files that should not be modified.
        /// </param>
        /// <returns></returns>
        Task CleanupAsync(ICollection<string> knownGoodFiles);
    }
}
