package controllers

import play.api.mvc._
import scala.concurrent.{Future, ExecutionContext}
import java.util.UUID

/**
 * Security trait for protecting actions that require authentication
 * Provides a helper method to wrap actions with authentication checks
 */
trait Secured {
  def controllerComponents: ControllerComponents

  /**
   * Authentication helper for async actions requiring database access
   * @param f Function that takes userId and returns an Action
   * @param ec ExecutionContext for async operations
   * @return Action that checks for valid user session before executing
   */
  def withAuth(f: UUID => Request[AnyContent] => Future[Result])(implicit ec: ExecutionContext): Action[AnyContent] = {
    controllerComponents.actionBuilder.async { request =>
      request.session.get("userId") match {
        case Some(id) => f(UUID.fromString(id))(request)
        case None => Future.successful(Results.Redirect(routes.AuthController.showLogin()))
      }
    }
  }
}