package controllers

import play.api.mvc._
import scala.concurrent.{Future, ExecutionContext}
import java.util.UUID
// A Trait is similar to an interface in Scala
// Trait that can be mixed into any controller to protect it
trait Secured {
  // Necessary to access request.session
  def controllerComponents: ControllerComponents

  // Defines an action that requires authentication
  def withAuth(f: UUID => Request[AnyContent] => Result): Action[AnyContent] = {
    controllerComponents.actionBuilder { request =>
      request.session.get("userId") match {
        case Some(id) => f(UUID.fromString(id))(request) // If there is a session, pass the UUID
        case None => Results.Redirect(routes.AuthController.showLogin()) // If not, go to login
      }
    }
  }
}