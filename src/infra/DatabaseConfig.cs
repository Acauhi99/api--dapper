using Microsoft.Data.Sqlite;

namespace api__dapper.infra;

public static class DatabaseConfig
{
  public static void InitializeDatabase()
  {
    using var connection = new SqliteConnection("Data Source=src/infra/db/app.db");
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Email TEXT NOT NULL UNIQUE,
                CreatedAt TEXT NOT NULL
            )";

    command.ExecuteNonQuery();
  }
}
