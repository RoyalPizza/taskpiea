using Microsoft.Data.Sqlite;
using Taskpiea.Core.Connections;
using Taskpiea.Core.Results;

namespace Taskpiea.Core.Accounts;

public sealed class UserRepositorySqlite : BaseRepository, IUserRepository
{
    public UserRepositorySqlite(IConnectionCache connectionCache) : base(connectionCache) { }

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
        CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL
        );";
        command.ExecuteNonQuery();
    }

    public async Task<CRUDResult<User>> CreateAsync(string project, User entity, CancellationToken cancellationToken = default)
    {
        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return new CRUDResult<User>(ResultCode.Failure, entity, "Failed to retrieve connection data.");

        var validateResult = await ValidateCreateAsync(project, entity, cancellationToken);
        if (validateResult.ResultCode == ResultCode.Failure)
            return new CRUDResult<User>(ResultCode.Failure, entity, "Validation failed.");

        string connectionString = connectionData.ConnectionString;
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Users (Name) VALUES ($name); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$name", entity.Name);

        var id = Convert.ToUInt32(await command.ExecuteScalarAsync(cancellationToken));
        entity.Id = id;

        return new CRUDResult<User>(ResultCode.Success, entity);
    }

    public async Task<CRUDResult<User>> DeleteAsync(string project, uint id, CancellationToken cancellationToken = default)
    {
        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return new CRUDResult<User>(ResultCode.Failure, id, "Failed to retrieve connection data.");

        string connectionString = connectionData.ConnectionString;
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Users WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);
        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

        if (rowsAffected == 0)
            return new CRUDResult<User>(ResultCode.Failure, id, "Failed to delete record.");

        return new CRUDResult<User>(ResultCode.Success, id);
    }

    public async Task<CRUDResult<User>> UpdateAsync(string project, User entity, CancellationToken cancellationToken = default)
    {
        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return new CRUDResult<User>(ResultCode.Failure, entity, "Failed to retrieve connection data.");

        var validateResult = await ValidateUpdateAsync(project, entity, cancellationToken);
        if (validateResult.ResultCode == ResultCode.Failure)
            return new CRUDResult<User>(ResultCode.Failure, entity, "Validation failed.");

        string connectionString = connectionData.ConnectionString;
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE Users SET Name = $name WHERE Id = $id";
        cmd.Parameters.AddWithValue("$name", entity.Name);
        cmd.Parameters.AddWithValue("$id", entity.Id);
        var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);

        if (rowsAffected == 0)
            return new CRUDResult<User>(ResultCode.Failure, entity, "User not found.");

        return new CRUDResult<User>(ResultCode.Success, entity);
    }

    public async Task<ValidateResult> ValidateCreateAsync(string project, User entity, CancellationToken cancellationToken = default)
    {
        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return new ValidateResult(ResultCode.Failure, entity.GetId(), "Failed to retrieve connection data.");

        string connectionString = connectionData.ConnectionString;
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Users WHERE Name = $name";
        command.Parameters.AddWithValue("$name", entity.Name);
        var count = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        if (count != 0)
        {
            ValidateError validateError = new ValidateError(nameof(User.Name), "The name already exists and cannot be created.");
            return new ValidateResult(ResultCode.Failure, entity.Id, validateError);
        }

        // TODO: the Id will always be 0 because create has not been called yet. Decided what to do about that.
        return new ValidateResult(ResultCode.Success, entity.Id);
    }

    public async Task<ValidateResult> ValidateUpdateAsync(string project, User entity, CancellationToken cancellationToken = default)
    {
        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return new ValidateResult(ResultCode.Failure, entity.GetId(), "Failed to retrieve connection data.");

        string connectionString = connectionData.ConnectionString;
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        // Make sure that Id exist
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Users WHERE Id = $id";
        command.Parameters.AddWithValue("$id", entity.Id);
        var count = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        if (count == 0)
        {
            ValidateError validateError = new ValidateError(nameof(User.Id), "The id does not exist..");
            return new ValidateResult(ResultCode.Failure, entity.Id, validateError);
        }

        // make sure the new name does not exist already
        command.CommandText = "SELECT COUNT(*) FROM Users WHERE Name = $name AND Id != $id";
        command.Parameters.Clear();
        command.Parameters.AddWithValue("$name", entity.Name);
        command.Parameters.AddWithValue("$id", entity.Id);

        count = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        if (count != 0)
        {
            ValidateError validateError = new ValidateError(nameof(User.Name), "The name already exists and cannot be created.");
            return new ValidateResult(ResultCode.Failure, entity.Id, validateError);
        }

        // check if name already exists
        return new ValidateResult(ResultCode.Success, entity.Id);
    }

    public async Task<GetSingleResult<User>> GetSingleAsync(string project, uint id, CancellationToken cancellationToken = default)
    {
        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return new GetSingleResult<User>(ResultCode.Failure, id, null, "Failed to retrieve connection data.");

        string connectionString = connectionData.ConnectionString;
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

    public async Task<GetManyResult<User>> GetAllAsync(string project, CancellationToken cancellationToken = default)
    {
        var connectionData = _connectionCache.GetConnectionData<SqliteConnectionData>(project);
        if (connectionData == null)
            return new GetManyResult<User>(ResultCode.Failure, new List<User>(), "Failed to retrieve connection data.");

        string connectionString = connectionData.ConnectionString;
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
