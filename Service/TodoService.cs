using ActionList.DTO;
using ActionList.Model;
using ActionList.Utility;

namespace ActionList.Service {
    public class TodoService {
        private readonly DatabaseService _databaseService;

        public TodoService(DatabaseService databaseService) {
            this._databaseService = databaseService;
        }

        //------------------------------------------------------------------------------------------------------------------------------ Get all tasks service

        #region Get all tasks service-method
        // získání seznamu objektů z DB dle jejich stavu, nebo vše
        public async Task<List<Todo>> GetTasksByStateAsync(string state) {
            using (var connection = _databaseService.CreateConnection()) {
                using (var command = connection.CreateCommand()) {
                    // příprava SQL dotazu podle stavu
                    command.CommandText = state switch {
                        "open" => "SELECT Id, Title, State, Content FROM todo WHERE State = 1;",
                        "inprogress" => "SELECT Id, Title, State, Content FROM todo WHERE State = 2;",
                        "finished" => "SELECT Id, Title, State, Content FROM todo WHERE State = 3;",
                        _ => "SELECT Id, Title, State, Content FROM todo;"
                    };

                    // asynchronní metoda pro provedení SQL dotazu a iteraci výsledků pomocí DbDataReader -> načtení dat z DB
                    using (var reader = await command.ExecuteReaderAsync()) {
                        var todos = new List<Todo>();
                        while (await reader.ReadAsync()) {
                            todos.Add(new Todo {
                                Id = reader.GetGuid(0),
                                Title = reader.GetString(1),
                                State = reader.GetInt32(2),
                                Content = reader.GetString(3)
                            });
                        }
                        return todos;
                    }
                }
            }
        }


        #endregion

        //----------------------------------------------------------------------------------------------------------------------------- Get task by id service

        #region Get task by id service-method
        // získání jednoho objektu z DB dle zadaného Id
        public async Task<Todo> GetTaskByIdAsync(Guid id) {
            using (var connection = _databaseService.CreateConnection()) {
                // příprava SQL dotazu
                using (var command = connection.CreateCommand()) {
                    command.CommandText = "SELECT Id, Title, State, Content FROM todo WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", id);

                    // asynchronní metoda pro provedení SQL dotazu a iteraci výsledků pomocí DbDataReader -> načtení dat z DB
                    using (var reader = await command.ExecuteReaderAsync()) {
                        if (await reader.ReadAsync()) {
                            return new Todo {
                                Id = reader.GetGuid(0),
                                Title = reader.GetString(1),
                                State = reader.GetInt32(2),
                                Content = reader.GetString(3)
                            };
                        }
                    }
                }
            }
            return null; // Pokud záznam neexistuje
        }
        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------- Create task service

        #region Save task to DB service-method
        // uložení objektu do DB
        public async Task<Todo> CreatedTaskAsync(TodoDto todoDto, int state) {
            // mapování DTO na základní model
            var todo = new Todo {
                Id = Uuid7Generator.GenerateUuid7(),
                Title = todoDto.Title,
                State = state,
                Content = todoDto.Content
            };

            using (var connection = _databaseService.CreateConnection()) {
                // příprava SQL dotazu
                using (var command = connection.CreateCommand()) {
                    command.CommandText = @"
                INSERT INTO todo (Id, Title, State, Content)
                VALUES (@Id, @Title, @State, @Content);
            ";
                    // přidání parametrů - hodnot vlastností
                    command.Parameters.AddWithValue("@Id", todo.Id);
                    command.Parameters.AddWithValue("@Title", todo.Title);
                    command.Parameters.AddWithValue("@Content", todo.Content);
                    command.Parameters.AddWithValue("@State", todo.State);

                    // provedení SQL příkazu
                    await command.ExecuteNonQueryAsync();
                }
            }
            return todo;
        }
        #endregion

        //--------------------------------------------------------------------------------------------------------------------------------- DeleteById service

        #region Delete task by id service-method
        // odstranění objektu z DB dle zadaného Id
        public async Task<bool> DeleteTaskByIdAsync(Guid id) {
            using (var connection = _databaseService.CreateConnection()) {
                // příprava SQL dotazu
                using (var command = connection.CreateCommand()) {
                    command.CommandText = "DELETE FROM todo WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", id);

                    // provedení SQL příkazu a ověření, zdali byl řádek odstraněn
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------- Update task service

        #region Update task properties service-method
        // metoda pro modifikaci objektu
        public async Task<Todo> UpdateTaskAsync(Guid id, TodoUpdateDto todoUpdateDto) {
            using (var connection = _databaseService.CreateConnection()) {
                // příprava SQL dotazu
                using (var command = connection.CreateCommand()) {
                    command.CommandText = @"
                UPDATE todo
                SET Title = @Title, Content = @Content, State = @State
                WHERE Id = @Id
                RETURNING Id, Title, State, Content;
            ";
                    // přidání parametrů - hodnot vlastností
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Title", todoUpdateDto.Title);
                    command.Parameters.AddWithValue("@Content", todoUpdateDto.Content);
                    command.Parameters.AddWithValue("@State", (int)Enum.Parse<TodoState>(todoUpdateDto.State, true));

                    // provede SQL příkaz a vrátí objekt DbDataReader, který umožňuje iteraci přes vrácené řádky
                    using (var reader = await command.ExecuteReaderAsync()) {
                        if (await reader.ReadAsync()) {
                            return new Todo {
                                Id = reader.GetGuid(0),
                                Title = reader.GetString(1),
                                State = reader.GetInt32(2),
                                Content = reader.GetString(3)
                            };
                        }
                    }
                }
            }
            return null;
        }





        #endregion


    }
}
