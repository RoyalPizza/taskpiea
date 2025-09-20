using Taskpiea.Core.Results;

namespace Taskpiea.Core;

public interface IRepository
{
    void Initialize(string project);
}

public interface IRepository<TEntity> : IRepository where TEntity : IEntity
{
    // standard CRUD ops
    Task<CRUDResult<TEntity>> CreateAsync(string project, TEntity entity, CancellationToken cancellationToken = default);
    Task<CRUDResult<TEntity>> DeleteAsync(string project, uint id, CancellationToken cancellationToken = default);
    Task<CRUDResult<TEntity>> UpdateAsync(string project, TEntity entity, CancellationToken cancellationToken = default);

    // validation is seperate so the client can call it without doing the full operation
    Task<ValidateResult> ValidateCreateAsync(string project, TEntity entity, CancellationToken cancellationToken = default);
    Task<ValidateResult> ValidateUpdateAsync(string project, TEntity entity, CancellationToken cancellationToken = default);

    // standard get ops
    Task<GetSingleResult<TEntity>> GetSingleAsync(string project, uint id, CancellationToken cancellationToken = default);
    Task<GetManyResult<TEntity>> GetAllAsync(string project, CancellationToken cancellationToken = default);
}
