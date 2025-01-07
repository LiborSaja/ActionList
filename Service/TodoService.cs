
using ActionList.DTO;
using ActionList.Model;
using ActionList.Utility;
using Microsoft.AspNetCore.Mvc;

namespace ActionList.Service {
    public class TodoService {
        private readonly DatabaseService _databaseService;

        public TodoService(DatabaseService databaseService) {
            this._databaseService = databaseService;
        }

        //----------------------------------------------------------------------------------------------------------------------------- Get all tasks service

        #region Get all tasks method
        public async Task<List<Todo>> GetAllTasksAsync() {
            using (var connection = _databaseService.CreateConnection()) {
                using (var command = connection.CreateCommand()) {
                    command.CommandText = "SELECT Id, Title, State, Content FROM todo";

                    using (var reader = await command.ExecuteReaderAsync()) {
                        var todos = new List<Todo>();
                        while (await reader.ReadAsync()) {
                            todos.Add(new Todo {
                                Id = reader.GetGuid(0),
                                Title = reader.GetString(1),
                                State = reader.GetInt32(2),
                                Content = reader.IsDBNull(3) ? null : reader.GetString(3)
                            });
                        }

                        // Pokud nejsou žádné záznamy, vrátíme null (nebo prázdný seznam, pokud preferuješ)
                        return todos.Any() ? todos : null;
                    }
                }
            }
        }

        #endregion

        //---------------------------------------------------------------------------------------------------------------------------- Get task by id service

        #region
        public async Task<Todo> GetTaskByIdAsync(Guid id) {
            using (var connection = _databaseService.CreateConnection()) {
                using (var command = connection.CreateCommand()) {
                    command.CommandText = "SELECT Id, Title, State, Content FROM public.todo WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", id);

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

        #region Post task method
        public async Task<Todo> CreatedTaskAsync(TodoDto todoDto, int state) {
            var todo = new Todo {
                Id = Uuid7Generator.GenerateUuid7(),
                Title = todoDto.Title,
                State = state,
                Content = todoDto.Content
            };

            using (var connection = _databaseService.CreateConnection()) {
                using (var command = connection.CreateCommand()) {
                    command.CommandText = @"
                INSERT INTO todo (Id, Title, State, Content)
                VALUES (@Id, @Title, @State, @Content);
            ";

                    command.Parameters.AddWithValue("@Id", todo.Id);
                    command.Parameters.AddWithValue("@Title", todo.Title);
                    command.Parameters.AddWithValue("@Content", todo.Content ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@State", todo.State);

                    await command.ExecuteNonQueryAsync();
                }
            }

            return todo;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------------------------- DeleteById service

        #region Delete task by id method
        public async Task<bool> DeleteTaskByIdAsync(Guid id) {
            using (var connection = _databaseService.CreateConnection()) {
                using (var command = connection.CreateCommand()) {
                    command.CommandText = "DELETE FROM public.todo WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", id);

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        #endregion

        //--------------------------------------------------------------------------------------------------------------------------------- Update task service

        #region Update task properties method
        public async Task<Todo> UpdateTaskAsync(Guid id, TodoUpdateDto todoUpdateDto) {
            var validStates = new[] { "open", "in progress", "finished" };
            if (!validStates.Contains(todoUpdateDto.State)) {
                throw new ArgumentException($"Invalid state: {todoUpdateDto.State}");
            }

            using (var connection = _databaseService.CreateConnection()) {
                using (var command = connection.CreateCommand()) {
                    command.CommandText = @"
                UPDATE public.todo
                SET Title = @Title, Content = @Content, State = @State
                WHERE Id = @Id
                RETURNING Id, Title, State, Content;
            ";

                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Title", todoUpdateDto.Title ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Content", todoUpdateDto.Content ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@State", todoUpdateDto.State switch {
                        "open" => 0,
                        "in progress" => 1,
                        "finished" => 2,
                        _ => throw new ArgumentException($"Invalid state: {todoUpdateDto.State}")
                    });

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
