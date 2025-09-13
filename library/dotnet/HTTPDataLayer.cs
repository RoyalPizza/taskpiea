using System.Net.Http.Json;
using Taskiea.Core.Results;

namespace Taskiea.Core;

public abstract class HTTPDataLayer<T> : IDataLayer<T> where T : IDataObject
{
    private HttpClient _httpClient;
    private readonly string _typeName = typeof(T).Name;

    public HTTPDataLayer(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CreateResult<T>> CreateAsync(T dataObject, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dataObject);
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/{_typeName}", dataObject, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<CreateResult<T>>();
            if (result == null)
                return new CreateResult<T>(ResultCode.Failure, dataObject, "Failed to deserialize data on create response.");
            return result;
        }

        return new CreateResult<T>(ResultCode.Failure, dataObject, "Failed to create data.");
    }

    public async Task<DeleteResult> DeleteAsync(uint id, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync($"api/{_typeName}/{id}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<DeleteResult>();
            if (result == null)
                return new DeleteResult(ResultCode.Failure, id, "Failed to deserialize data on delete response.");
            return result;
        }

        return new DeleteResult(ResultCode.Failure, id, "Failed to delete data.");
    }

    public async Task<UpdateResult<T>> UpdateAsync(T dataObject, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dataObject);
        HttpResponseMessage resposne = await _httpClient.PutAsJsonAsync<T>($"api/{_typeName}", dataObject, cancellationToken);

        if (resposne.IsSuccessStatusCode)
        {
            var result = await resposne.Content.ReadFromJsonAsync<UpdateResult<T>>();
            if (result == null)
                return new UpdateResult<T>(ResultCode.Failure, dataObject, "Failed to deserialize data on update response.");
            return result;
        }

        return new UpdateResult<T>(ResultCode.Failure, dataObject, "Failed to update data.");
    }

    public async Task<ValidateResult> ValidateCreateAsync(T dataObject, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dataObject);
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/{_typeName}/Validate", dataObject, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ValidateResult>();
            if (result == null)
                return new ValidateResult(ResultCode.Failure, dataObject.GetId(), "Failed to deserialize data on validate create response.");
            return result;
        }

        return new ValidateResult(ResultCode.Failure, dataObject.GetId(), "Failed to validate create data.");
    }

    public async Task<ValidateResult> ValidateUpdateAsync(T dataObject, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dataObject);
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/{_typeName}/Validate", dataObject, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ValidateResult>();
            if (result == null)
                return new ValidateResult(ResultCode.Failure, dataObject.GetId(), "Failed to deserialize data on validate update response.");
            return result;
        }

        return new ValidateResult(ResultCode.Failure, dataObject.GetId(), "Failed to validate update data.");
    }

    public async Task<GetSingleResult<T>> GetSingleAsync(uint id, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/{_typeName}/Single/{id}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<GetSingleResult<T>>();
            if (result == null)
                return new GetSingleResult<T>(ResultCode.Failure, id, default(T), "Failed to deserialize data on get single response.");
            return result;
        }

        return new GetSingleResult<T>(ResultCode.Failure, id, default(T), "Failed to retrieve data");
    }

    public async Task<GetManyResult<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/{_typeName}/All", cancellationToken);

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
