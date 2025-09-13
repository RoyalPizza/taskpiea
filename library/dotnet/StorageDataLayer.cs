using Taskiea.Core.Results;

namespace Taskiea.Core;

public abstract class StorageDataLayer<T> where T : IDataObject
{
    protected readonly string _connectionString;
    
    public StorageDataLayer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public abstract void Initialize();

    // standard CRUD ops
    public abstract Task<CreateResult<T>> CreateAsync(T dataObject);
    public abstract Task<DeleteResult> DeleteAsync(T dataObject);
    public abstract Task<UpdateResult<T>> UpdateAsync(T dataObject);

    // validation is seperate so the client can call it without doing the full operation
    public abstract Task<ValidateResult> ValidateCreateAsync(T dataObject);
    public abstract Task<ValidateResult> ValidateUpdateAsync(T dataObject);

    // TODO: The gets do not use result wrappers. Decide if this is good or bad
    public abstract Task<T> GetSingleAsync(uint id);
    public abstract Task<List<T>> GetAllAsync();
}
