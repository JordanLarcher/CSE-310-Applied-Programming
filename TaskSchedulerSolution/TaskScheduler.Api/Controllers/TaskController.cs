using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskScheduler.Application.DTOs.Tasks;
using TaskScheduler.Application.Services.Interfaces;

namespace TaskScheduler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks([FromQuery] TaskFilter? filter)
        {
            var userId = GetCurrentUserId();
            var tasks = await _taskService.GetUserTasksAsync(userId, filter);
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var task = await _taskService.GetTaskByIdAsync(userId, id);
                return Ok(task);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetOverdueTasks()
        {
            var userId = GetCurrentUserId();
            var tasks = await _taskService.GetOverdueTasksAsync(userId);
            return Ok(tasks);
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<TaskStatisticsDto>> GetStatistics()
        {
            var userId = GetCurrentUserId();
            var stats = await _taskService.GetTaskStatisticsAsync(userId);
            return Ok(stats);
        }

        [HttpPost]
        public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var task = await _taskService.CreateTaskAsync(dto, userId);
                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var task = await _taskService.UpdateTaskAsync(id, dto, userId);
                return Ok(task);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/complete")]
        public async Task<ActionResult<TaskDto>> MarkAsCompleted(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var task = await _taskService.MarkTaskAsCompletedAsync(userId, id);
                return Ok(task);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTask(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskService.DeleteTaskAsync(userId, id);

                if (result)
                {
                    return Ok(new { message = "Task deleted successfully" });
                }

                return NotFound(new { message = "Task not found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
