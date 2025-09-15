namespace Taskpiea.Core.Results;

public sealed class ValidateResult
{
    public ResultCode ResultCode { get; init; }
    public uint Id { get; init; }
    public string ErrorMessage { get; set; } = ""; // This is for errors like "failed to deserialize"
    public List<ValidateError> Errors { get; set; } = new List<ValidateError>();

    public ValidateResult() { }
    public ValidateResult(ResultCode resultCode, uint id, string errorMessage = "")
    {
        ResultCode = resultCode;
        Id = id;
    }
    public ValidateResult(ResultCode resultCode, uint id, ValidateError error) : this(resultCode, id)
    {
        Errors.Add(error);
    }
    public ValidateResult(ResultCode resultCode, uint id, List<ValidateError> errors) : this(resultCode, id)
    {
        Errors.AddRange(errors);
    }
}