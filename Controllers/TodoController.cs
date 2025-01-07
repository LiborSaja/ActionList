using ActionList.DTO;
using ActionList.Filters;
using ActionList.Model;
using ActionList.Service;
using ActionList.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace ActionList.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase {

        private readonly TodoService _todoService;

        public TodoController(TodoService todoService) {
            this._todoService = todoService;
        }

        //------------------------------------------------------------------------------------------------------------------------------------- HttpGet

        #region Get all tasks method
        [HttpGet]
        public async Task<IActionResult> GetAllTasks() {
            var allTasks = await _todoService.GetAllTasksAsync();

            // Pokud nejsou žádné záznamy, vrátíme zprávu
            if (allTasks == null) {
                return NotFound(new ErrorResponse {
                    Error = new ErrorDetail {
                        Code = "404002",
                        Message = "No tasks found in the database."
                    }
                });
            }

            var taskDto = allTasks.Select(task => new TodoDto {
                Id = task.Id,
                Title = task.Title,
                State = ((TodoState)task.State).ToString(),
                Content = task.Content,
                Created = Uuid7Extractor.ExtractCreationTime(task.Id.ToString())
            }).ToList();

            return Ok(taskDto);
        }

        #endregion

        //---------------------------------------------------------------------------------------------------------------------------------- HttpGet(id)

        #region Get task by id method
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(string id) {
            // Kontrola platnosti GUID a existence v databázi
            if (!Guid.TryParse(id, out var validId) || (await _todoService.GetTaskByIdAsync(validId)) is not Todo taskById) {
                return NotFound(new ErrorResponse {
                    Error = new ErrorDetail {
                        Code = "404003",
                        Message = $"Task with ID '{id}' not found."
                    }
                });
            }

            // Převod na DTO
            var taskDto = new TodoDto {
                Id = taskById.Id,
                Title = taskById.Title,
                State = ((TodoState)taskById.State).ToString(),
                Content = taskById.Content,
                Created = Uuid7Extractor.ExtractCreationTime(taskById.Id.ToString())
            };

            return Ok(taskDto);
        }



        #endregion

        //------------------------------------------------------------------------------------------------------------------------------------- HttpPost

        #region Create task method
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TodoDto todoDto) {
            // Seznam chyb
            var validationErrors = new List<string>();

            // Validace Title
            if (string.IsNullOrWhiteSpace(todoDto.Title)) {
                validationErrors.Add("The 'Title' field is required.");
            }

            // Validace Content
            if (string.IsNullOrWhiteSpace(todoDto.Content)) {
                validationErrors.Add("The 'Content' field is required.");
            }

            // Pokud jsou chyby, vrať je
            if (validationErrors.Any()) {
                return BadRequest(new ErrorResponse {
                    Error = new ErrorDetail {
                        Code = "400001",
                        Message = "Validation failed.",
                        Details = validationErrors
                    }
                });
            }

            // Ověření, zda hodnota odpovídá pouze povoleným hodnotám
            var allowedStates = Enum.GetNames(typeof(TodoState));
            if (!allowedStates.Contains(todoDto.State, StringComparer.OrdinalIgnoreCase)) {
                return BadRequest(new ErrorResponse {
                    Error = new ErrorDetail {
                        Code = "400002",
                        Message = $"Invalid state value: {todoDto.State}",
                        Details = allowedStates.ToList()
                    }
                });
            }

            // Převedení validního stringu na číselný stav
            var stateEnum = Enum.Parse<TodoState>(todoDto.State, true);

            // Zavolání servisní metody s validovaným stavem
            var createdTask = await _todoService.CreatedTaskAsync(todoDto, (int)stateEnum);
            return Ok(createdTask);
        }



        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------- HttpDelete(id)

        #region Delete task by id method
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskById(Guid id) {
            var deleteById = await _todoService.DeleteTaskByIdAsync(id);
            return Ok(id);
        }
        #endregion

        //--------------------------------------------------------------------------------------------------------------------------------------- HttpPut

        #region Update task properties method
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] TodoUpdateDto todoUpdateDto) {
            // Seznam chyb
            var validationErrors = new List<string>();

            // Validace Title
            if (string.IsNullOrWhiteSpace(todoUpdateDto.Title)) {
                validationErrors.Add("The 'Title' field is required.");
            }

            // Validace Content
            if (string.IsNullOrWhiteSpace(todoUpdateDto.Content)) {
                validationErrors.Add("The 'Content' field is required.");
            }

            // Pokud jsou chyby, vrať je
            if (validationErrors.Any()) {
                return BadRequest(new ErrorResponse {
                    Error = new ErrorDetail {
                        Code = "400001",
                        Message = "Validation failed.",
                        Details = validationErrors
                    }
                });
            }

            // Zavolání servisní metody
            var updatedTask = await _todoService.UpdateTaskAsync(id, todoUpdateDto);
            return Ok(updatedTask);
        }



        #endregion



    }
}
