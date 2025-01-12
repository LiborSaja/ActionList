using ActionList.DTO;
using ActionList.Model;
using ActionList.Service;
using ActionList.Utility;
using Microsoft.AspNetCore.Mvc;

namespace ActionList.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase {

        private readonly TodoService _todoService;

        public TodoController(TodoService todoService) {
            this._todoService = todoService;
        }

        //--------------------------------------------------------------------------------------------------------------------------------- HttpGet - all tasks

        #region Get all tasks method
        [HttpGet]
        public async Task<IActionResult> GetTasks([FromQuery] string state = "all") {
            // povolené hodnoty pro filtr
            var validStates = new[] { "all", "open", "inprogress", "finished" };
            if (!validStates.Contains(state, StringComparer.OrdinalIgnoreCase)) {
                return BadRequest(new ErrorResponse {
                    Error = new ErrorDetail {
                        Code = "400003",
                        Message = $"Invalid state value: {state}. Valid values are: {string.Join(", ", validStates)}."
                    }
                });
            }
            var filteredTasks = await _todoService.GetTasksByStateAsync(state.ToLower());

            // pokud nejsou žádné objekty, vrátí strukturovanou zprávu
            if (filteredTasks == null || !filteredTasks.Any()) {
                return NotFound(new ErrorResponse {
                    Error = new ErrorDetail {
                        Code = "404002",
                        Message = "No tasks found in the database."
                    }
                });
            }

            // mapování základního modelu na model DTO - pro vracení na frontend ve formátu vhodném pro API
            var taskDto = filteredTasks.Select(task => new TodoDto {
                Id = task.Id,
                Title = task.Title,
                State = ((TodoState)task.State).ToString(),
                Content = task.Content,
                Created = Uuid7Extractor.ExtractCreationTime(task.Id.ToString())
            }).ToList();
            return Ok(taskDto);
        }


        #endregion

        //----------------------------------------------------------------------------------------------------------------------------------------- HttpGet(id)

        #region Get task by id method
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(string id) {
            // kontrola platnosti GUID a existence daného ID v databázi
            if (!Guid.TryParse(id, out var validId) || (await _todoService.GetTaskByIdAsync(validId)) is not Todo taskById) {
                return NotFound(new ErrorResponse {
                    Error = new ErrorDetail {
                        Code = "404003",
                        Message = $"Task with ID '{id}' not found."
                    }
                });
            }

            // mapování základního modelu na DTO
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

        //-------------------------------------------------------------------------------------------------------------------------------------------- HttpPost

        #region Create task method
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TodoDto todoDto) {
            // validace title a content a jejich uložení do kolekce pro přehledné zobrazení
            var validationErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(todoDto.Title)) {
                validationErrors.Add("The 'Title' field is required.");
            }
            if (string.IsNullOrWhiteSpace(todoDto.Content)) {
                validationErrors.Add("The 'Content' field is required.");
            }
            // pokud některý z inputů nebude vyplněn, vrátí chybu/y 
            if (validationErrors.Any()) {
                return BadRequest(new ErrorResponse {
                    Error = new ErrorDetail {
                        Code = "400001",
                        Message = "Validation failed.",
                        Details = validationErrors
                    }
                });
            }

            // ověření, zda hodnota odpovídá pouze definovaným hodnotám - defacto zbytečné, když na frontendu je <select>
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
            // převedení validního stringu na číselný stav
            var stateEnum = Enum.Parse<TodoState>(todoDto.State, true);

            // voláni service
            var createdTask = await _todoService.CreatedTaskAsync(todoDto, (int)stateEnum);
            return Ok(createdTask);
        }



        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------- HttpDelete(id)

        #region Delete task by id method
        [HttpDelete("{id}")]
        // odstranění objektu z DB dle Id
        public async Task<IActionResult> DeleteTaskById(Guid id) {
            var deleteById = await _todoService.DeleteTaskByIdAsync(id);
            return Ok(id);
        }
        #endregion

        //--------------------------------------------------------------------------------------------------------------------------------------------- HttpPut

        #region Update task properties method
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] TodoUpdateDto todoUpdateDto) {
            // validace title a content a uložení do kolekce
            var validationErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(todoUpdateDto.Title)) {
                validationErrors.Add("The 'Title' field is required.");
            }
            if (string.IsNullOrWhiteSpace(todoUpdateDto.Content)) {
                validationErrors.Add("The 'Content' field is required.");
            }

            // pokud některá z vlastností nevyplněna, vrátí strukturovanou zprávu
            if (validationErrors.Any()) {
                return BadRequest(new ErrorResponse {
                    Error = new ErrorDetail {
                        Code = "400001",
                        Message = "Validation failed.",
                        Details = validationErrors
                    }
                });
            }

            // volání service
            var updatedTask = await _todoService.UpdateTaskAsync(id, todoUpdateDto);
            return Ok(updatedTask);
        }
        #endregion



    }
}
