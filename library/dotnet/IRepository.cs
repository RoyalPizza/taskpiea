using Taskiea.Core.Connections;
using Taskiea.Core.Results;

namespace Taskiea.Core;

public interface IRepository<T, T1> where T : IEntity where T1 : BaseConnectionData
{
    // this is so each datalayer can init their storage method (like make tables)
    void Initialize(T1 connectionData);

    // standard CRUD ops
    Task<CreateResult<T>> CreateAsync(T1 connectionData, T entity, CancellationToken cancellationToken);
    Task<DeleteResult> DeleteAsync(T1 connectionData, uint id, CancellationToken cancellationToken);
    Task<UpdateResult<T>> UpdateAsync(T1 connectionData, T entity, CancellationToken cancellationToken);

    // validation is seperate so the client can call it without doing the full operation
    Task<ValidateResult> ValidateCreateAsync(T1 connectionData, T entity, CancellationToken cancellationToken);
    Task<ValidateResult> ValidateUpdateAsync(T1 connectionData, T entity, CancellationToken cancellationToken);

    // standard get ops
    Task<GetSingleResult<T>> GetSingleAsync(T1 connectionData, uint id, CancellationToken cancellationToken);
    Task<GetManyResult<T>> GetAllAsync(T1 connectionData, CancellationToken cancellationToken);
}
