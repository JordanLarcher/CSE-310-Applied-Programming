package controllers

import javax.inject.Inject
import models.{Task, Tasks}
import play.api.mvc._
import play.api.data.Form
import play.api.data.Forms._
import play.api.i18n.I18nSupport
import scala.concurrent.{ExecutionContext, Future}
import slick.jdbc.PostgresProfile.api._
import java.util.UUID
import java.sql.Timestamp
import play.api.db.slick.{DatabaseConfigProvider, HasDatabaseConfigProvider}
import slick.jdbc.JdbcProfile
import services.TaskService

/**
 * Form data case class for task creation and editing
 * @param description Task description
 * @param estimated Estimated number of pomodoros (1-10)
 */
case class TaskData(description: String, estimated: Int)

class TaskController @Inject()(
    protected val dbConfigProvider: DatabaseConfigProvider,
    cc: ControllerComponents,
    taskService: TaskService,
    messagesApi: play.api.i18n.MessagesApi
)(implicit ec: ExecutionContext) extends AbstractController(cc) with HasDatabaseConfigProvider[JdbcProfile] with Secured with I18nSupport {

  override val controllerComponents: ControllerComponents = cc

  // --- FORMS ---
  
  /**
   * Form definition for task creation and editing with validation
   */
  val taskForm = Form(
    mapping(
      "description" -> nonEmptyText.verifying("Description must be at least 3 characters", _.length >= 3),
      "estimated" -> number(min = 1, max = 10)
    )(TaskData.apply)(TaskData.unapply)
  )

  // 14 - GET /tasks (List tasks)
  /**
   * Display list of tasks for the authenticated user
   * @param userId authenticated user ID
   * @param request implicit HTTP request
   * @return HTML page with task list
   */
  def listTasks = withAuth { userId => implicit request =>
    //Delegate to the Task Service
    taskService.list(userId).map { tasks => 
        Ok(views.html.taskList(tasks))
    }.recover {
      case ex: Exception =>
        InternalServerError("Error loading tasks: " + ex.getMessage)
    }
  }

  // 15 - GET /tasks/new (Show create task form)
  /**
   * Display the task creation form
   * @param userId authenticated user ID
   * @param request implicit HTTP request
   * @return HTML page with task creation form
   */
  def showCreateTask = withAuth { userId => implicit request =>
    Future.successful(Ok(views.html.createTask(taskForm)))
  }

  // 16 - POST /tasks/new (Process task creation)
  /**
   * Process task creation form submission
   * @param userId authenticated user ID
   * @param request implicit HTTP request containing form data
   * @return Redirect to task list on success or form with errors on failure
   */
  def createTask = withAuth { userId => implicit request =>
    taskForm.bindFromRequest().fold(
      formWithErrors => Future.successful(BadRequest(views.html.createTask(formWithErrors))),
      data => {
        val currentTime = new Timestamp(System.currentTimeMillis())
        val newTask = Task(
          UUID.randomUUID(), 
          userId, 
          data.description.trim, 
          data.estimated, 
          0, 
          "todo",
          currentTime, 
          currentTime
        )
        // Delegate to the TaskService
        taskService.create(newTask).map(_ => Redirect(routes.TaskController.listTasks))
      }
    )
  }

  // 17 - GET /tasks/:id/edit (Show edit task form)
  /**
   * Display the task editing form for a specific task
   * @param id task ID to edit
   * @param userId authenticated user ID
   * @param request implicit HTTP request
   * @return HTML page with task editing form or 404 if task not found
   */
  def showEditTask(id: UUID) = withAuth { userId => implicit request =>
    // Delegate to the TaskService
    taskService.getById(id, userId).map {
      case Some(task) => 
        val filledForm = taskForm.fill(TaskData(task.description, task.estimatedPomodoros))
        Ok(views.html.editTask(id, filledForm))
      case None => NotFound("Task not found") 
    }
  }

  // 18 - POST /tasks/:id/edit (Process task editing)
  /**
   * Process task editing form submission
   * @param id task ID to edit
   * @param userId authenticated user ID
   * @param request implicit HTTP request containing form data
   * @return Redirect to task list on success or form with errors on failure
   */
  def editTask(id: UUID) = withAuth { userId => implicit request =>
    taskForm.bindFromRequest().fold(
      formWithErrors => Future.successful(BadRequest(views.html.editTask(id, formWithErrors))),
      data => {
        // Delegated to the taskSer
        taskService.update(id, userId, data.description.trim, data.estimated).map { rowsUpdated =>
          if (rowsUpdated > 0) Redirect(routes.TaskController.listTasks)
          else NotFound("Task not found")
        }.recover {
          case ex: Exception =>
            InternalServerError(views.html.editTask(id, taskForm.withGlobalError("Failed to update task")))
        }
      }
    )
  }

  // 19 - POST /tasks/:id/delete (Delete task)
  /**
   * Delete a specific task
   * @param id task ID to delete
   * @param userId authenticated user ID
   * @param request implicit HTTP request
   * @return Redirect to task list
   */
  def deleteTask(id: UUID) = withAuth { userId => implicit request =>
    // Delegated to the taskService
    taskService.delete(id, userId).map { rowsDeleted =>
      if (rowsDeleted > 0 ) Redirect(routes.TaskController.listTasks)
      else NotFound("Task not found")
    }
  }

  // 20 - POST /tasks/:id/complete (Complete Pomodoro - AJAX or Form)
  /**
   * Complete a pomodoro session for a specific task
   * @param id task ID to update
   * @param userId authenticated user ID
   * @param request implicit HTTP request
   * @return JSON response for AJAX requests or redirect for form submissions
   */
  def completeTask(id: UUID) = withAuth { userId => implicit request =>
    // Delegated the complex logic to the taskService
    taskService.completePomodoro(id, userId).map {
      case Some(( newCompleted, newStatus)) =>
        if (request.accepts("application/json")) {
          Ok(s"""{"success": true, "completed": $newCompleted, "status": "$newStatus"}""").as("application/json")
        } else {
          Redirect(routes.TaskController.listTasks)
        }
      case None => 
        if (request.accepts("application/json")) {
          NotFound("""{"error": "Task not found"}""").as("application/json")
        } else {
          NotFound("Task not found")
        }
    }.recover {
      case ex: Exception =>
        if (request.accepts("application/json")) {
          InternalServerError("""{"error": "Failed to complete task"}""").as("application/json")
        } else {
          InternalServerError("Failed to complete task")
        }
    }
  }
}