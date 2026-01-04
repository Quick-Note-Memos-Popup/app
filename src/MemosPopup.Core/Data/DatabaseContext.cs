using System.IO;
using Microsoft.Data.Sqlite;
using MemosPopup.Core.Models;

namespace MemosPopup.Core.Data;

public class DatabaseContext
{
    private readonly string _connectionString;
    private static DatabaseContext? _instance;
    private static readonly object _lock = new();

    public static DatabaseContext Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new DatabaseContext();
                }
            }
            return _instance;
        }
    }

    private DatabaseContext()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MemosPopup");
        
        Directory.CreateDirectory(appDataPath);
        var dbPath = Path.Combine(appDataPath, "memos.db");
        _connectionString = $"Data Source={dbPath}";
        
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Memos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Content TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT
            )";
        command.ExecuteNonQuery();
    }

    public SqliteConnection GetConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
