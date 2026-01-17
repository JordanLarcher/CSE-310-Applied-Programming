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

/**
 * Form data case class for task creation and editing
 * @param description Task description
 * @param estimated Estimated number of pomodoros (1-10)
 */
case class TaskData(description: String, estimated: Int)

class TaskController @Inject()(
    protected val dbConfigProvider: DatabaseConfigProvider,
    cc: ControllerComponents,
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
    val query = Tasks.tasks.filter(_.userId === userId).sortBy(_.createdAt.desc)
    db.run(query.result).map { tasks =>
      Ok(views.html.taskList(tasks)) // Render the task list view
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
        db.run(Tasks.tasks += newTask).map(_ => Redirect(routes.TaskController.listTasks))
          .recover {
            case ex: Exception =>
              InternalServerError(views.html.createTask(taskForm.withGlobalError("Failed to create task")))
          }
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
    // Find the task and verify it belongs to the user
    val query = Tasks.tasks.filter(t => t.id === id && t.userId === userId)
    db.run(query.result.headOption).map {
      case Some(task) =>
        // Fill the form with existing data
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
        val query = Tasks.tasks.filter(t => t.id === id && t.userId === userId)
        val updateAction = query.map(t => (t.description, t.estimatedPomodoros, t.updatedAt))
          .update((data.description.trim, data.estimated, new Timestamp(System.currentTimeMillis())))
        
        db.run(updateAction).map { rowsUpdated =>
          if (rowsUpdated > 0) {
            Redirect(routes.TaskController.listTasks)
          } else {
            NotFound("Task not found")
          }
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
    val query = Tasks.tasks.filter(t => t.id === id && t.userId === userId)
    db.run(query.delete).map { rowsDeleted =>
      if (rowsDeleted > 0) {
        Redirect(routes.TaskController.listTasks)
      } else {
        NotFound("Task not found")
      }
    }.recover {
      case ex: Exception =>
        InternalServerError("Failed to delete task")
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
    val query = Tasks.tasks.filter(t => t.id === id && t.userId === userId)
    db.run(query.result.headOption).flatMap {
      case Some(task) =>
        val newCompleted = task.completedPomodoros + 1
        val newStatus = if (newCompleted >= task.estimatedPomodoros) "done" else "in_progress"
        val updateQuery = query.map(t => (t.completedPomodoros, t.status, t.updatedAt))
          .update((newCompleted, newStatus, new Timestamp(System.currentTimeMillis())))
        
        // If request accepts JSON (AJAX) respond with JSON, otherwise redirect
        db.run(updateQuery).map { _ => 
            if (request.accepts("application/json")) {
              Ok(s"""{"success": true, "completed": $newCompleted, "status": "$newStatus"}""")
                .as("application/json")
            } else {
              Redirect(routes.TaskController.listTasks)
            }
        }
      case None => Future.successful(NotFound("Task not found"))
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