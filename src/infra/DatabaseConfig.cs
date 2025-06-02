using Microsoft.Data.Sqlite;

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
                Password TEXT NOT NULL,
                Role TEXT NOT NULL DEFAULT 'User',
                CreatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Services (
                Id TEXT PRIMARY KEY,
                Key TEXT NOT NULL UNIQUE,
                Title TEXT NOT NULL,
                Description TEXT NOT NULL,
                FeaturesTitle TEXT NOT NULL,
                DetailsTitle TEXT NOT NULL,
                PackagesTitle TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ServiceFeatures (
                Id TEXT PRIMARY KEY,
                ServiceId TEXT NOT NULL,
                Text TEXT NOT NULL,
                [Order] INTEGER NOT NULL,
                FOREIGN KEY (ServiceId) REFERENCES Services (Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS ServiceDetails (
                Id TEXT PRIMARY KEY,
                ServiceId TEXT NOT NULL,
                Text TEXT NOT NULL,
                [Order] INTEGER NOT NULL,
                FOREIGN KEY (ServiceId) REFERENCES Services (Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Packages (
                Id TEXT PRIMARY KEY,
                ServiceId TEXT NOT NULL,
                Name TEXT NOT NULL,
                Price TEXT NOT NULL,
                IsPopular INTEGER NOT NULL DEFAULT 0,
                [Order] INTEGER NOT NULL,
                CreatedAt TEXT NOT NULL,
                FOREIGN KEY (ServiceId) REFERENCES Services (Id) ON DELETE CASCADE
            )";

    command.ExecuteNonQuery();
  }
}
