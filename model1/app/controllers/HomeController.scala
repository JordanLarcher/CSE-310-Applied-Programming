package controllers

import javax.inject._
import play.api._
import play.api.mvc._
import play.api.i18n.I18nSupport
import scala.concurrent.{ExecutionContext, Future}
import java.util.UUID

/**
 * Controller for handling the application's home page and routing
 * Redirects authenticated users to tasks, unauthenticated users to login
 */
@Singleton
class HomeController @Inject()(
  val controllerComponents: ControllerComponents
)(implicit ec: ExecutionContext) extends BaseController with I18nSupport {

  /**
   * Home page action that checks user authentication status
   * Redirects to appropriate page based on authentication state
   * @param request implicit HTTP request
   * @return Redirect to tasks if authenticated, login page if not
   */
  def index() = Action { implicit request: Request[AnyContent] =>
    request.session.get("userId") match {
      case Some(userId) =>
        // User is authenticated, redirect to tasks list page
        Redirect(routes.TaskController.listTasks)
      case None =>
        // User is not authenticated, redirect to login page
        Redirect(routes.AuthController.showLogin())
    }
  }
}
