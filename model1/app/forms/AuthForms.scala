package forms
import play.api.data.Form
import play.api.data.Form._
import play.api.data.Forms.{email, mapping, nonEmptyText}
import play.api.data.format.Formats

// These are the DTOs Object Transporters
// These are immutable classes as well
case class RegisterData(email: String, password: String, confirmPassword: String)
case class LoginData(email: String, password: String)

object AuthForms {
  // This is Registration form that validates email and password length
  // The keyword val declares that this is an immutable variable
  val registerForm = Form(
    mapping(
      "email" -> email,
      "password" -> nonEmptyText(minLength = 8),
      "confirmPassword" -> nonEmptyText
    )(RegisterData.apply)(RegisterData.unapply).verifying(
      "Passwords must match",
      data => data.password == data.confirmPassword
    )
  )

  // Login Form
  val loginForm = Form(
    mapping(
      "email" -> email,
      "password" -> nonEmptyText
    )(LoginData.apply)(LoginData.unapply)
  )
}