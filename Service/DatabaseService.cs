using Npgsql;

namespace ActionList.Service {
    public class DatabaseService {
        private readonly string _connectionString;
        private readonly string _databaseName;

        public DatabaseService(string connectionString, string databaseName) {
            _connectionString = connectionString;
            _databaseName = databaseName;
        }

        // metoda pro vytvoření připojení k DB
        public NpgsqlConnection CreateConnection() {
            var fullConnectionString = $"{_connectionString};Database={_databaseName}";
            var connection = new NpgsqlConnection(fullConnectionString);
            connection.Open();
            return connection;
        }

        // inicializace DB
        public async Task InitializeDatabaseAsync() {
            await EnsureDatabaseExistsAsync();
            await EnsureTableExistsAsync();
            await SeedSampleDataAsync();
        }

        // ověření existence DB, jinak vytvoření nové
        public async Task EnsureDatabaseExistsAsync() {
            using (var connection = new NpgsqlConnection(_connectionString)) {
                await connection.OpenAsync();
                // příprava SQL příkazu pro kontrolu existence DB
                using (var command = connection.CreateCommand()) {
                    command.CommandText = $@"
                SELECT 1 FROM pg_database WHERE datname = '{_databaseName}';
            ";
                    //provedení SQL příkazu, výstup: 1 - DB existuje -> nic; null - DB neexistuje -> vytvoří novou
                    var exists = await command.ExecuteScalarAsync();
                    if (exists == null) {
                        command.CommandText = $"CREATE DATABASE \"{_databaseName}\";";
                        await command.ExecuteNonQueryAsync();
                        Console.WriteLine($"Database '{_databaseName}' created.");
                    }
                    else {
                        Console.WriteLine($"Database '{_databaseName}' already exists.");

                    }
                }
            }
        }

        // ověření existence tabulky v DB, jinak vytvoření nové
        private async Task EnsureTableExistsAsync() {
            try {

                using (var connection = new NpgsqlConnection($"{_connectionString};Database={_databaseName}")) {
                    await connection.OpenAsync();
                    // příprava SQL příkazu pro vytvoření tabulky
                    using (var command = connection.CreateCommand()) {
                        command.CommandText = @"
                CREATE TABLE IF NOT EXISTS todo (
                    Id UUID PRIMARY KEY,
                    Title TEXT NOT NULL,
                    State INTEGER NOT NULL,
                    Content TEXT NOT NULL
                );
            ";
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Create table error: {ex.Message}");
                throw;
            }
        }


        // vložení ukázkových dat do tabulky, pokud neexistují
        private async Task SeedSampleDataAsync() {
            using (var connection = new NpgsqlConnection($"{_connectionString};Database={_databaseName}")) {
                await connection.OpenAsync();
                // kontrola počtu objektů v DB, pokud null -> vloží ukázkové objekty
                using (var command = connection.CreateCommand()) {
                    command.CommandText = "SELECT COUNT(*) FROM todo;";
                    var recordCount = (long)await command.ExecuteScalarAsync();
                    if (recordCount == 0) {
                        command.CommandText = @"
                    INSERT INTO todo (Id, Title, State, Content)
                    VALUES
                        ('01945bee-cffd-74ed-af50-c747564504f5', 'Develop Backend', 3, 'Develop backend service in C# with connection to PostgreSQL database, using no ORM.'),
                        ('01945bf0-048c-7075-84f0-e78da0da19c6', 'Develop Frontend', 3, 'Develop frontend service using Vue.js and Vite. Connect to backend.'),
                        ('01945bf0-60e4-7b21-a0a2-1c26752db7b6', 'Dockerize', 3, 'Ensure that the entire project is executable in Docker.'),
                        ('01945bf0-b9ad-7446-b1ac-c4634dd65d22', 'Get job as Developer', 1, ' : ) ');
                ";
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

    }
}
