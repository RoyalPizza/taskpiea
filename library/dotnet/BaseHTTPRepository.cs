using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Taskpiea.Core.Connections;
using Taskpiea.Core.Results;

namespace Taskpiea.Core;

public abstract class BaseHTTPRepository<TEntity> : BaseRepository, IRepository<TEntity> where TEntity : IEntity
{
    private string _typeName;

    protected BaseHTTPRepository(IConnectionCache connectionCache) : base(connectionCache)
    {
        _typeName = typeof(TEntity).Name;
    }

    private HTTPConnectionData? GetConnectionData(string project) => _connectionCache.GetConnectionData<HTTPConnectionData>(project);

    public override void Initialize(string project)
    {

    }

    protected static void AddProjectToRequest(HttpRequestMessage request, HTTPConnectionData connectionData)
    {
        // Pass the project name to the API so it can use the storage datalayer with the proper connection string.
        request.Headers.Add("X-Project-Id", connectionData.ProjectName);
    }

    public async Task<CreateResult<TEntity>> CreateAsync(string project, TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var connectionData = GetConnectionData(project);
        if (connectionData == null)
            return new CreateResult<TEntity>(ResultCode.Failure, entity, "Failed to retrieve connection data.");
        if (connectionData.HttpClient == null)
            return new CreateResult<TEntity>(ResultCode.Failure, entity, "No HTTP client configured.");

        // TODO: Update all other HTTP functions to have this request style
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/{_typeName}");
        AddProjectToRequest(request, connectionData);
        request.Content = new StringContent(JsonSerializer.Serialize(entity), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await connectionData.HttpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<CreateResult<TEntity>>();
            if (result == null)
                return new CreateResult<TEntity>(ResultCode.Failure, entity, "Failed to deserialize data on create response.");
            return result;
        }

        return new CreateResult<TEntity>(ResultCode.Failure, entity, "Failed to create data.");
    }

    public async Task<DeleteResult> DeleteAsync(string project, uint id, CancellationToken cancellationToken = default)
    {
        var connectionData = GetConnectionData(project);
        if (connectionData == null)
            return new DeleteResult(ResultCode.Failure, id, "Failed to retrieve connection data.");
        if (connectionData.HttpClient == null)
            return new DeleteResult(ResultCode.Failure, id, "No HTTP client configured.");

        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/{_typeName}/{id}");
        AddProjectToRequest(request, connectionData);

        HttpResponseMessage response = await connectionData.HttpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<DeleteResult>();
            if (result == null)
                return new DeleteResult(ResultCode.Failure, id, "Failed to deserialize data on delete response.");
            return result;
        }

        return new DeleteResult(ResultCode.Failure, id, "Failed to delete data.");
    }

    public async Task<UpdateResult<TEntity>> UpdateAsync(string project, TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var connectionData = GetConnectionData(project);
        if (connectionData == null)
            return new UpdateResult<TEntity>(ResultCode.Failure, entity, "Failed to retrieve connection data.");
        if (connectionData.HttpClient == null)
            return new UpdateResult<TEntity>(ResultCode.Failure, entity, "No HTTP client configured.");

        var request = new HttpRequestMessage(HttpMethod.Put, $"api/{_typeName}");
        AddProjectToRequest(request, connectionData);
        request.Content = new StringContent(JsonSerializer.Serialize(entity), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await connectionData.HttpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<UpdateResult<TEntity>>();
            if (result == null)
                return new UpdateResult<TEntity>(ResultCode.Failure, entity, "Failed to deserialize data on update response.");
            return result;
        }

        return new UpdateResult<TEntity>(ResultCode.Failure, entity, "Failed to update data.");
    }

    public async Task<ValidateResult> ValidateCreateAsync(string project, TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var connectionData = GetConnectionData(project);
        if (connectionData == null)
            return new ValidateResult(ResultCode.Failure, entity.GetId(), "Failed to retrieve connection data.");
        if (connectionData.HttpClient == null)
            return new ValidateResult(ResultCode.Failure, entity.GetId(), "No HTTP client configured.");

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/{_typeName}/Validate");
        AddProjectToRequest(request, connectionData);
        request.Content = new StringContent(JsonSerializer.Serialize(entity), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await connectionData.HttpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ValidateResult>();
            if (result == null)
                return new ValidateResult(ResultCode.Failure, entity.GetId(), "Failed to deserialize data on validate create response.");
            return result;
        }

        return new ValidateResult(ResultCode.Failure, entity.GetId(), "Failed to validate create data.");
    }

    public async Task<ValidateResult> ValidateUpdateAsync(string project, TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var connectionData = GetConnectionData(project);
        if (connectionData == null)
            return new ValidateResult(ResultCode.Failure, entity.GetId(), "Failed to retrieve connection data.");
        if (connectionData.HttpClient == null)
            return new ValidateResult(ResultCode.Failure, entity.GetId(), "No HTTP client configured.");

        var request = new HttpRequestMessage(HttpMethod.Put, $"api/{_typeName}/Validate");
        AddProjectToRequest(request, connectionData);
        request.Content = new StringContent(JsonSerializer.Serialize(entity), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await connectionData.HttpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ValidateResult>();
            if (result == null)
                return new ValidateResult(ResultCode.Failure, entity.GetId(), "Failed to deserialize data on validate update response.");
            return result;
        }

        return new ValidateResult(ResultCode.Failure, entity.GetId(), "Failed to validate update data.");
    }

    public async Task<GetSingleResult<TEntity>> GetSingleAsync(string project, uint id, CancellationToken cancellationToken = default)
    {
        var connectionData = GetConnectionData(project);
        if (connectionData == null)
            return new GetSingleResult<TEntity>(ResultCode.Failure, id, default(TEntity), "Failed to retrieve connection data.");
        if (connectionData.HttpClient == null)
            return new GetSingleResult<TEntity>(ResultCode.Failure, id, default(TEntity), "No HTTP client configured.");

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/{_typeName}/Single/{id}");
        AddProjectToRequest(request, connectionData);

        HttpResponseMessage response = await connectionData.HttpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<GetSingleResult<TEntity>>();
            if (result == null)
                return new GetSingleResult<TEntity>(ResultCode.Failure, id, default(TEntity), "Failed to deserialize data on get single response.");
            return result;
        }

        return new GetSingleResult<TEntity>(ResultCode.Failure, id, default(TEntity), "Failed to retrieve data");
    }

    public async Task<GetManyResult<TEntity>> GetAllAsync(string project, CancellationToken cancellationToken = default)
    {
        var connectionData = GetConnectionData(project);
        if (connectionData == null)
            return new GetManyResult<TEntity>(ResultCode.Failure, "Failed to retrieve connection data.");
        if (connectionData.HttpClient == null)
            return new GetManyResult<TEntity>(ResultCode.Failure, "No HTTP client configured.");

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/{_typeName}/All");
        AddProjectToRequest(request, connectionData);

        HttpResponseMessage response = await connectionData.HttpClient.SendAsync(request, cancellationToken);

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
