-- Create the user
CREATE USER pomodoro_user WITH PASSWORD '1998';

-- Create the database
CREATE DATABASE pomodoro_db OWNER pomodoro_user;

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE pomodoro_db TO pomodoro_user;

# --- !Ups

CREATE TABLE users (
                       id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                       email VARCHAR(255) UNIQUE NOT NULL,
                       password_hash VARCHAR(255),
                       display_name VARCHAR(100) DEFAULT NULL,
                       google_id VARCHAR(255) UNIQUE,
                       created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE tasks (
                       id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                       user_id UUID REFERENCES users(id) ON DELETE CASCADE,
                       description VARCHAR(255) NOT NULL,
                       estimated_pomodoros INT DEFAULT 1,
                       completed_pomodoros INT DEFAULT 0,
                       status VARCHAR(20) DEFAULT 'todo',
                       created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                       updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

# --- !Downs

DROP TABLE tasks;
DROP TABLE users;