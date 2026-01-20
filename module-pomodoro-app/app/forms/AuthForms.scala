package forms
import play.api.data.Form
import play.api.data.Form._
import play.api.data.Forms.{email, mapping, nonEmptyText}
import play.api.data.format.Formats

/**
 * Data Transfer Object (DTO) for user registration
 * @param email User's email address
 * @param password User's desired password
 * @param confirmPassword Password confirmation for validation
 */
case class RegisterData(email: String, password: String, confirmPassword: String)

/**
 * Data Transfer Object (DTO) for user login
 * @param email User's email address
 * @param password User's password
 */
case class LoginData(email: String, password: String)

/**
 * Companion object containing form definitions for authentication
 * Provides validated forms for registration and login operations
 */
object AuthForms {
  /**
   * Registration form with email validation, password length requirement, and password confirmation
   * Email must be valid format, password must be at least 8 characters, and both passwords must match
   */
  val registerForm = Form(
    mapping(
      "email" -> email.verifying("Invalid email format", _.matches("^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$")),
      "password" -> nonEmptyText(minLength = 8, maxLength = 128).verifying(
        "Password must contain at least one letter and one number",
        pwd => pwd.exists(_.isLetter) && pwd.exists(_.isDigit)
      ),
      "confirmPassword" -> nonEmptyText
    )(RegisterData.apply)(RegisterData.unapply).verifying(
      "Passwords must match",
      data => data.password == data.confirmPassword
    )
  )

  /**
   * Login form with email and password validation
   * Both fields are required and trimmed
   */
  val loginForm = Form(
    mapping(
      "email" -> email.verifying("Invalid email format", _.matches("^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$")),
      "password" -> nonEmptyText
    )(LoginData.apply)(LoginData.unapply _)
  )
}