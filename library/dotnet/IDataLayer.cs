using Taskiea.Core.Results;

namespace Taskiea.Core;

public interface IDataLayer<T> where T : IDataObject
{
    Task<CreateResult<T>> CreateAsync(T dataObject, CancellationToken cancellationToken);
    Task<DeleteResult> DeleteAsync(uint id, CancellationToken cancellationToken);
    Task<UpdateResult<T>> UpdateAsync(T dataObject, CancellationToken cancellationToken);

    // validation is seperate so the client can call it without doing the full operation
    Task<ValidateResult> ValidateCreateAsync(T dataObject, CancellationToken cancellationToken);
    Task<ValidateResult> ValidateUpdateAsync(T dataObject, CancellationToken cancellationToken);

    // TODO: The gets do not use result wrappers. Decide if this is good or bad
    Task<GetSingleResult<T>> GetSingleAsync(uint id, CancellationToken cancellationToken);
    Task<GetManyResult<T>> GetAllAsync(CancellationToken cancellationToken);
}
