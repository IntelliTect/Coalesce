namespace IntelliTect.Coalesce.Validation;

internal class ValidationResult
{
    public bool WasSuccessful { get; set; }
    public bool IsWarning { get; set; }
    public string? Area { get; set; }
    public string? Message { get; set; }

    public bool IsError => !WasSuccessful && !IsWarning;

    public override string ToString()
    {
        if (WasSuccessful)
        {
            return $"  Success: {Area}: {Message}";
        }
        if (IsWarning)
        {
            return $"-- Warning: {Area}: {Message}";
        }
        return $"** Failure: {Area}: {Message}";
    }
}
