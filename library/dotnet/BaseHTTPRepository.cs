using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Taskiea.Core.Connections;
using Taskiea.Core.Results;

namespace Taskiea.Core;

public abstract class BaseHTTPRepository<TEntity> : IRepository<TEntity> where TEntity : IEntity
{
    private HTTPConnectionData _connectionData;
    private HttpClient _httpClient = null!;
    private readonly string _typeName = typeof(TEntity).Name;

    public void Initialize(string project)
    {
        _connectionData = ConnectionCache.GetConnectionData<HTTPConnectionData>(project);
        _httpClient = _connectionData.HttpClient;
    }

    protected static void AddProjectToRequest(HttpRequestMessage request, HTTPConnectionData connectionData)
    {
        // Pass the project name to the API so it can use the storage datalayer with the proper connection string.
        request.Headers.Add("X-Project-Id", connectionData.ProjectName);
    }

    public async Task<CreateResult<TEntity>> CreateAsync(string project, TEntity entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // TODO: Update all other HTTP functions to have this request style
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/{_typeName}");
        AddProjectToRequest(request, _connectionData);
        request.Content = new StringContent(JsonSerializer.Serialize(entity), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<CreateResult<TEntity>>();
            if (result == null)
                return new CreateResult<TEntity>(ResultCode.Failure, entity, "Failed to deserialize data on create response.");
            return result;
        }

        return new CreateResult<TEntity>(ResultCode.Failure, entity, "Failed to create data.");
    }

    public async Task<DeleteResult> DeleteAsync(string project, uint id, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/{_typeName}/{id}");
        AddProjectToRequest(request, _connectionData);

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

    public async Task<UpdateResult<TEntity>> UpdateAsync(string project, TEntity entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var request = new HttpRequestMessage(HttpMethod.Put, $"api/{_typeName}");
        AddProjectToRequest(request, _connectionData);
        request.Content = new StringContent(JsonSerializer.Serialize(entity), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<UpdateResult<TEntity>>();
            if (result == null)
                return new UpdateResult<TEntity>(ResultCode.Failure, entity, "Failed to deserialize data on update response.");
            return result;
        }

        return new UpdateResult<TEntity>(ResultCode.Failure, entity, "Failed to update data.");
    }

    public async Task<ValidateResult> ValidateCreateAsync(string project, TEntity entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/{_typeName}/Validate");
        AddProjectToRequest(request, _connectionData);
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

    public async Task<ValidateResult> ValidateUpdateAsync(string project, TEntity entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var request = new HttpRequestMessage(HttpMethod.Put, $"api/{_typeName}/Validate");
        AddProjectToRequest(request, _connectionData);
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

    public async Task<GetSingleResult<TEntity>> GetSingleAsync(string project, uint id, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/{_typeName}/Single/{id}");
        AddProjectToRequest(request, _connectionData);

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<GetSingleResult<TEntity>>();
            if (result == null)
                return new GetSingleResult<TEntity>(ResultCode.Failure, id, default(TEntity), "Failed to deserialize data on get single response.");
            return result;
        }

        return new GetSingleResult<TEntity>(ResultCode.Failure, id, default(TEntity), "Failed to retrieve data");
    }

    public async Task<GetManyResult<TEntity>> GetAllAsync(string project, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/{_typeName}/All");
        AddProjectToRequest(request, _connectionData);

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<GetManyResult<TEntity>>();
            if (result == null)
                return new GetManyResult<TEntity>(ResultCode.Failure, "Failed to deserialize data on get single response.");
            return result;
        }

        return new GetManyResult<TEntity>(ResultCode.Failure, "Failed to retrieve data.");
    }
}
