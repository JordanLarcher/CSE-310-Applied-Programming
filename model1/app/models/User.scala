package models

import java.util.UUID
import slick.jdbc.PostgresProfile.api._
import slick.lifted.ProvenShape
import java.sql.Timestamp

// La Case Class representa una fila de datos
case class User(id: UUID, email: String, passwordHash: Option[String], displayName: Option[String], googleId: Option[String], createdAt: Timestamp)

// La definición de la Tabla para Slick
class Users(tag: Tag) extends Table[User](tag, "users") {
  def id = column[UUID]("id", O.PrimaryKey)
  def email = column[String]("email")
  def passwordHash = column[Option[String]]("password_hash")
  def displayName = column[Option[String]]("display_name")
  def googleId = column[Option[String]]("google_id")
  def createdAt = column[Timestamp]("created_at")

  // Mapeo entre la tabla y la clase
  def * : ProvenShape[User] = (id, email, passwordHash, displayName, googleId, createdAt) <> (User.tupled, User.unapply)
}

object Users {
  // El objeto de consulta que usarás en los controladores
  lazy val users = TableQuery[Users]
}