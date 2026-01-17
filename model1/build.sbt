name := """dynamic-pomodoro-timer"""
organization := "com.jlarcher"

version := "1.0-SNAPSHOT"

lazy val root = (project in file(".")).enablePlugins(PlayScala)

scalaVersion := "2.13.18"

libraryDependencies += guice
libraryDependencies += "org.scalatestplus.play" %% "scalatestplus-play" % "7.0.2" % Test

// Adds additional packages into Twirl
//TwirlKeys.templateImports += "com.jlarcher.controllers._"

// Adds additional packages into conf/routes
// play.sbt.routes.RoutesKeys.routesImport += "com.jlarcher.binders._"
libraryDependencies ++= Seq(
  guice,
  "org.playframework" %% "play-slick" % "6.2.0",           // Integración de Play con Slick
  "org.playframework" %% "play-slick-evolutions" % "6.2.0", // Para migraciones de DB
  "org.postgresql" % "postgresql" % "42.7.3",              // Driver de Postgres
  "com.h2database" % "h2" % "2.2.224",                    // H2 database for tests
  "com.github.t3hnar" %% "scala-bcrypt" % "4.3.0",           // Para hashear passwords
  "org.pac4j" %% "play-pac4j" % "12.0.0-PLAY3.0",          // Seguridad (OAuth)
  "org.scalatestplus.play" %% "scalatestplus-play" % "7.0.0" % Test
)