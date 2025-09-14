using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Taskiea.Core.Results;

namespace Taskiea.Core;

public abstract class BaseHTTPRepository<T> where T : IEntity
{
    private HttpClient _httpClient;
    private readonly string _typeName = typeof(T).Name;

    public BaseHTTPRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private static void AddProjectToRequest(HttpRequestMessage request, ProjectConnectionData projectConnectionData)
    {
        // Pass the project name to the API so it can use the storage datalayer with the proper connection string.
        request.Headers.Add("X-Project-Id", projectConnectionData.ProjectName);
    }

    public async Task<CreateResult<T>> CreateAsync(ProjectConnectionData projectConnectionData, T dataObject, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dataObject);

        // TODO: Update all other HTTP functions to have this request style
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/{_typeName}");
        AddProjectToRequest(request, projectConnectionData);
        request.Content = new StringContent(JsonSerializer.Serialize(dataObject), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<CreateResult<T>>();
            if (result == null)
                return new CreateResult<T>(ResultCode.Failure, dataObject, "Failed to deserialize data on create response.");
            return result;
        }

        return new CreateResult<T>(ResultCode.Failure, dataObject, "Failed to create data.");
    }

    public async Task<DeleteResult> DeleteAsync(ProjectConnectionData projectConnectionData, uint id, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/{_typeName}/{id}");
        AddProjectToRequest(request, projectConnectionData);

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

    public async Task<UpdateResult<T>> UpdateAsync(ProjectConnectionData projectConnectionData, T dataObject, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dataObject);

        var request = new HttpRequestMessage(HttpMethod.Put, $"api/{_typeName}");
        AddProjectToRequest(request, projectConnectionData);
        request.Content = new StringContent(JsonSerializer.Serialize(dataObject), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<UpdateResult<T>>();
            if (result == null)
                return new UpdateResult<T>(ResultCode.Failure, dataObject, "Failed to deserialize data on update response.");
            return result;
        }

        return new UpdateResult<T>(ResultCode.Failure, dataObject, "Failed to update data.");
    }

    public async Task<ValidateResult> ValidateCreateAsync(ProjectConnectionData projectConnectionData, T dataObject, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dataObject);

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/{_typeName}/Validate");
        AddProjectToRequest(request, projectConnectionData);
        request.Content = new StringContent(JsonSerializer.Serialize(dataObject), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ValidateResult>();
            if (result == null)
                return new ValidateResult(ResultCode.Failure, dataObject.GetId(), "Failed to deserialize data on validate create response.");
            return result;
        }

        return new ValidateResult(ResultCode.Failure, dataObject.GetId(), "Failed to validate create data.");
    }

    public async Task<ValidateResult> ValidateUpdateAsync(ProjectConnectionData projectConnectionData, T dataObject, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dataObject);

        var request = new HttpRequestMessage(HttpMethod.Put, $"api/{_typeName}/Validate");
        AddProjectToRequest(request, projectConnectionData);
        request.Content = new StringContent(JsonSerializer.Serialize(dataObject), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ValidateResult>();
            if (result == null)
                return new ValidateResult(ResultCode.Failure, dataObject.GetId(), "Failed to deserialize data on validate update response.");
            return result;
        }

        return new ValidateResult(ResultCode.Failure, dataObject.GetId(), "Failed to validate update data.");
    }

    public async Task<GetSingleResult<T>> GetSingleAsync(ProjectConnectionData projectConnectionData, uint id, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/{_typeName}/Single/{id}");
        AddProjectToRequest(request, projectConnectionData);

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

    public async Task<GetManyResult<T>> GetAllAsync(ProjectConnectionData projectConnectionData, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/{_typeName}/All");
        AddProjectToRequest(request, projectConnectionData);

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
