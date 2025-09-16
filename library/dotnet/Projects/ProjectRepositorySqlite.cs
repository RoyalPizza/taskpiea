using Microsoft.Data.Sqlite;
using Taskpiea.Core.Connections;
using Taskpiea.Core.Results;

namespace Taskpiea.Core.Projects;

// TODO: Finish this code. Needs design thought as to what file manages the DB. The prober or this.
// I think this as the probber is just a "pre checker"

public sealed class ProjectRepositorySqlite : BaseRepository, IProjectRepository
{
    public ProjectRepositorySqlite(IConnectionCache connectionCache) : base(connectionCache)
    {
        
    }

    public override void Initialize(string project)
    {
        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return;

        string connectionString = connectionData.ConnectionString;
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Projects (
            Name TEXT PRIMARY KEY,
            CreatedOn TEXT NOT NULL
        );";
        command.ExecuteNonQuery();
    }

    public async Task<CreateResult<Project>> CreateAsync(string project, Project entity, CancellationToken cancellationToken = default)
    {
        // TODO: Call verify. If success then create the database file, call initialize so
        // the project table is created. Then create an entry in the database for the project.
        // There will only ever be one entry in this table. So create cannot be called if the database
        // already exists. But verify will fail if one already exists, so we are good.


        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return new CreateResult<Project>(ResultCode.Failure, entity, "Failed to retrieve connection data.");

        var validateResult = await ValidateCreateAsync(project, entity, cancellationToken);
        if (validateResult.ResultCode == ResultCode.Failure)
            return new CreateResult<Project>(ResultCode.Failure, entity, "Validation failed.");

        string connectionString = connectionData.ConnectionString;
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Projects (Name) VALUES ($name);";
        command.Parameters.AddWithValue("$name", entity.Name);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return new CreateResult<Project>(ResultCode.Success, entity);
    }

    public Task<DeleteResult> DeleteAsync(string project, uint id, CancellationToken cancellationToken = default)
    {
        // TODO: Not sure if this will be here or not

        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return Task.FromResult(new DeleteResult(ResultCode.Failure, id, "Failed to retrieve connection data."));

        string filePath = connectionData.GetFilePath();
        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.FromResult(new DeleteResult(ResultCode.Success, id));
    }

    public async Task<UpdateResult<Project>> UpdateAsync(string project, Project entity, CancellationToken cancellationToken = default)
    {
        // TODO: call verify. If success, then rename the database if the projectname is different.
        // then update the project name in the database. 
        // Right now there is only one field, but if there are other fields later, update those.

        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return new UpdateResult<Project>(ResultCode.Failure, entity, "Failed to retrieve connection data.");

        var validateResult = await ValidateUpdateAsync(project, entity, cancellationToken);
        if (validateResult.ResultCode == ResultCode.Failure)
            return new UpdateResult<Project>(ResultCode.Failure, entity, "Validation failed.");

        string connectionString = connectionData.ConnectionString;
        string oldFilePath = connectionString.Replace("Data Source=", "");
        string newFilePath = Path.Combine(Path.GetDirectoryName(oldFilePath)!, $"{entity.Name}{SqliteConnectionData.PROJECT_EXTENSION}");

        //if (oldFilePath != newFilePath)
        //{
        //    await connection.CloseAsync();
        //    File.Move(oldFilePath, newFilePath);
        //    connectionData.ConnectionString = $"Data Source={newFilePath}";
        //    _connectionCache.UpdateConnectionData(project, connectionData);
        //}

        //using var connection = new SqliteConnection(connectionData.ConnectionString);
        //await connection.OpenAsync(cancellationToken);

        //using var cmd = connection.CreateCommand();
        //cmd.CommandText = "UPDATE Projects SET Name = $newName WHERE Name = $oldName";
        //cmd.Parameters.AddWithValue("$newName", entity.Name);
        //cmd.Parameters.AddWithValue("$oldName", project);
        //var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);

        //if (rowsAffected == 0)
        //    return new UpdateResult<Project>(ResultCode.Failure, entity, "Project not found.");

        return new UpdateResult<Project>(ResultCode.Success, entity);
    }

    public async Task<ValidateResult> ValidateCreateAsync(string project, Project entity, CancellationToken cancellationToken = default)
    {
        // TODO: Not sure we will ever use this. Decide what to do about this. For now, leave this as is.
        throw new NotImplementedException();
    }

    public async Task<ValidateResult> ValidateUpdateAsync(string project, Project entity, CancellationToken cancellationToken = default)
    {
        // TODO: Not sure we will ever use this. Decide what to do about this. For now, leave this as is.
        throw new NotImplementedException();
    }

    public async Task<GetManyResult<Project>> GetAllAsync(string project, CancellationToken cancellationToken = default)
    {
        // TODO: Not sure we will ever use this. Decide what to do about this. For now, leave this as is.
        throw new NotImplementedException();
    }

    public async Task<GetSingleResult<Project>> GetSingleAsync(string project, uint id, CancellationToken cancellationToken = default)
    {
        // TODO: Not sure we will ever use this. Decide what to do about this. For now, leave this as is.
        throw new NotImplementedException();
    }
}
