using Taskiea.Core.Results;

namespace Taskiea.Core;

public interface ICRUDDataLayer<T> where T : IDataObject
{
    Task<CreateResult<T>> CreateAsync(ProjectConnectionData projectConnectionData, T dataObject, CancellationToken cancellationToken);
    Task<DeleteResult> DeleteAsync(ProjectConnectionData projectConnectionData, uint id, CancellationToken cancellationToken);
    Task<UpdateResult<T>> UpdateAsync(ProjectConnectionData projectConnectionData, T dataObject, CancellationToken cancellationToken);

    // validation is seperate so the client can call it without doing the full operation
    Task<ValidateResult> ValidateCreateAsync(ProjectConnectionData projectConnectionData, T dataObject, CancellationToken cancellationToken);
    Task<ValidateResult> ValidateUpdateAsync(ProjectConnectionData projectConnectionData, T dataObject, CancellationToken cancellationToken);

    // TODO: The gets do not use result wrappers. Decide if this is good or bad
    Task<GetSingleResult<T>> GetSingleAsync(ProjectConnectionData projectConnectionData, uint id, CancellationToken cancellationToken);
    Task<GetManyResult<T>> GetAllAsync(ProjectConnectionData projectConnectionData, CancellationToken cancellationToken);
}