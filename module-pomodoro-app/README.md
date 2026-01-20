Dynamic Pomodoro Timer

This is a productivity app that combines a task list with a Pomodoro timer. I built it to help focus on specific tasks without getting distracted by complicated features. It’s designed to be clean, simple, and easy on the eyes (thanks to the Nordic color theme).

What it does

The core idea is simple: you create a task, estimate how many "pomodoros" (25-minute blocks) it will take, and then start the timer.

Focus Timer: Handles the standard work/break cycle. You can customize the times for work, short breaks, and long breaks.

Task List: You can add, edit, and delete tasks. The timer is "anchored" to a specific task so you know exactly what you're working on.

Reports: It tracks your completed cycles so you can see how much time you've actually spent working.

User Accounts: You have your own account, so your tasks and settings are private.

Persistence: If you accidentally close the tab or refresh the page, the timer remembers where it was.

Tech Stack

I used this project to work with a robust JVM stack:

Backend: Scala with Play Framework.

Database: PostgreSQL (using Slick for database access).

Frontend: Server-side rendered HTML (Twirl templates) with vanilla JavaScript. I kept the JS simple—no React or Angular, just clean logic to handle the timer state.

Styling: Custom CSS.

How to Run It

Prerequisites

You need to have these installed on your machine:

Java JDK 21

SBT (Scala Build Tool)

PostgreSQL

Database Setup

Before running the app, create a local database and a user for it. You can run these commands in your Postgres terminal (psql):

CREATE USER pomodoro_user WITH PASSWORD '1998';
CREATE DATABASE pomodoro_db OWNER pomodoro_user;


Note: If you want to use different credentials, just update the conf/application.conf file.

Running the App

Clone this repository.

Open your terminal in the project folder.

Run the server:

sbt run


(The first time you run this, it might take a while to download dependencies).

Open your browser and go to http://localhost:9000.

You will see a red button asking to apply a database script ("Apply Evolutions"). Click it. This creates the necessary tables for you.

Project Structure

If you want to look at the code:

app/controllers/: Handles the web requests (TaskController, AuthController, etc.).

app/services/: Contains the business logic, keeping the controllers clean.

app/views/: The HTML templates.

public/javascripts/: The logic for the timer and local storage.

conf/routes: Defines all the URLs for the application.

Credits

Created as a project to explore full-stack development with Scala. Feel free to use it or modify it for your own workflow.