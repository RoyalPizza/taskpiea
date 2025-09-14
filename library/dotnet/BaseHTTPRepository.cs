using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Taskiea.Core.Connections;
using Taskiea.Core.Results;

namespace Taskiea.Core;

public abstract class BaseHTTPRepository<T> : IRepository<T, HTTPConnectionData> where T : IEntity
{
    private HttpClient _httpClient = null!;
    private readonly string _typeName = typeof(T).Name;

    public void Initialize(HTTPConnectionData httpConnectionData)
    {
        ArgumentNullException.ThrowIfNull(httpConnectionData.HttpClient);
        _httpClient = httpConnectionData.HttpClient;
    }

    protected static void AddProjectToRequest(HttpRequestMessage request, HTTPConnectionData connectionData)
    {
        // Pass the project name to the API so it can use the storage datalayer with the proper connection string.
        request.Headers.Add("X-Project-Id", connectionData.ProjectName);
    }

    public async Task<CreateResult<T>> CreateAsync(HTTPConnectionData connectionData, T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // TODO: Update all other HTTP functions to have this request style
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/{_typeName}");
        AddProjectToRequest(request, connectionData);
        request.Content = new StringContent(JsonSerializer.Serialize(entity), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<CreateResult<T>>();
            if (result == null)
                return new CreateResult<T>(ResultCode.Failure, entity, "Failed to deserialize data on create response.");
            return result;
        }

        return new CreateResult<T>(ResultCode.Failure, entity, "Failed to create data.");
    }

    public async Task<DeleteResult> DeleteAsync(HTTPConnectionData connectionData, uint id, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/{_typeName}/{id}");
        AddProjectToRequest(request, connectionData);

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<DeleteResult>();
            if (result == null)
                return new DeleteResult(ResultCode.Failure, id, "Failed to deserialize data on delete response.");
            return result;
        }

        return new DeleteResult(ResultCode.Failure, id, "Failed to delete data.");
    }

    public async Task<UpdateResult<T>> UpdateAsync(HTTPConnectionData connectionData, T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var request = new HttpRequestMessage(HttpMethod.Put, $"api/{_typeName}");
        AddProjectToRequest(request, connectionData);
        request.Content = new StringContent(JsonSerializer.Serialize(entity), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<UpdateResult<T>>();
            if (result == null)
                return new UpdateResult<T>(ResultCode.Failure, entity, "Failed to deserialize data on update response.");
            return result;
        }

        return new UpdateResult<T>(ResultCode.Failure, entity, "Failed to update data.");
    }

    public async Task<ValidateResult> ValidateCreateAsync(HTTPConnectionData connectionData, T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/{_typeName}/Validate");
        AddProjectToRequest(request, connectionData);
        request.Content = new StringContent(JsonSerializer.Serialize(entity), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ValidateResult>();
            if (result == null)
                return new ValidateResult(ResultCode.Failure, entity.GetId(), "Failed to deserialize data on validate create response.");
            return result;
        }

        return new ValidateResult(ResultCode.Failure, entity.GetId(), "Failed to validate create data.");
    }

    public async Task<ValidateResult> ValidateUpdateAsync(HTTPConnectionData connectionData, T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var request = new HttpRequestMessage(HttpMethod.Put, $"api/{_typeName}/Validate");
        AddProjectToRequest(request, connectionData);
        request.Content = new StringContent(JsonSerializer.Serialize(entity), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ValidateResult>();
            if (result == null)
                return new ValidateResult(ResultCode.Failure, entity.GetId(), "Failed to deserialize data on validate update response.");
            return result;
        }

        return new ValidateResult(ResultCode.Failure, entity.GetId(), "Failed to validate update data.");
    }

    public async Task<GetSingleResult<T>> GetSingleAsync(HTTPConnectionData connectionData, uint id, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/{_typeName}/Single/{id}");
        AddProjectToRequest(request, connectionData);

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<GetSingleResult<T>>();
            if (result == null)
                return new GetSingleResult<T>(ResultCode.Failure, id, default(T), "Failed to deserialize data on get single response.");
            return result;
        }

        return new GetSingleResult<T>(ResultCode.Failure, id, default(T), "Failed to retrieve data");
    }

    public async Task<GetManyResult<T>> GetAllAsync(HTTPConnectionData connectionData, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/{_typeName}/All");
        AddProjectToRequest(request, connectionData);

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<GetManyResult<T>>();
            if (result == null)
                return new GetManyResult<T>(ResultCode.Failure, "Failed to deserialize data on get single response.");
            return result;
        }

        return new GetManyResult<T>(ResultCode.Failure, "Failed to retrieve data.");
    }
}
