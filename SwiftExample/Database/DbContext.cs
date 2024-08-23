using System;
using System.Data.SQLite;

namespace SwiftExample.Database
{
    public class DbContext
    {
        private readonly string _connectionString;

        public DbContext(IConfiguration configuration)
        {
            var dbFolder = Path.Combine(Environment.CurrentDirectory, "Data");
            if (!Directory.Exists(dbFolder))
            {
                Directory.CreateDirectory(dbFolder); 
            }
            var dbPath = Path.Combine(dbFolder, "SwiftDatabase.db");
            _connectionString = $"Data Source={dbPath};Version=3;";
            Console.WriteLine($"Database Path: {dbPath}");
        }

        public void ExecuteCommand(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }
                    command.ExecuteNonQuery();
                }
            }
        }

        public void CreateDatabase()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Messages (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ReferenceNumber TEXT,
                    RelatedReference TEXT,
                    Narrative TEXT
                );";
                    command.ExecuteNonQuery();
                }
            }
        }

        public void InsertMessage(string referenceNumber, string relatedReference, string narrative)
        {
            string sql = "INSERT INTO Messages (ReferenceNumber, RelatedReference, Narrative) VALUES (@ReferenceNumber, @RelatedReference, @Narrative)";
            var parameters = new Dictionary<string, object>
            {
                {"@ReferenceNumber", referenceNumber},
                {"@RelatedReference", relatedReference},
                {"@Narrative", narrative}
            };
            ExecuteCommand(sql, parameters);
        }
    }
}
