namespace Taskiea.Core.Results;

public class GetSingleResult<T> where T : IEntity
{
    public ResultCode ResultCode { get; init; }
    public uint Id { get; init; }
    public T? DataObject { get; init; }
    public string ErrorMessage { get; init; } = "";

    public GetSingleResult() { }
    public GetSingleResult(ResultCode resultCode, uint id, T? dataObject, string errorMessage = "")
    {
        ResultCode = resultCode;
        Id = id;
        ErrorMessage = errorMessage;
        DataObject = dataObject;
    }
}
