using ActionList.DTO;
using ActionList.Filters;
using ActionList.Service;
using ActionList.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

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
            var taskDto = allTasks.Select(Task => new TodoDto {
                Id = Task.Id,
                Title = Task.Title,
                State = Task.State,
                Content = Task.Content,
                Created = Uuid7Extractor.ExtractCreationTime(Task.Id.ToString())
            }).ToList();
            return Ok(taskDto);
        }
        #endregion

        //---------------------------------------------------------------------------------------------------------------------------------- HttpGet(id)

        #region Get task by id method
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(Guid id) {
            var taskById = await _todoService.GetTaskByIdAsync(id);
            return Ok(taskById);
        }
        #endregion

        //------------------------------------------------------------------------------------------------------------------------------------- HttpPost

        #region Create task method
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TodoDto todoDto) {        
            var createdTask = await _todoService.CreatedTaskAsync(todoDto);
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
            var updatedTask = await _todoService.UpdateTaskAsync(id, todoUpdateDto);
            return Ok(updatedTask);
        }




        #endregion



    }
}
