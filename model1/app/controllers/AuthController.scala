package controllers

import javax.inject.Inject
import models.{User, Users}
import play.api.mvc._

import scala.concurrent.{ExecutionContext, Future}
import slick.jdbc.PostgresProfile.api._
import com.github.t3hnar.bcrypt._
import forms.AuthForms._

import java.util.UUID
import play.api.db.slick.{DatabaseConfigProvider, HasDatabaseConfigProvider}
import play.api.http.Writeable.wByteArray
import slick.jdbc.JdbcProfile

class AuthController @Inject()(
                                protected val dbConfigProvider: DatabaseConfigProvider,
                                cc: ControllerComponents
                              )(implicit ec: ExecutionContext) extends AbstractController(cc) with HasDatabaseConfigProvider[JdbcProfile] {

  // Muestra el formulario de registro
  def showRegister = Action { implicit request =>
    Ok(views.html.register(registerForm))
  }

  // This process the registration
  def register = Action.async(parse.form(registerForm)) { implicit request =>
    val data = request.body
    // Password Hashing
    val hashed = data.password.bcryptSafeBounded(12).toOption.get

    // Create User Object
    val user = User(
      UUID.randomUUID(),
      data.email,
      Some(hashed),
      Some(data.email.split("@")(0)), // DisplayName by default
      None,
      new java.sql.Timestamp(System.currentTimeMillis())
    )

    // Insert the new user in DB and redirect to log in
    db.run(Users.users += user).map { _ =>
      Redirect(routes.HomeController.index()).withSession("userId" -> user.id.toString)
    }.recover {
      case ex: Exception =>
        BadRequest(views.html.register(registerForm.withGlobalError("Error: El email ya existe.")))
    }
  }

  // Display the Login form
  def showLogin = Action { implicit request =>
    Ok(views.html.login(loginForm))
  }

  // Procesa el login
  def login = Action.async(parse.form(loginForm)) { implicit request =>
    val data = request.body

    // Find User by Email
    db.run(Users.users.filter(_.email === data.email).result.headOption).map {
      // Si existe usuario Y el password coincide con el hash
      case Some(user) if data.password.isBcryptedSafeBounded(user.passwordHash.getOrElse("")).toOption.getOrElse(false) =>
        Redirect(routes.HomeController.index()).withSession("userId" -> user.id.toString)
      case _ =>
        BadRequest(views.html.login(loginForm.withGlobalError("Invalid Credentials")))
    }
  }

  // End session
  def logout = Action {
    Redirect(routes.AuthController.showLogin()).withNewSession
  }
}