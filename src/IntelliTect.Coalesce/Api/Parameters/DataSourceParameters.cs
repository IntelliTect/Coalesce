namespace IntelliTect.Coalesce.Api;

public class DataSourceParameters : IDataSourceParameters
{
    /// <inheritdoc />
    public string? Includes { get; set; }

    /// <inheritdoc />
    public string? DataSource { get; set; }
}
