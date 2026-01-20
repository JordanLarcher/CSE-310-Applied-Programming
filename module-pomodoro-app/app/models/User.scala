package models

import java.util.UUID
import slick.jdbc.PostgresProfile.api._
import slick.lifted.ProvenShape
import java.sql.Timestamp

/**
 * User case class representing a user row in the database
 * @param id Unique user identifier
 * @param email User's email address (unique)
 * @param passwordHash Optional bcrypt hash for password authentication
 * @param displayName Optional display name for the user
 * @param googleId Optional Google OAuth ID for social login
 * @param createdAt Timestamp when the user was created
 */
case class User(id: UUID, email: String, passwordHash: Option[String], displayName: Option[String], googleId: Option[String], createdAt: Timestamp)

/**
 * Slick table definition for the users table
 * Maps database columns to Scala types and provides query interface
 */
class Users(tag: Tag) extends Table[User](tag, "users") {
  def id = column[UUID]("id", O.PrimaryKey)
  def email = column[String]("email")
  def passwordHash = column[Option[String]]("password_hash")
  def displayName = column[Option[String]]("display_name")
  def googleId = column[Option[String]]("google_id")
  def createdAt = column[Timestamp]("created_at")

  /**
   * Default projection mapping between table columns and case class
   * Enables automatic conversion between database rows and User objects
   */
  def * : ProvenShape[User] = (id, email, passwordHash, displayName, googleId, createdAt) <> (User.tupled, User.unapply)
}

/**
 * Companion object providing query interface for User operations
 * Contains the TableQuery used throughout the application
 */
object Users {
  /**
   * Query object used for all database operations on users table
   * Provides type-safe query building and execution
   */
  lazy val users = TableQuery[Users]
}