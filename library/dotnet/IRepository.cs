using Taskiea.Core.Results;

namespace Taskiea.Core;

public interface IRepository
{
    // this is so each datalayer can init their storage method (like make tables)
    void Initialize(string project);
}

public interface IRepository<TEntity> : IRepository where TEntity : IEntity
{
    // standard CRUD ops
    Task<CreateResult<TEntity>> CreateAsync(string project, TEntity entity, CancellationToken cancellationToken);
    Task<DeleteResult> DeleteAsync(string project, uint id, CancellationToken cancellationToken);
    Task<UpdateResult<TEntity>> UpdateAsync(string project, TEntity entity, CancellationToken cancellationToken);

    // validation is seperate so the client can call it without doing the full operation
    Task<ValidateResult> ValidateCreateAsync(string project, TEntity entity, CancellationToken cancellationToken);
    Task<ValidateResult> ValidateUpdateAsync(string project, TEntity entity, CancellationToken cancellationToken);

    // standard get ops
    Task<GetSingleResult<TEntity>> GetSingleAsync(string project, uint id, CancellationToken cancellationToken);
    Task<GetManyResult<TEntity>> GetAllAsync(string project, CancellationToken cancellationToken);
}
