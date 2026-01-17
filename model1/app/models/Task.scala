package models

import java.util.UUID
import java.sql.Timestamp
import slick.jdbc.PostgresProfile.api._
import slick.lifted.ProvenShape

/**
 * Task case class representing a task row in the database
 * @param id Unique task identifier
 * @param userId Foreign key reference to the owning user
 * @param description Task description
 * @param estimatedPomodoros Number of pomodoros estimated to complete the task
 * @param completedPomodoros Number of pomodoros already completed
 * @param status Current task status (todo, in_progress, done)
 * @param createdAt Timestamp when the task was created
 * @param updatedAt Timestamp when the task was last updated
 */
case class Task(id: UUID, userId: UUID, description: String, estimatedPomodoros: Int, completedPomodoros: Int, status: String, createdAt: Timestamp, updatedAt: Timestamp)

/**
 * Slick table definition for the tasks table
 * Maps database columns to Scala types and provides query interface
 */
class Tasks(tag: Tag) extends Table[Task](tag, "tasks") {
  def id = column[UUID]("id", O.PrimaryKey)
  def userId = column[UUID]("user_id")
  def description = column[String]("description")
  def estimatedPomodoros = column[Int]("estimated_pomodoros")
  def completedPomodoros = column[Int]("completed_pomodoros")
  def status = column[String]("status")
  def createdAt = column[Timestamp]("created_at")
  def updatedAt = column[Timestamp]("updated_at")

  /**
   * Default projection mapping between table columns and case class
   * Enables automatic conversion between database rows and Task objects
   */
  def * : ProvenShape[Task] = (id, userId, description, estimatedPomodoros, completedPomodoros, status, createdAt, updatedAt) <> (Task.tupled, Task.unapply)
}

/**
 * Companion object providing query interface for Task operations
 * Contains the TableQuery used throughout the application
 */
object Tasks {
  /**
   * Query object used for all database operations on tasks table
   * Provides type-safe query building and execution
   */
  lazy val tasks = TableQuery[Tasks]
}