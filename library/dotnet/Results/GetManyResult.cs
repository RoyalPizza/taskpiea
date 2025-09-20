namespace Taskpiea.Core.Results;

public class GetManyResult<T> where T : IEntity
{
    public ResultCode ResultCode { get; init; }
    public List<T> Entities { get; set; } = new List<T>();
    public string ErrorMessage { get; init; } = "";

    public GetManyResult() { }
    public GetManyResult(ResultCode resultCode, string errorMessage = "")
    {
        ResultCode = resultCode;
        ErrorMessage = errorMessage;
    }
    public GetManyResult(ResultCode resultCode, List<T> entities, string errorMessage = "")
    {
        ResultCode = resultCode;
        ErrorMessage = errorMessage;
        Entities.AddRange(entities);
    }
}
