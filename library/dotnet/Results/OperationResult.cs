using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Taskiea.Core.Results;

public class OperationResult<T> where T : IDataObject
{
    public bool IsSuccess { get => StatusCode == HttpStatusCode.OK && Data != null; }
    public string? ErrorMessage { get; init; }
    public HttpStatusCode StatusCode { get; init; } // todo: decide if I just want this to be http, or my own status codes
    public T? Data { get; set; }

    public OperationResult() {}
    public OperationResult(string? errorMessage, HttpStatusCode statusCode, T? data)
    {
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
        Data = data;
    }
}
