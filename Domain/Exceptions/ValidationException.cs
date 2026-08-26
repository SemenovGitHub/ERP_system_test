namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when request validation fails
/// </summary>
public sealed class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }

    public ValidationException(string message, IReadOnlyDictionary<string, string[]> validationErrors)
        : base(message)
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : base($"Validation failed for {propertyName}: {errorMessage}")
    {
        ValidationErrors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }
}