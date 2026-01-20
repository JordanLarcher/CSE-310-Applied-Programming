package services

import javax.inject.Inject
import models.{Task, Tasks}
import play.api.db.slick.{DatabaseConfigProvider, HasDatabaseConfigProvider}
import slick.jdbc.JdbcProfile
import slick.jdbc.PostgresProfile.api._
import java.util.UUID
import java.sql.Timestamp
import scala.concurrent.{ExecutionContext, Future}

class TaskService @Inject()(protected val dbConfigProvider: DatabaseConfigProvider)(implicit ec: ExecutionContext) 
    extends HasDatabaseConfigProvider[JdbcProfile] {

  // List tasks for a specific user
    def list(userId: UUID): Future[Seq[Task]] = {
    db.run(Tasks.tasks.filter(_.userId === userId).sortBy(_.createdAt.desc).result)
    }

    // Create a new task
    def create(task: Task): Future[Int] = {
        db.run(Tasks.tasks += task)
    }

    // Get a single task (if belongs to user)
    def getById(taskId: UUID, userId: UUID): Future[Option[Task]] = {
        db.run(Tasks.tasks.filter(t => t.id === taskId && t.userId === userId).result.headOption)
    }

    // Update a task
    def update(taskId: UUID, userId: UUID, desc: String, estimated: Int): Future[Int] = {
        val query = Tasks.tasks.filter(t => t.id === taskId && t.userId === userId)
        val updateAction = query.map(t => (t.description, t.estimatedPomodoros, t.updatedAt))
        .update((desc, estimated, new Timestamp(System.currentTimeMillis())))
        
        db.run(updateAction)
    }

    // Delete a task
    def delete(taskId: UUID, userId: UUID): Future[Int] = {
        db.run(Tasks.tasks.filter(t => t.id === taskId && t.userId === userId).delete)
    }

    // Complete a Pomodoro logic (Business Logic moved here!)
    def completePomodoro(taskId: UUID, userId: UUID): Future[Option[(Int, String)]] = {
        // 1. Find the task
        val query = Tasks.tasks.filter(t => t.id === taskId && t.userId === userId)
        
        db.run(query.result.headOption).flatMap {
        case Some(task) =>
            // 2. Calculate new state
            val newCompleted = task.completedPomodoros + 1
            val newStatus = if (newCompleted >= task.estimatedPomodoros) "done" else "in_progress"
            
            // 3. Update DB
            val updateQuery = query.map(t => (t.completedPomodoros, t.status, t.updatedAt))
            .update((newCompleted, newStatus, new Timestamp(System.currentTimeMillis())))
            
            // 4. Return the new values to the controller
            db.run(updateQuery).map(_ => Some((newCompleted, newStatus)))
            
        case None => Future.successful(None)
        }
    }
}