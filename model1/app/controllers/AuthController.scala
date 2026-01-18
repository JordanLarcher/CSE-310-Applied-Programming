package controllers

import javax.inject.Inject
import models.{User, Users}
import play.api.mvc._
import play.api.i18n.I18nSupport

import scala.concurrent.{ExecutionContext, Future}
import slick.jdbc.PostgresProfile.api._
import com.github.t3hnar.bcrypt._
import forms.AuthForms._

import java.util.UUID
import play.api.db.slick.{DatabaseConfigProvider, HasDatabaseConfigProvider}
import play.api.http.Writeable.wByteArray
import slick.jdbc.JdbcProfile
import play.api.data.Form
import play.api.data.Forms._

import forms._
import forms.AuthForms._

class AuthController @Inject()(
                                protected val dbConfigProvider: DatabaseConfigProvider,
                                cc: ControllerComponents,
                                messagesApi: play.api.i18n.MessagesApi
                              )(implicit ec: ExecutionContext) extends AbstractController(cc) with HasDatabaseConfigProvider[JdbcProfile] with I18nSupport {

  /**
   * Display the registration form
   * @param request implicit HTTP request
   * @return HTML page with registration form
   */
  def showRegister = Action { implicit request =>
    Ok(views.html.register(registerForm))
  }

  /**
   * Process user registration with password hashing and validation
   * @param request implicit HTTP request containing form data
   * @return Redirect to home page on success or error page on failure
   */
  def register = Action.async(parse.form(registerForm)) { implicit request =>
    val data = request.body
    
    // Secure password hashing with error handling
    data.password.bcryptSafeBounded(12) match {
      case hashed if hashed.isSuccess =>
        // Create User Object with extracted display name from email
        val displayName = data.email.split("@").headOption.getOrElse("User")
        val user = User(
          UUID.randomUUID(),
          data.email,
          Some(hashed.get),
          Some(displayName),
          None,
          new java.sql.Timestamp(System.currentTimeMillis())
        )

        // Insert the new user in DB and redirect to home
        db.run(Users.users += user).map { _ =>
          Redirect(routes.HomeController.index()).withSession("userId" -> user.id.toString)
        }.recover {
          case ex: Exception =>
            BadRequest(views.html.register(registerForm.withGlobalError("Registration failed: Email may already exist")))
        }
      case _ =>
        Future.successful(BadRequest(views.html.register(registerForm.withGlobalError("Password hashing failed"))))
    }
  }

  /**
   * Display the login form
   * @param request implicit HTTP request
   * @return HTML page with login form
   */
  def showLogin = Action { implicit request =>
    Ok(views.html.login(loginForm))
  }

  /**
   * Process user login with secure password verification
   * @param request implicit HTTP request containing login credentials
   * @return Redirect to home page on success or error page on failure
   */
  def login = Action.async(parse.form(loginForm)) { implicit request =>
    val data = request.body

    // Find User by Email and verify password
    db.run(Users.users.filter(_.email === data.email).result.headOption).map {
      case Some(user) =>
        // Secure password verification with proper error handling
        user.passwordHash match {
          case Some(hash) =>
            data.password.isBcryptedSafeBounded(hash) match {
              case verified if verified.isSuccess && verified.get =>
                Redirect(routes.HomeController.index()).withSession("userId" -> user.id.toString)
              case _ =>
                BadRequest(views.html.login(loginForm.withGlobalError("Invalid email or password")))
            }
          case None =>
            BadRequest(views.html.login(loginForm.withGlobalError("Invalid email or password")))
        }
      case None =>
        BadRequest(views.html.login(loginForm.withGlobalError("Invalid email or password")))
    }
  }

  /**
   * End user session by clearing session data
   * @return Redirect to login page
   */
  def logout = Action {
    Redirect(routes.AuthController.showLogin()).withNewSession
  }
}