// Explicitly in IntelliTect.Coalesce to simplify typical using statements 
namespace IntelliTect.Coalesce
{
    public interface IDataSourceParameters
    {
        string Includes { get; }

        string DataSource { get; }
    }
}
