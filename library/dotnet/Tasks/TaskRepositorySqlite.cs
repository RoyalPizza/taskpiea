using Microsoft.Data.Sqlite;
using Taskpiea.Core.Connections;
using Taskpiea.Core.Results;
using Taskpiea.Core.Tasks;

namespace Taskpiea.Core;

public sealed class TaskRepositorySqlite : BaseRepository, ITaskRepository
{
    public TaskRepositorySqlite(IConnectionCache connectionCache) : base(connectionCache)
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
            CREATE TABLE IF NOT EXISTS Tasks (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT NOT NULL,
                Status INTEGER NOT NULL,
                AssigneeId INTEGER NULL,
                FOREIGN KEY (AssigneeId) REFERENCES Users(Id)
            );";
        command.ExecuteNonQuery();
    }

    public async Task<CreateResult<TaskItem>> CreateAsync(string project, TaskItem entity, CancellationToken cancellationToken = default)
    {
        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return new CreateResult<TaskItem>(ResultCode.Failure, entity, "Failed to retrieve connection data.");

        var validateResult = await ValidateCreateAsync(project, entity, cancellationToken);
        if (validateResult.ResultCode == ResultCode.Failure)
            return new CreateResult<TaskItem>(ResultCode.Failure, entity, "Validation failed.");

        string connectionString = connectionData.ConnectionString;
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Tasks (Name, Description, Status) VALUES ($name, $description, $status); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$name", entity.Name);
        command.Parameters.AddWithValue("$description", entity.Description);
        command.Parameters.AddWithValue("$status", entity.Status);
        entity.Id = Convert.ToUInt32(await command.ExecuteScalarAsync(cancellationToken));
        return new CreateResult<TaskItem>(ResultCode.Success, entity);
    }

    public async Task<DeleteResult> DeleteAsync(string project, uint id, CancellationToken cancellationToken = default)
    {
        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return new DeleteResult(ResultCode.Failure, id, "Failed to retrieve connection data.");

        string connectionString = connectionData.ConnectionString;
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Tasks WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);
        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected == 0)
            return new DeleteResult(ResultCode.Failure, id, "Failed to delete record.");
        return new DeleteResult(ResultCode.Success, id);
    }

    public async Task<UpdateResult<TaskItem>> UpdateAsync(string project, TaskItem entity, CancellationToken cancellationToken = default)
    {
        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return new UpdateResult<TaskItem>(ResultCode.Failure, entity, "Failed to retrieve connection data.");

        var validateResult = await ValidateUpdateAsync(project, entity, cancellationToken);
        if (validateResult.ResultCode == ResultCode.Failure)
            return new UpdateResult<TaskItem>(ResultCode.Failure, entity, "Validation failed.");

        string connectionString = connectionData.ConnectionString;
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE Tasks SET Name = $name, Description = $description, Status = $status WHERE Id = $id";
        cmd.Parameters.AddWithValue("$name", entity.Name);
        cmd.Parameters.AddWithValue("$description", entity.Description);
        cmd.Parameters.AddWithValue("$status", entity.Status);
        cmd.Parameters.AddWithValue("$id", entity.Id);
        var rowsAffected = await cmd.ExecuteNonQueryAsync();

        if (rowsAffected == 0)
            return new UpdateResult<TaskItem>(ResultCode.Failure, entity, "Task not found.");

        return new UpdateResult<TaskItem>(ResultCode.Success, entity);
    }

    public async Task<ValidateResult> ValidateCreateAsync(string project, TaskItem entity, CancellationToken cancellationToken = default)
    {
        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return new ValidateResult(ResultCode.Failure, entity.GetId(), "Failed to retrieve connection data.");

        string connectionString = connectionData.ConnectionString;
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Tasks WHERE Name = $name";
        command.Parameters.AddWithValue("$name", entity.Name);
        var count = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        if (count != 0)
        {
            ValidateError validateError = new ValidateError(nameof(TaskItem.Name), "The name already exists and cannot be created.");
            return new ValidateResult(ResultCode.Failure, entity.Id, validateError);
        }
        return new ValidateResult(ResultCode.Success, entity.Id);
    }

    public async Task<ValidateResult> ValidateUpdateAsync(string project, TaskItem entity, CancellationToken cancellationToken = default)
    {
        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return new ValidateResult(ResultCode.Failure, entity.GetId(), "Failed to retrieve connection data.");

        string connectionString = connectionData.ConnectionString;
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Tasks WHERE Id = $id";
        command.Parameters.AddWithValue("$id", entity.Id);
        var count = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        if (count == 0)
        {
            ValidateError validateError = new ValidateError(nameof(TaskItem.Id), "The id does not exist.");
            return new ValidateResult(ResultCode.Failure, entity.Id, validateError);
        }
        command.CommandText = "SELECT COUNT(*) FROM Tasks WHERE Name = $name AND Id != $id";
        command.Parameters.Clear();
        command.Parameters.AddWithValue("$name", entity.Name);
        command.Parameters.AddWithValue("$id", entity.Id);
        object? x = await command.ExecuteScalarAsync(cancellationToken);
        count = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        if (count != 0)
        {
            ValidateError validateError = new ValidateError(nameof(TaskItem.Name), "The name already exists.");
            return new ValidateResult(ResultCode.Failure, entity.Id, validateError);
        }
        return new ValidateResult(ResultCode.Success, entity.Id);
    }

    public async Task<GetSingleResult<TaskItem>> GetSingleAsync(string project, uint id, CancellationToken cancellationToken = default)
    {
        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return new GetSingleResult<TaskItem>(ResultCode.Failure, id, null, "Failed to retrieve connection data.");

        string connectionString = connectionData.ConnectionString;
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Description, Status FROM Tasks WHERE Id = $id LIMIT 1";
        command.Parameters.AddWithValue("$id", id);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var taskItem = new TaskItem
            {
                Id = (uint)reader.GetInt64(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Status = (Status)reader.GetInt32(3)
            };
            return new GetSingleResult<TaskItem>(ResultCode.Success, id, taskItem);
        }
        return new GetSingleResult<TaskItem>(ResultCode.Failure, id, null, "Failed to get task from storage.");
    }

    public async Task<GetManyResult<TaskItem>> GetAllAsync(string project, CancellationToken cancellationToken = default)
    {
        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return new GetManyResult<TaskItem>(ResultCode.Failure, new List<TaskItem>(), "Failed to retrieve connection data.");

        string connectionString = connectionData.ConnectionString;
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Description, Status FROM Tasks";

        var tasks = new List<TaskItem>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tasks.Add(new TaskItem
            {
                Id = (uint)reader.GetInt64(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Status = (Status)reader.GetInt32(3)
            });
        }
        return new GetManyResult<TaskItem>(ResultCode.Success, tasks);
    }
}