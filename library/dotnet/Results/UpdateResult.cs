namespace Taskiea.Core.Results;

public sealed class UpdateResult<T> where T : IDataObject
{
    public ResultCode ResultCode { get; init; }
    public T? DataObject { get; init; }
    public string ErrorMessage { get; init; } = "";

    public UpdateResult() { }
    public UpdateResult(ResultCode resultCode, T? dataObject, string errorMessage = "")
    {
        ResultCode = resultCode;
        DataObject = dataObject;
        ErrorMessage = errorMessage;
    }
}
