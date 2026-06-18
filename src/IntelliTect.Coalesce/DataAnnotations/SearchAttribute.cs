namespace IntelliTect.Coalesce.DataAnnotations;


/// <summary>
/// Marks a property as searchable in list search operations. When applied to a property, 
/// that property will be included in searches performed by the API list endpoint.
/// 
/// Use the <see cref="SearchMethods"/> enumeration to control how the search term is matched.
/// Use <see cref="RootWhitelist"/> and <see cref="RootBlacklist"/> to restrict searching based on the root model type.
/// Use <see cref="Includes"/> and <see cref="Excludes"/> to restrict searching based on the request's content view.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Property)]
public class SearchAttribute : System.Attribute
{
    public enum SearchMethods
    {
        /// <summary>
        /// Search term will be checked for at the beginning of the field's value in a case insensitive manner.
        /// </summary>
        BeginsWith = 1,

        /// <summary>
        /// Search term will be checked for anywhere inside the field's value in a case insensitive manner.
        /// </summary>
        Contains = 2,

        /// <summary>
        /// Search term must match the field exactly in a case insensitive manner.
        /// </summary>
        Equals = 3,

        /// <summary>
        /// <para>
        /// Search term must match exactly, using the natural casing handling of the evaluation environment.
        /// </para>
        /// 
        /// <para>
        /// Default database collation will be used if evaluated in SQL,
        /// and exact casing will be used if evaluated in memory.
        /// </para>
        /// 
        /// <para>
        /// This allows index seeks to be used instead of index scans,
        /// providing extra high performance searches against indexed columns.
        /// </para>
        /// </summary>
        EqualsNatural = 4,
    };

    /// <summary>
    /// If set to true (the default), each word in the search terms will be searched for in each searchable field independently.
    /// </summary>
    public bool IsSplitOnSpaces { get; set; } = true;

    /// <summary>
    /// <para>
    /// Specifies how string columns are searched. See individual enum members for details.
    /// </para>
    /// <para>
    /// Has no effect on non-string values. Numbers, GUIDs, and enums are always searched with exact values. 
    /// Dates are searched with lower and upper bounds if the user input could be parsed as a partial or complete date.
    /// </para>
    /// </summary>
    public SearchMethods SearchMethod { get; set; } = SearchMethods.BeginsWith;

    /// <summary>
    /// A comma-delimited list of model class names that, if set,
    /// will restrict the targeted property from being searched unless the
    /// root object of the API call was one of the specified class names.
    /// </summary>
    public string? RootWhitelist { get; set; }
    
    /// <summary>
    /// A comma-delimited list of model class names that, if set,
    /// will restrict the targeted property from being searched if
    /// the root object of the API call was one of the specified class names.
    /// </summary>
    public string? RootBlacklist { get; set; }

    /// <summary>
    /// A comma-delimited list of content views that, if set,
    /// will restrict the targeted property from being searched unless
    /// the request includes one of the specified content views.
    /// 
    /// <para>
    /// When this property is set, the property will only be searched if the API request 
    /// includes a matching content view in the <c>ContentView</c> parameter. 
    /// If this is empty or null, the property is searched regardless of the content view.
    /// </para>
    /// 
    /// <para>
    /// For example, if a property has <c>[Search(Includes = "details")]</c>, 
    /// it will only be searched when the request includes <c>?contentView=details</c>.
    /// Multiple content views can be specified as a comma-delimited list: <c>Includes = "details, admin"</c>.
    /// </para>
    /// </summary>
    public string? Includes { get; set; }

    /// <summary>
    /// A comma-delimited list of content views that, if set,
    /// will restrict the targeted property from being searched if
    /// the request includes one of the specified content views.
    /// 
    /// <para>
    /// When this property is set, the property will not be searched if the API request 
    /// includes a matching content view in the <c>ContentView</c> parameter.
    /// If this is empty or null, the property is searched regardless of the content view.
    /// </para>
    /// 
    /// <para>
    /// For example, if a property has <c>[Search(Excludes = "preview")]</c>, 
    /// it will not be searched when the request includes <c>?contentView=preview</c>.
    /// Multiple content views can be specified as a comma-delimited list: <c>Excludes = "preview, summary"</c>.
    /// </para>
    /// 
    /// <para>
    /// Note: If both <see cref="Includes"/> and <see cref="Excludes"/> are specified, <see cref="Includes"/> takes precedence.
    /// </para>
    /// </summary>
    public string? Excludes { get; set; }
}
