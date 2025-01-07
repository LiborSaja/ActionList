using Npgsql;

namespace ActionList.Service {
    public class DatabaseService {
        private readonly string _connectionString;
        private readonly string _databaseName;

        public DatabaseService(string connectionString, string databaseName) {
            _connectionString = connectionString;
            _databaseName = databaseName;
        }

        public NpgsqlConnection CreateConnection() {
            var connectionString = $"{_connectionString};Database={_databaseName}";
            var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public void InitializeDatabase() {
            // Připojení bez specifikace databáze pro vytvoření databáze
            using (var connection = new NpgsqlConnection(_connectionString)) {
                connection.Open();

                using (var command = connection.CreateCommand()) {
                    // Kontrola a vytvoření databáze
                    command.CommandText = $@"
                        SELECT 1 FROM pg_database WHERE datname = '{_databaseName}';
                        ";
                    var exists = command.ExecuteScalar();

                    if (exists == null) {
                        Console.WriteLine($"Databáze '{_databaseName}' neexistuje. Vytvářím ji...");
                        command.CommandText = $"CREATE DATABASE \"{_databaseName}\";";
                        command.ExecuteNonQuery();
                        Console.WriteLine($"Databáze '{_databaseName}' byla vytvořena.");
                    }
                    else {
                        Console.WriteLine($"Databáze '{_databaseName}' již existuje.");
                    }
                }
            }

            // Připojení ke konkrétní databázi pro vytvoření tabulky
            using (var connection = new NpgsqlConnection($"{_connectionString};Database={_databaseName}")) {
                connection.Open();
                using (var command = connection.CreateCommand()) {
                    Console.WriteLine($"Připojeno k databázi: {_databaseName}");

                    // Vytvoření tabulky
                    command.CommandText = @"
            CREATE TABLE IF NOT EXISTS todo (
                Id UUID PRIMARY KEY,
                Title TEXT NOT NULL,
                State INTEGER NOT NULL,
                Content TEXT
            );
            ";
                    Console.WriteLine("Provádím příkaz:");
                    Console.WriteLine(command.CommandText);

                    command.ExecuteNonQuery();
                    Console.WriteLine("Tabulka 'Todo' byla vytvořena (pokud neexistovala).");
                }
            }
        }

    }
}
