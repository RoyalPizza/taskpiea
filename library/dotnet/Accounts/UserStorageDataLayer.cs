using Microsoft.Data.Sqlite;
using Taskiea.Core.Results;

namespace Taskiea.Core.Accounts;

public sealed class UserStorageDataLayer : StorageDataLayer<User>
{
    public override void Initialize(ProjectConnectionData projectConnectionData)
    {
        string connectionString = GetConnectionString(projectConnectionData);

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL
            );";
        command.ExecuteNonQuery();
    }

    public override async Task<CreateResult<User>> CreateAsync(ProjectConnectionData projectConnectionData, User dataObject, CancellationToken cancellationToken)
    {
        string connectionString = GetConnectionString(projectConnectionData);

        var validateResult = await ValidateCreateAsync(projectConnectionData, dataObject, cancellationToken);
        if (validateResult.ResultCode == ResultCode.Failure)
            return new CreateResult<User>(ResultCode.Failure, dataObject, "Validation failed.");

        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Users (Name) VALUES ($name); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$name", dataObject.Name);

        var id = Convert.ToUInt32(await command.ExecuteScalarAsync(cancellationToken));
        dataObject.Id = id;

        return new CreateResult<User>(ResultCode.Success, dataObject);
    }

    public override async Task<DeleteResult> DeleteAsync(ProjectConnectionData projectConnectionData, uint id, CancellationToken cancellationToken)
    {
        string connectionString = GetConnectionString(projectConnectionData);

        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Users WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);
        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

        if (rowsAffected == 0)
            return new DeleteResult(ResultCode.Failure, id, "Failed to delete record.");

        return new DeleteResult(ResultCode.Success, id);
    }

    public override async Task<UpdateResult<User>> UpdateAsync(ProjectConnectionData projectConnectionData, User dataObject, CancellationToken cancellationToken)
    {
        string connectionString = GetConnectionString(projectConnectionData);

        var validateResult = await ValidateUpdateAsync(projectConnectionData, dataObject, cancellationToken);
        if (validateResult.ResultCode == ResultCode.Failure)
            return new UpdateResult<User>(ResultCode.Failure, dataObject, "Validation failed.");

        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE Users SET Name = $name WHERE Id = $id";
        cmd.Parameters.AddWithValue("$name", dataObject.Name);
        cmd.Parameters.AddWithValue("$id", dataObject.Id);
        var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);

        if (rowsAffected == 0)
            return new UpdateResult<User>(ResultCode.Failure, dataObject, "User not found.");

        return new UpdateResult<User>(ResultCode.Success, dataObject);
    }

    public override async Task<ValidateResult> ValidateCreateAsync(ProjectConnectionData projectConnectionData, User dataObject, CancellationToken cancellationToken)
    {
        string connectionString = GetConnectionString(projectConnectionData);

        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Users WHERE Name = $name";
        command.Parameters.AddWithValue("$name", dataObject.Name);
        var count = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        if (count != 0)
        {
            ValidateError validateError = new ValidateError(nameof(User.Name), "The name already exists and cannot be created.");
            return new ValidateResult(ResultCode.Failure, dataObject.Id, validateError);
        }

        // TODO: the Id will always be 0 because create has not been called yet. Decided what to do about that.
        return new ValidateResult(ResultCode.Success, dataObject.Id);
    }

    public override async Task<ValidateResult> ValidateUpdateAsync(ProjectConnectionData projectConnectionData, User dataObject, CancellationToken cancellationToken)
    {
        string connectionString = GetConnectionString(projectConnectionData);

        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        // Make sure that Id exist
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Users WHERE Id = $id";
        command.Parameters.AddWithValue("$id", dataObject.Id);
        var count = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        if (count == 0)
        {
            ValidateError validateError = new ValidateError(nameof(User.Id), "The id does not exist..");
            return new ValidateResult(ResultCode.Failure, dataObject.Id, validateError);
        }

        // make sure the new name does not exist already
        command.CommandText = "SELECT COUNT(*) FROM Users WHERE Name = $name AND Id != $id";
        command.Parameters.Clear();
        command.Parameters.AddWithValue("$name", dataObject.Name);
        command.Parameters.AddWithValue("$id", dataObject.Id);

        count = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        if (count != 0)
        {
            ValidateError validateError = new ValidateError(nameof(User.Name), "The name already exists and cannot be created.");
            return new ValidateResult(ResultCode.Failure, dataObject.Id, validateError);
        }

        // check if name already exists
        return new ValidateResult(ResultCode.Success, dataObject.Id);
    }

    public override async Task<GetSingleResult<User>> GetSingleAsync(ProjectConnectionData projectConnectionData, uint id, CancellationToken cancellationToken)
    {
        string connectionString = GetConnectionString(projectConnectionData);

        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name FROM Users WHERE Id = $id LIMIT 1";
        command.Parameters.AddWithValue("$id", id);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            var user = new User
            {
                Id = (uint)reader.GetInt64(0),
                Name = reader.GetString(1)
            };
            return new GetSingleResult<User>(ResultCode.Success, id, user);
        }
        return new GetSingleResult<User>(ResultCode.Failure, id, null, "Failed to get user from storage.");
    }

    public override async Task<GetManyResult<User>> GetAllAsync(ProjectConnectionData projectConnectionData, CancellationToken cancellationToken)
    {
        string connectionString = GetConnectionString(projectConnectionData);

        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name FROM Users";

        var users = new List<User>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(new User
            {
                Id = (uint)reader.GetInt64(0),
                Name = reader.GetString(1)
            });
        }

        return new GetManyResult<User>(ResultCode.Success, users);
    }
}
