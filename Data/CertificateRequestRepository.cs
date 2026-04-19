using MauiApp1.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MauiApp1.Data
{
    public class CertificateRequestRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;

        public CertificateRequestRepository(ILogger<CertificateRequestRepository> logger)
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
            CREATE TABLE IF NOT EXISTS CertificateRequest (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId TEXT NOT NULL,
                CertificateType TEXT NOT NULL,
                Purpose TEXT NOT NULL,
                Status INTEGER NOT NULL DEFAULT 0,
                RequestDate TEXT NOT NULL,
                ProcessedDate TEXT,
                AdminNotes TEXT,
                FOREIGN KEY (UserId) REFERENCES AppUser(Id)
            );";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating CertificateRequest table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<List<CertificateRequest>> ListAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"
                SELECT cr.*, au.Email as RequesterEmail, au.Role as RequesterRole
                FROM CertificateRequest cr
                LEFT JOIN AppUser au ON cr.UserId = au.Id
                ORDER BY cr.RequestDate DESC";

            var requests = new List<CertificateRequest>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                requests.Add(MapFromReader(reader));
            }

            return requests;
        }

        public async Task<List<CertificateRequest>> ListByUserIdAsync(string userId)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"
                SELECT cr.*, au.Email as RequesterEmail, au.Role as RequesterRole
                FROM CertificateRequest cr
                LEFT JOIN AppUser au ON cr.UserId = au.Id
                WHERE cr.UserId = @userId
                ORDER BY cr.RequestDate DESC";
            selectCmd.Parameters.AddWithValue("@userId", userId);

            var requests = new List<CertificateRequest>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                requests.Add(MapFromReader(reader));
            }

            return requests;
        }

        public async Task<List<CertificateRequest>> ListPendingAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"
                SELECT cr.*, au.Email as RequesterEmail, au.Role as RequesterRole
                FROM CertificateRequest cr
                LEFT JOIN AppUser au ON cr.UserId = au.Id
                WHERE cr.Status = @status
                ORDER BY cr.RequestDate DESC";
            selectCmd.Parameters.AddWithValue("@status", (int)CertificateStatus.Pending);

            var requests = new List<CertificateRequest>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                requests.Add(MapFromReader(reader));
            }

            return requests;
        }

        public async Task<CertificateRequest?> GetAsync(int id)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"
                SELECT cr.*, au.Email as RequesterEmail, au.Role as RequesterRole
                FROM CertificateRequest cr
                LEFT JOIN AppUser au ON cr.UserId = au.Id
                WHERE cr.Id = @id";
            selectCmd.Parameters.AddWithValue("@id", id);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }

            return null;
        }

        public async Task<int> SaveAsync(CertificateRequest request)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();
            if (request.Id == 0)
            {
                saveCmd.CommandText = @"
                INSERT INTO CertificateRequest (UserId, CertificateType, Purpose, Status, RequestDate, ProcessedDate, AdminNotes)
                VALUES (@UserId, @CertificateType, @Purpose, @Status, @RequestDate, @ProcessedDate, @AdminNotes);
                SELECT last_insert_rowid();";
            }
            else
            {
                saveCmd.CommandText = @"
                UPDATE CertificateRequest
                SET UserId = @UserId, CertificateType = @CertificateType, Purpose = @Purpose,
                    Status = @Status, RequestDate = @RequestDate, ProcessedDate = @ProcessedDate, AdminNotes = @AdminNotes
                WHERE Id = @Id";
                saveCmd.Parameters.AddWithValue("@Id", request.Id);
            }

            saveCmd.Parameters.AddWithValue("@UserId", request.UserId);
            saveCmd.Parameters.AddWithValue("@CertificateType", request.CertificateType);
            saveCmd.Parameters.AddWithValue("@Purpose", request.Purpose);
            saveCmd.Parameters.AddWithValue("@Status", (int)request.Status);
            saveCmd.Parameters.AddWithValue("@RequestDate", request.RequestDate.ToString("O"));
            saveCmd.Parameters.AddWithValue("@ProcessedDate", request.ProcessedDate?.ToString("O") ?? (object)DBNull.Value);
            saveCmd.Parameters.AddWithValue("@AdminNotes", request.AdminNotes ?? (object)DBNull.Value);

            var result = await saveCmd.ExecuteScalarAsync();
            if (request.Id == 0 && result != null)
            {
                request.Id = Convert.ToInt32(result);
            }

            return request.Id;
        }

        public async Task UpdateStatusAsync(int id, CertificateStatus status, string? adminNotes = null)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = @"
                UPDATE CertificateRequest
                SET Status = @Status, ProcessedDate = @ProcessedDate, AdminNotes = @AdminNotes
                WHERE Id = @Id";
            updateCmd.Parameters.AddWithValue("@Id", id);
            updateCmd.Parameters.AddWithValue("@Status", (int)status);
            updateCmd.Parameters.AddWithValue("@ProcessedDate", DateTime.UtcNow.ToString("O"));
            updateCmd.Parameters.AddWithValue("@AdminNotes", adminNotes ?? (object)DBNull.Value);

            await updateCmd.ExecuteNonQueryAsync();
        }

        public async Task<int> DeleteAsync(int id)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM CertificateRequest WHERE Id = @id";
            deleteCmd.Parameters.AddWithValue("@id", id);

            return await deleteCmd.ExecuteNonQueryAsync();
        }

        public async Task DropTableAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var dropCmd = connection.CreateCommand();
            dropCmd.CommandText = "DROP TABLE IF EXISTS CertificateRequest";
            await dropCmd.ExecuteNonQueryAsync();
            _hasBeenInitialized = false;
        }

        private CertificateRequest MapFromReader(SqliteDataReader reader)
        {
            var request = new CertificateRequest
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetString(1),
                CertificateType = reader.GetString(2),
                Purpose = reader.GetString(3),
                Status = (CertificateStatus)reader.GetInt32(4),
                RequestDate = DateTime.Parse(reader.GetString(5)),
                ProcessedDate = reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6)),
                AdminNotes = reader.IsDBNull(7) ? null : reader.GetString(7)
            };

            // Map requester info if available
            if (!reader.IsDBNull(8))
            {
                request.Requester = new User
                {
                    Id = request.UserId,
                    Email = reader.GetString(8),
                    Role = (UserRole)reader.GetInt32(9)
                };
            }

            return request;
        }
    }
}
