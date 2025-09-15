namespace Taskpiea.Core.Results;

public sealed class CreateResult<T> where T : IEntity
{
    public ResultCode ResultCode { get; init; }
    public T? DataObject { get; init; }
    public string ErrorMessage { get; init; } = "";

    public CreateResult() { }
    public CreateResult(ResultCode resultCode, T? entity, string errorMessage = "")
    {
        ResultCode = resultCode;
        ErrorMessage = errorMessage;
        DataObject = entity;
    }
}
