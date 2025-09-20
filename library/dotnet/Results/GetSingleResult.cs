namespace Taskpiea.Core.Results;

public class GetSingleResult<T> where T : IEntity
{
    public ResultCode ResultCode { get; init; }
    public T? Entity { get; init; }
    public string ErrorMessage { get; init; } = "";

    public GetSingleResult() { }
    public GetSingleResult(ResultCode resultCode, uint id, T? entity, string errorMessage = "")
    {
        ResultCode = resultCode;
        ErrorMessage = errorMessage;
        Entity = entity;
    }
}
