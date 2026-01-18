package controllers

import javax.inject._
import play.api.mvc._

class HomeController @Inject()(cc: ControllerComponents) extends AbstractController(cc) {

  // If has session -> Go to tasks. If not -> Show Landing Page
  def index = Action { implicit request =>
    request.session.get("userId") match {
      case Some(_) => Redirect(routes.TaskController.listTasks)
      case None    => Ok(views.html.index())
    }
  }
}