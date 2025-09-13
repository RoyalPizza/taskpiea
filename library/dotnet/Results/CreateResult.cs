namespace Taskiea.Core.Results;

public sealed class CreateResult<T> where T : IDataObject
{
    public ResultCode ResultCode { get; init; }
    public T? DataObject { get; init; }
    public string ErrorMessage { get; init; } = "";

    public CreateResult() { }
    public CreateResult(ResultCode resultCode, T? dataObject, string errorMessage = "")
    {
        ResultCode = resultCode; ErrorMessage = errorMessage;
        ErrorMessage = errorMessage;
        DataObject = dataObject;
    }
}
