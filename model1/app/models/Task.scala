package models

import java.util.UUID
import java.sql.Timestamp
import slick.jdbc.PostgresProfile.api._
import slick.lifted.ProvenShape

case class Task(id: UUID, userId: UUID, description: String, estimatedPomodoros: Int, completedPomodoros: Int, status: String, createdAt: Timestamp, updatedAt: Timestamp)

class Tasks(tag: Tag) extends Table[Task](tag, "tasks") {
  def id = column[UUID]("id", O.PrimaryKey)
  def userId = column[UUID]("user_id")
  def description = column[String]("description")
  def estimatedPomodoros = column[Int]("estimated_pomodoros")
  def completedPomodoros = column[Int]("completed_pomodoros")
  def status = column[String]("status")
  def createdAt = column[Timestamp]("created_at")
  def updatedAt = column[Timestamp]("updated_at")

  def * : ProvenShape[Task] = (id, userId, description, estimatedPomodoros, completedPomodoros, status, createdAt, updatedAt) <> (Task.tupled, Task.unapply)
}

object Tasks {
  lazy val tasks = TableQuery[Tasks]
}