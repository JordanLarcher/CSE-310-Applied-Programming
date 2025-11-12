using Microsoft.AspNetCore.Mvc;
using TaskScheduler.Shared; // <-- Import our shared models

namespace TaskScheduler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Sets the URL path to /api/Tasks
    public class TasksController : ControllerBase
    {
        // For this module, we use a simple in-memory list.
        // 'static' ensures the list persists between requests.
        // This list stores our 'variables' (the tasks).
        private static readonly List<TaskItem> _tasks = new List<TaskItem>();
        private static int _nextId = 1;

        // REQUIREMENT: Functions (Methods)
        // This is a function that handles HTTP GET requests to /api/Tasks
        [HttpGet]
        public ActionResult<IEnumerable<TaskItem>> GetTasks()
        {
            // Returns the full list of tasks
            return Ok(_tasks);
        }

        // Handles GET requests to /api/Tasks/{id}
        [HttpGet("{id}")]
        public ActionResult<TaskItem> GetTask(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            
            // REQUIREMENT: Conditionals
            if (task == null)
            {
                return NotFound(); // Returns 404 if task doesn't exist
            }
            return Ok(task); // Returns 200 with the task
        }

        // REQUIREMENT: Functions (Methods)
        // This function handles HTTP POST requests to /api/Tasks
        [HttpPost]
        public ActionResult<TaskItem> CreateTask(TaskItem task)
        {
            // REQUIREMENT: Expressions
            // This is an assignment expression.
            task.Id = _nextId++; // Assign new ID and increment
            
            _tasks.Add(task);

            // Returns a 201 Created status with the new task
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        // Handles HTTP PUT requests to /api/Tasks/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateTask(int id, TaskItem updatedTask)
        {
            // REQUIREMENT: Conditionals
            if (id != updatedTask.Id)
            {
                return BadRequest(); // 400 Bad Request
            }

            var existingTask = _tasks.FirstOrDefault(t => t.Id == id);
            if (existingTask == null)
            {
                return NotFound(); // 404 Not Found
            }

            // Update the existing task's properties
            existingTask.Title = updatedTask.Title;
            existingTask.Description = updatedTask.Description;
            existingTask.DueDate = updatedTask.DueDate;
            existingTask.IsComplete = updatedTask.IsComplete;
            existingTask.ReminderTime = updatedTask.ReminderTime;
            existingTask.IsReminderSent = updatedTask.IsReminderSent; // Allow resetting reminders
            existingTask.Config = updatedTask.Config;

            return NoContent(); // 204 No Content (success)
        }

        // Handles HTTP DELETE requests to /api/Tasks/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            
            // REQUIREMENT: Conditionals
            if (task == null)
            {
                return NotFound();
            }

            _tasks.Remove(task);
            return NoContent(); // 204 No Content (success)
        }
    }
}
