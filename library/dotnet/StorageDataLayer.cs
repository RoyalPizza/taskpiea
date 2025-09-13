using Taskiea.Core.Results;

namespace Taskiea.Core;

public abstract class StorageDataLayer<T> : IDataLayer<T> where T : IDataObject
{
    protected readonly string _connectionString;

    public StorageDataLayer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public abstract void Initialize();

    // standard CRUD ops
    public abstract Task<CreateResult<T>> CreateAsync(T dataObject, CancellationToken cancellationToken);
    public abstract Task<DeleteResult> DeleteAsync(uint id, CancellationToken cancellationToken);
    public abstract Task<UpdateResult<T>> UpdateAsync(T dataObject, CancellationToken cancellationToken);

    // validation is seperate so the client can call it without doing the full operation
    public abstract Task<ValidateResult> ValidateCreateAsync(T dataObject, CancellationToken cancellationToken);
    public abstract Task<ValidateResult> ValidateUpdateAsync(T dataObject, CancellationToken cancellationToken);

    // TODO: The gets do not use result wrappers. Decide if this is good or bad
    public abstract Task<GetSingleResult<T>> GetSingleAsync(uint id, CancellationToken cancellationToken);
    public abstract Task<GetManyResult<T>> GetAllAsync(CancellationToken cancellationToken);
}
