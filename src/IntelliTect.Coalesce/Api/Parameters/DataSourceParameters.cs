namespace IntelliTect.Coalesce.Api
{
    public class DataSourceParameters : IDataSourceParameters
    {
        /// <summary>
        /// An arbitrary string that will be used to perform property-level filtering
        /// using [DtoIncludesAttribute] and [DtoExcludesAttribute].
        /// Specifying "none" will prevent the default query behavior of including all immediate relations when using the StandardDataSource.
        /// </summary>
        public string Includes { get; set; }

        /// <summary>
        /// Data source to use for the list.
        /// </summary>
        public string DataSource { get; set; }
    }
}
