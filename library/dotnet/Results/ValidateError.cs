namespace Taskpiea.Core.Results;

public sealed class ValidateError
{
    public string PropertyName { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public ValidateError() { }
    public ValidateError(string propertyName, string message)
    {
        PropertyName = propertyName;
        Message = message;
    }
}
