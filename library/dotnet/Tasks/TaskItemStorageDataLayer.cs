using Microsoft.Data.Sqlite;
using Taskiea.Core.Results;
using Taskiea.Core.Tasks;

namespace Taskiea.Core;

public sealed class TaskStorageDataLayer : StorageDataLayer<TaskItem>
{
    public override void Initialize(ProjectConnectionData projectConnectionData)
    {
        string connectionString = GetConnectionString(projectConnectionData);

        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Tasks (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT NOT NULL,
                Status INTEGER NOT NULL
            );";
        command.ExecuteNonQuery();
    }

    public override async Task<CreateResult<TaskItem>> CreateAsync(ProjectConnectionData projectConnectionData, TaskItem dataObject, CancellationToken cancellationToken)
    {
        string connectionString = GetConnectionString(projectConnectionData);

        var validateResult = await ValidateCreateAsync(projectConnectionData, dataObject, cancellationToken);
        if (validateResult.ResultCode == ResultCode.Failure)
            return new CreateResult<TaskItem>(ResultCode.Failure, dataObject, "Validation failed.");

        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Tasks (Name, Description, Status) VALUES ($name, $description, $status); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$name", dataObject.Name);
        command.Parameters.AddWithValue("$description", dataObject.Description);
        command.Parameters.AddWithValue("$status", dataObject.Status);
        dataObject.Id = Convert.ToUInt32(await command.ExecuteScalarAsync(cancellationToken));
        return new CreateResult<TaskItem>(ResultCode.Success, dataObject);
    }

    public override async Task<DeleteResult> DeleteAsync(ProjectConnectionData projectConnectionData, uint id, CancellationToken cancellationToken)
    {
        string connectionString = GetConnectionString(projectConnectionData);

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

    public override async Task<UpdateResult<TaskItem>> UpdateAsync(ProjectConnectionData projectConnectionData, TaskItem dataObject, CancellationToken cancellationToken)
    {
        string connectionString = GetConnectionString(projectConnectionData);

        var validateResult = await ValidateUpdateAsync(projectConnectionData, dataObject, cancellationToken);
        if (validateResult.ResultCode == ResultCode.Failure)
            return new UpdateResult<TaskItem>(ResultCode.Failure, dataObject, "Validation failed.");

        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE Tasks SET Name = $name, Description = $description, Status = $status WHERE Id = $id";
        cmd.Parameters.AddWithValue("$name", dataObject.Name);
        cmd.Parameters.AddWithValue("$description", dataObject.Description);
        cmd.Parameters.AddWithValue("$status", dataObject.Status);
        cmd.Parameters.AddWithValue("$id", dataObject.Id);
        var rowsAffected = await cmd.ExecuteNonQueryAsync();

        if (rowsAffected == 0)
            return new UpdateResult<TaskItem>(ResultCode.Failure, dataObject, "Task not found.");

        return new UpdateResult<TaskItem>(ResultCode.Success, dataObject);
    }

    public override async Task<ValidateResult> ValidateCreateAsync(ProjectConnectionData projectConnectionData, TaskItem dataObject, CancellationToken cancellationToken)
    {
        string connectionString = GetConnectionString(projectConnectionData);

        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Tasks WHERE Name = $name";
        command.Parameters.AddWithValue("$name", dataObject.Name);
        var count = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        if (count != 0)
        {
            ValidateError validateError = new ValidateError(nameof(TaskItem.Name), "The name already exists and cannot be created.");
            return new ValidateResult(ResultCode.Failure, dataObject.Id, validateError);
        }
        return new ValidateResult(ResultCode.Success, dataObject.Id);
    }

    public override async Task<ValidateResult> ValidateUpdateAsync(ProjectConnectionData projectConnectionData, TaskItem dataObject, CancellationToken cancellationToken)
    {
        string connectionString = GetConnectionString(projectConnectionData);

        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Tasks WHERE Id = $id";
        command.Parameters.AddWithValue("$id", dataObject.Id);
        var count = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        if (count == 0)
        {
            ValidateError validateError = new ValidateError(nameof(TaskItem.Id), "The id does not exist.");
            return new ValidateResult(ResultCode.Failure, dataObject.Id, validateError);
        }
        command.CommandText = "SELECT COUNT(*) FROM Tasks WHERE Name = $name AND Id != $id";
        command.Parameters.Clear();
        command.Parameters.AddWithValue("$name", dataObject.Name);
        command.Parameters.AddWithValue("$id", dataObject.Id);
        object? x = await command.ExecuteScalarAsync(cancellationToken);
        count = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        if (count != 0)
        {
            ValidateError validateError = new ValidateError(nameof(TaskItem.Name), "The name already exists.");
            return new ValidateResult(ResultCode.Failure, dataObject.Id, validateError);
        }
        return new ValidateResult(ResultCode.Success, dataObject.Id);
    }

    public override async Task<GetSingleResult<TaskItem>> GetSingleAsync(ProjectConnectionData projectConnectionData, uint id, CancellationToken cancellationToken)
    {
        string connectionString = GetConnectionString(projectConnectionData);

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

    public override async Task<GetManyResult<TaskItem>> GetAllAsync(ProjectConnectionData projectConnectionData, CancellationToken cancellationToken)
    {
        string connectionString = GetConnectionString(projectConnectionData);

        var tasks = new List<TaskItem>();
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Description, Status FROM Tasks";
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