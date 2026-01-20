package controllers

import javax.inject.Inject
import play.api.mvc._
import scala.concurrent.{ExecutionContext, Future}
import slick.jdbc.PostgresProfile.api._
import models.{Tasks, Users} // Import our models
import play.api.db.slick.{DatabaseConfigProvider, HasDatabaseConfigProvider}
import slick.jdbc.JdbcProfile
import java.util.UUID

class ReportController @Inject()(
                                  protected val dbConfigProvider: DatabaseConfigProvider,
                                  cc: ControllerComponents
                                )(implicit ec: ExecutionContext) extends AbstractController(cc) with HasDatabaseConfigProvider[JdbcProfile] with Secured {

  override val controllerComponents: ControllerComponents = cc
  // GET /report
  def index = withAuth { userId => implicit request =>
    // 1. Find all user tasks
    val tasksQuery = Tasks.tasks.filter(_.userId === userId)

    // 2. Execute the query
    db.run(tasksQuery.result).map { tasks =>

      // 3. Calculate statistics in memory (Scala)
      val totalTasks = tasks.size
      val completedTasks = tasks.count(_.status == "done")

      // Sum all completed pomodoros from all tasks
      val totalPomodoros = tasks.foldLeft(0)(_ + _.completedPomodoros)

      // Calculate total time: (Pomos * 25 minutes) / 60 = Hours
      val totalMinutes = totalPomodoros * 25
      val hoursFocused = totalMinutes / 60.0

      // 4. Send this data to the view
      Ok(views.html.report(totalTasks, completedTasks, totalPomodoros, hoursFocused))
    }
  }
}