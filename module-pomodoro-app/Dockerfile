# 1. Use an official image that has Java 21 and SBT installed
FROM sbtscala/scala-sbt:eclipse-temurin-jammy-21.0.2_1.9.9_3.4.0

# 2. Set the working directory inside the container
WORKDIR /app

# 3. Copy your project files into the container
COPY . /app

# 4. Build the application (create the start script)
RUN sbt stage

# 5. Expose port 9000 (Play's default)
EXPOSE 9000

# 6. Define the command to run the app
# We use the generated start script and pass the port and secret key
# NOTE: Replace 'dynamic-pomodoro-timer' with your exact project name from build.sbt if different
CMD ["./target/universal/stage/bin/dynamic-pomodoro-timer", "-Dplay.http.secret.key=${PLAY_HTTP_SECRET_KEY}", "-Dhttp.port=9000", "-Dplay.evolutions.db.default.autoApply=true"]