using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace api__dapper.infra;

public static class DatabaseConfig
{
  public static void InitializeDatabase(IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    using var connection = new SqliteConnection(connectionString);
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
