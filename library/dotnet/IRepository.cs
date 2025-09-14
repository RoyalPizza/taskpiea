using Taskiea.Core.Results;

namespace Taskiea.Core;

public interface IRepository<T> where T : IEntity
{
    // this is so each datalayer can init their storage method (like make tables)
    void Initialize(ProjectConnectionData projectConnectionData);

    // standard CRUD ops
    Task<CreateResult<T>> CreateAsync(ProjectConnectionData projectConnectionData, T dataObject, CancellationToken cancellationToken);
    Task<DeleteResult> DeleteAsync(ProjectConnectionData projectConnectionData, uint id, CancellationToken cancellationToken);
    Task<UpdateResult<T>> UpdateAsync(ProjectConnectionData projectConnectionData, T dataObject, CancellationToken cancellationToken);

    // validation is seperate so the client can call it without doing the full operation
    Task<ValidateResult> ValidateCreateAsync(ProjectConnectionData projectConnectionData, T dataObject, CancellationToken cancellationToken);
    Task<ValidateResult> ValidateUpdateAsync(ProjectConnectionData projectConnectionData, T dataObject, CancellationToken cancellationToken);

    // standard get ops
    Task<GetSingleResult<T>> GetSingleAsync(ProjectConnectionData projectConnectionData, uint id, CancellationToken cancellationToken);
    Task<GetManyResult<T>> GetAllAsync(ProjectConnectionData projectConnectionData, CancellationToken cancellationToken);
}
