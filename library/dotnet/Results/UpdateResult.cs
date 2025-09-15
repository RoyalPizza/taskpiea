namespace Taskiea.Core.Results;

public sealed class UpdateResult<T> where T : IEntity
{
    public ResultCode ResultCode { get; init; }
    public T? DataObject { get; init; }
    public string ErrorMessage { get; init; } = "";

    public UpdateResult() { }
    public UpdateResult(ResultCode resultCode, T? entity, string errorMessage = "")
    {
        ResultCode = resultCode;
        DataObject = entity;
        ErrorMessage = errorMessage;
    }
}
