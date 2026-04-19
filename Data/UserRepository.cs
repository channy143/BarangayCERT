using MauiApp1.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MauiApp1.Data
{
    public class UserRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;

        public UserRepository(ILogger<UserRepository> logger)
        {
            _logger = logger;
        }

        private async Task Init()
        {
            if (_hasBeenInitialized)
                return;

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS AppUser (
                Id TEXT PRIMARY KEY,
                Email TEXT NOT NULL,
                Role INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                LastLoginAt TEXT NOT NULL
            );";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating AppUser table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<List<User>> ListAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM AppUser";
            var users = new List<User>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = reader.GetString(0),
                    Email = reader.GetString(1),
                    Role = (UserRole)reader.GetInt32(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    LastLoginAt = DateTime.Parse(reader.GetString(4))
                });
            }

            return users;
        }

        public async Task<User?> GetAsync(string id)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM AppUser WHERE Id = @id";
            selectCmd.Parameters.AddWithValue("@id", id);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetString(0),
                    Email = reader.GetString(1),
                    Role = (UserRole)reader.GetInt32(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    LastLoginAt = DateTime.Parse(reader.GetString(4))
                };
            }

            return null;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM AppUser WHERE Email = @email";
            selectCmd.Parameters.AddWithValue("@email", email);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetString(0),
                    Email = reader.GetString(1),
                    Role = (UserRole)reader.GetInt32(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    LastLoginAt = DateTime.Parse(reader.GetString(4))
                };
            }

            return null;
        }

        public async Task<int> SaveAsync(User user)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var existing = await GetAsync(user.Id);

            var saveCmd = connection.CreateCommand();
            if (existing == null)
            {
                saveCmd.CommandText = @"
                INSERT INTO AppUser (Id, Email, Role, CreatedAt, LastLoginAt)
                VALUES (@Id, @Email, @Role, @CreatedAt, @LastLoginAt);";
            }
            else
            {
                saveCmd.CommandText = @"
                UPDATE AppUser
                SET Email = @Email, Role = @Role, LastLoginAt = @LastLoginAt
                WHERE Id = @Id";
            }

            saveCmd.Parameters.AddWithValue("@Id", user.Id);
            saveCmd.Parameters.AddWithValue("@Email", user.Email);
            saveCmd.Parameters.AddWithValue("@Role", (int)user.Role);
            saveCmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt.ToString("O"));
            saveCmd.Parameters.AddWithValue("@LastLoginAt", user.LastLoginAt.ToString("O"));

            return await saveCmd.ExecuteNonQueryAsync();
        }

        public async Task<int> DeleteAsync(string id)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM AppUser WHERE Id = @id";
            deleteCmd.Parameters.AddWithValue("@id", id);

            return await deleteCmd.ExecuteNonQueryAsync();
        }

        public async Task DropTableAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var dropCmd = connection.CreateCommand();
            dropCmd.CommandText = "DROP TABLE IF EXISTS AppUser";
            await dropCmd.ExecuteNonQueryAsync();
            _hasBeenInitialized = false;
        }
    }
}
