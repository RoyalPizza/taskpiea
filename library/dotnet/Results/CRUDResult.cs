namespace Taskpiea.Core.Results;

public sealed class CRUDResult<T> where T : IEntity
{
    public ResultCode ResultCode { get; init; }
    public uint Id { get; init; }
    public T? Entity { get; init; }
    public string ErrorMessage { get; init; } = "";
    public List<ValidateError> ValidationErrors { get; set; } = new List<ValidateError>();

    public CRUDResult() { }
    public CRUDResult(ResultCode resultCode, uint id, string errorMessage = "")
    {
        ResultCode = resultCode;
        Id = id;
        Entity = default;
        ErrorMessage = errorMessage;
    }
    public CRUDResult(ResultCode resultCode, T? entity, string errorMessage = "")
    {
        ResultCode = resultCode;
        Id = entity?.GetId() ?? 0;
        Entity = entity;
        ErrorMessage = errorMessage;
    }
}
