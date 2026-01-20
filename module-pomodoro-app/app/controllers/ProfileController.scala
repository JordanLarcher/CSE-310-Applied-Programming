package controllers

import javax.inject.Inject
import play.api.mvc._
import scala.concurrent.{ExecutionContext, Future}
import slick.jdbc.PostgresProfile.api._
import models.{User, Users}
import play.api.db.slick.{DatabaseConfigProvider, HasDatabaseConfigProvider}
import slick.jdbc.JdbcProfile
import play.api.data.Form
import play.api.data.Forms._
import play.api.i18n.I18nSupport


/**
 * Data class for the Profile Form
 */
case class ProfileData(displayName: String)

class ProfileController @Inject()(
    protected val dbConfigProvider: DatabaseConfigProvider,
    cc: ControllerComponents
)(implicit ec: ExecutionContext) extends AbstractController(cc) with HasDatabaseConfigProvider[JdbcProfile] with Secured with I18nSupport {

    override val controllerComponents: ControllerComponents = cc

    // Validation for the profile form
    val profileForm = Form(
        mapping(
        "displayName" -> nonEmptyText(minLength = 2, maxLength = 50)
        )(ProfileData.apply)(ProfileData.unapply)
    )

    // GET /profile - Display the profile page
    def showProfile = withAuth { userId => implicit request =>
        db.run(Users.users.filter(_.id === userId).result.headOption).map {
        case Some(user) =>
            // Pre-fill the form with the current display name (or empty string if None)
            val filledForm = profileForm.fill(ProfileData(user.displayName.getOrElse("")))
            Ok(views.html.profile(user, filledForm))
        case None => 
            // If user not found in DB (rare edge case), log them out
            Redirect(routes.AuthController.logout())
        }
    }



    // POST /profile - Handle profile updates
    def updateProfile = withAuth { userId => implicit request =>
        profileForm.bindFromRequest().fold(
        // If validation fails (e.g., name too short), re-render page with errors
        formWithErrors => {
            db.run(Users.users.filter(_.id === userId).result.headOption).map {
            case Some(user) => BadRequest(views.html.profile(user, formWithErrors))
            case None => Redirect(routes.AuthController.logout())
            }
        },
        // If validation succeeds, update the database
        data => {
            val updateAction = Users.users.filter(_.id === userId).map(_.displayName).update(Some(data.displayName))
            db.run(updateAction).map { _ =>
            Redirect(routes.ProfileController.showProfile).flashing("success" -> "Profile updated successfully")
            }
        }
        )
    }
}