🍅 Dynamic Pomodoro Timer

Stop juggling tabs. Get in the zone.

This isn't just another timer app. I built this to solve a specific problem: the disconnect between to-do lists and doing the work.

Dynamic Pomodoro Timer anchors your focus session to a specific task, ensuring you're not just "working," but working on the right thing. Wrapped in a soothing Nordic color palette, it's designed to reduce eye strain and keep you calm while you crush your goals.

✨ Key Features

🎯 Anchored Focus: You don't just start a timer; you start a task. The UI highlights exactly what you should be doing right now.

🧠 Smart Cycles: Automatically manages the flow between Focus, Short Breaks, and Long Breaks.

📊 Productivity Reports: Tracks every completed cycle. See exactly how many hours of deep work you've logged.

💾 Bulletproof Persistence: Accidentally closed the tab? Refreshed the page? No problem. The timer state uses local storage to pick up exactly where you left off.

🎨 Nordic UI: A custom-designed interface based on the popular Nord theme. Minimalist, clean, and beautiful.

🔒 Private Accounts: Your tasks and data are yours. Full user authentication ensures privacy.

🛠 The Tech Stack

I chose a robust, industrial-strength stack for this project to ensure type safety and performance.

Component

Technology

Why?

Backend

Scala + Play Framework

High concurrency, type safety, and great developer experience.

Database

PostgreSQL + Slick

Relational data integrity with a functional database access library.

Frontend

Twirl + Vanilla JS

Server-side rendering for speed, with lightweight JS for the timer logic. No heavy frameworks required.

Styling

Custom CSS

Hand-crafted styles using CSS Variables for consistent theming.

🚀 Getting Started

Ready to focus? Let's get this running on your machine.

1. Prerequisites

Ensure you have the following installed:

Java JDK 21 (The engine)

SBT (Scala Build Tool)

PostgreSQL (The database)

2. Database Setup

Create the local database. Open your terminal or psql tool and run:

CREATE USER pomodoro_user WITH PASSWORD '1998';
CREATE DATABASE pomodoro_db OWNER pomodoro_user;


> Note: If you prefer different credentials, just update conf/application.conf.

3. Run the App

Clone the repo and fire it up:

# Install dependencies and start the server
sbt run


(Grab a coffee ☕ the first time you run this; SBT needs to download the internet).

Once you see Server started, open http://localhost:9000.

> Pro Tip: You will see a red button saying "Apply Evolutions". Click it! This automatically creates your database tables.

📂 Project Structure

Here is a map of the territory if you want to explore the code:

app/controllers/ 🎮

TaskController.scala: Handles CRUD operations for tasks.

ReportController.scala: Calculates productivity stats.

app/services/ 🧠

TaskService.scala: Contains the business logic, keeping controllers clean.

app/views/ 🖼️

Contains the HTML templates (Twirl).

public/javascripts/timer.js ⏱️

The brain of the frontend. Handles the countdown, state saving, and audio alerts.

conf/routes 🚦

The API definition file.
