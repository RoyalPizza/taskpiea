namespace Taskpiea.Core.Results;

public sealed class DeleteResult
{
    public ResultCode ResultCode { get; init; }
    public uint Id { get; init; }
    public string ErrorMessage { get; init; } = "";

    public DeleteResult() { }
    public DeleteResult(ResultCode resultCode, uint id, string errorMessage = "")
    {
        ResultCode = resultCode;
        Id = id;
        ErrorMessage = errorMessage;
    }
}
