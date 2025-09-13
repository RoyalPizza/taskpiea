using System.Net.Http.Json;
using Taskiea.Core.Results;

namespace Taskiea.Core;

public abstract class HTTPDataLayer<T> where T : IDataObject
{
    private HttpClient _httpClient;
    private readonly string _typeName = typeof(T).Name;

    public HTTPDataLayer(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OperationResult<T>> CreateAsync(T dataObject, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dataObject);
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/{_typeName}", dataObject, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var operationResult = await response.Content.ReadFromJsonAsync<OperationResult<T>>();
            if (operationResult == null)
                return new OperationResult<T>("Failed to deserialize data on create response", response.StatusCode, default(T));
            return operationResult;
        }

        return new OperationResult<T>("Failed to create data", response.StatusCode, dataObject);
    }

    public async Task<OperationResult<T>> DeleteAsync(uint id, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync($"api/{_typeName}/{id}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var operationResult = await response.Content.ReadFromJsonAsync<OperationResult<T>>();
            if (operationResult == null)
                return new OperationResult<T>("Failed to deserialize data on delete response", response.StatusCode, default(T));
            return operationResult;
        }

        return new OperationResult<T>("Failed to delete data", response.StatusCode, default(T));
    }

    //public async Task<OperationResult<List<T>>> GetAllAsync(CancellationToken cancellationToken)
    //{
    //    HttpResponseMessage response = await _httpClient.GetAsync($"api/{_typeName}/All", cancellationToken);

    //    if (response.IsSuccessStatusCode)
    //    {

    //    }

    //    return null;

    //    //return new OperationResult<T>("Failed to retrieve data", response.StatusCode, default(T));
    //}

    public async Task<OperationResult<T>> GetSingleAsync(uint id, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/{_typeName}/Single/{id}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var operationResult = await response.Content.ReadFromJsonAsync<OperationResult<T>>();
            if (operationResult == null)
                return new OperationResult<T>("Failed to deserialize data on get response", response.StatusCode, default(T));
            return operationResult;
        }

        return new OperationResult<T>("Failed to retrieve data", response.StatusCode, default(T));
    }
}
