using Taskiea.Core.Results;

namespace Taskiea.Core.Projects;

public sealed class ProjectRepositorySqlite : IProjectRepository
{
    public Task<CreateResult<Project>> CreateAsync(ProjectConnectionData projectConnectionData, Project dataObject, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<DeleteResult> DeleteAsync(ProjectConnectionData projectConnectionData, uint id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<GetManyResult<Project>> GetAllAsync(ProjectConnectionData projectConnectionData, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<GetSingleResult<Project>> GetSingleAsync(ProjectConnectionData projectConnectionData, uint id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Initialize(ProjectConnectionData projectConnectionData)
    {
        throw new NotImplementedException();
    }

    public Task<UpdateResult<Project>> UpdateAsync(ProjectConnectionData projectConnectionData, Project dataObject, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ValidateResult> ValidateCreateAsync(ProjectConnectionData projectConnectionData, Project dataObject, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ValidateResult> ValidateUpdateAsync(ProjectConnectionData projectConnectionData, Project dataObject, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
