// Explicitly in IntelliTect.Coalesce to simplify typical using statements 
namespace IntelliTect.Coalesce
{
    public interface IDataSourceParameters
    {
        /// <summary>
        /// The "include" string specified by the client.
        /// Used primarily for property-level DTO trimming.
        /// Specifying "none" will prevent the default query behavior of 
        /// including all immediate relations when using the StandardDataSource.
        /// </summary>
        string Includes { get; }

        /// <summary>
        /// The name of the data source that was requested by the client.
        /// This could be different than the name of the data source that gets resolved
        /// (in the event of overridden default data sources).
        /// </summary>
        string DataSource { get; }
    }
}
