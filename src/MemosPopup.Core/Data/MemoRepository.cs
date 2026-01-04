using Microsoft.Data.Sqlite;
using MemosPopup.Core.Models;

namespace MemosPopup.Core.Data;

public class MemoRepository
{
    private readonly DatabaseContext _context;

    public static event EventHandler? MemosChanged;

    public MemoRepository()
    {
        _context = DatabaseContext.Instance;
    }

    private static void OnMemosChanged()
    {
        MemosChanged?.Invoke(null, EventArgs.Empty);
    }

    public async Task<int> AddMemoAsync(Memo memo)
    {
        using var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Memos (Title, Content, CreatedAt)
            VALUES (@title, @content, @createdAt);
            SELECT last_insert_rowid();";
        
        command.Parameters.AddWithValue("@title", memo.Title);
        command.Parameters.AddWithValue("@content", memo.Content);
        command.Parameters.AddWithValue("@createdAt", memo.CreatedAt.ToString("O"));

        var result = await command.ExecuteScalarAsync();
        OnMemosChanged();
        return Convert.ToInt32(result);
    }

    public async Task<List<Memo>> GetAllMemosAsync()
    {
        var memos = new List<Memo>();
        using var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Content, CreatedAt, UpdatedAt FROM Memos ORDER BY CreatedAt DESC";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            memos.Add(new Memo
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Content = reader.GetString(2),
                CreatedAt = DateTime.Parse(reader.GetString(3)),
                UpdatedAt = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4))
            });
        }

        return memos;
    }

    public async Task<bool> DeleteMemoAsync(int id)
    {
        using var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Memos WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateMemoAsync(Memo memo)
    {
        using var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Memos 
            SET Title = @title, Content = @content, UpdatedAt = @updatedAt
            WHERE Id = @id";
        
        command.Parameters.AddWithValue("@id", memo.Id);
        command.Parameters.AddWithValue("@title", memo.Title);
        command.Parameters.AddWithValue("@content", memo.Content);
        command.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("O"));

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}
