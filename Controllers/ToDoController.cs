using BlogApi.Data;
using BlogApi.DTOs.ToDo;
using BlogApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToDoController : ControllerBase
    {
        private readonly BloggingContext _context;

        public ToDoController(BloggingContext context)
        {
            _context = context;
        }

        /// <summary>
        /// This is the api to list all the tasks but the data is sorted like this:
        /// First data entered by logged in user.
        /// Then First, tasks where IsCompleted == false, ordered by older TaskDateTime first.
        /// Then, tasks where IsCompleted == true, ordered by newer TaskDateTime first.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("ToDoLists")]
        public async Task<IActionResult> ToDoList()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);

            var tasks = await _context.ToDos
                .Where(t => t.EntryByUserId == userId)
                .OrderBy(t => t.IsCompleted)  // False tasks first
                .ThenBy(t => t.IsCompleted == false ? t.TaskDateTime : DateTime.MaxValue) // Order false tasks by older TaskDateTime first
                .ThenByDescending(t => t.IsCompleted == true ? t.TaskDateTime : DateTime.MinValue) // Order true tasks by newer TaskDateTime first
                .ToListAsync();

            return Ok(tasks);
        }

        /// <summary>
        /// Api to add new task.
        /// </summary>
        /// <param name="ToDoSaveEditDto"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("AddToDoTask")]
        public async Task<IActionResult> AddToDoTask([FromBody] ToDoSaveEditDto saveDto)
        {
            var tasks = new ToDo
            {
                Task = saveDto.Task,
                TaskDateTime = saveDto.TaskDateTime,
                TaskAssignedForDateTime = saveDto.TaskDateTime,
                IsCompleted = false,
                EntryByUserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
            };

            _context.ToDos.Add(tasks);
            await _context.SaveChangesAsync();

            return Ok(tasks);
        }

        [Authorize]
        [HttpPost("EditToDoTask")]
        public async Task<IActionResult> UpdateToDoTaskDetails(int taskId, [FromBody] ToDoSaveEditDto editDto)
        {
            var task = await _context.ToDos.FindAsync(taskId);
            if (task == null)
                return NotFound();

            // check if current user is the one to enter todo
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);

            if (task.EntryByUserId != userId)
                return Forbid();

            task.Task = editDto.Task;
            task.TaskDateTime = editDto.TaskDateTime;

            await _context.SaveChangesAsync();

            return Ok(task);
        }

        /// <summary>
        /// This api is to complete the task.
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("CompleteToDoTask")]
        public async Task<IActionResult> CompleteToDoTask([FromBody] int taskId)
        {
            var task = await _context.ToDos.FindAsync(taskId);
            if (task == null)
                return NotFound();

            // check if current user is the one to enter todo
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);

            if (task.EntryByUserId != userId)
                return Forbid();

            task.IsCompleted = true;

            await _context.SaveChangesAsync();

            return Ok(task);
        }

        /// <summary>
        /// This is the api to delete the tasks.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("DeleteToDoTask")]
        public async Task<IActionResult> DeleteToDoTask([FromBody] int taskId)
        {
            var task = await _context.ToDos.FindAsync(taskId);
            if (task == null)
                return NotFound();

            // check if current user is the author
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);

            if (task.EntryByUserId != userId)
                return Forbid();

            _context.ToDos.Remove(task);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}