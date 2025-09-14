using Taskiea.Core.Results;

namespace Taskiea.Core.Projects;

/*
 * This datalayer does not use Sqlite, because it creates/deletes databases and such.
 * Project files are stored as .taskp files. They can be stored anywhere, but on a server
 * they will always be saved in an "AppData" style directory. Standalone will also default
 * to "AppData".
 * 
 * "GetAll" will only work in a server/client mode because we allow flexibility in standalone mode.
 * Need to figure out how we could make this work in standalone, but typically standalone uses
 * an "open recent" system, not an "open all" system.
 * 
 * If create is called on a project, the storage datalayer needs to check if the DB exist.
 * If it doesnt, then all the other datalyers need to have their "initialize" called.
 * 
 */

public sealed class ProjectStorageDataLayer : StorageDataLayer<Project>
{
    public override void Initialize(ProjectConnectionData projectConnectionData)
    {
        throw new NotImplementedException();
    }

    public override async Task<CreateResult<Project>> CreateAsync(ProjectConnectionData projectConnectionData, Project dataObject, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override async Task<DeleteResult> DeleteAsync(ProjectConnectionData projectConnectionData, uint id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override async Task<UpdateResult<Project>> UpdateAsync(ProjectConnectionData projectConnectionData, Project dataObject, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override async Task<ValidateResult> ValidateCreateAsync(ProjectConnectionData projectConnectionData, Project dataObject, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override async Task<ValidateResult> ValidateUpdateAsync(ProjectConnectionData projectConnectionData, Project dataObject, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override async Task<GetSingleResult<Project>> GetSingleAsync(ProjectConnectionData projectConnectionData, uint id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override async Task<GetManyResult<Project>> GetAllAsync(ProjectConnectionData projectConnectionData, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
