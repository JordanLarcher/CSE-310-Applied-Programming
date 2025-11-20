-- =============================================
-- TaskScheduler Tables Creation Script
-- PostgreSQL Version
-- =============================================

-- Ensure we're using the correct database
\c task_scheduler_db;

-- =============================================
-- 1. Users Table
-- =============================================
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "PasswordHash" VARCHAR(500) NOT NULL,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "IsEmailVerified" BOOLEAN NOT NULL DEFAULT FALSE,
    "GoogleId" VARCHAR(255) NULL,
    "EmailVerificationToken" VARCHAR(500) NULL,
    "PasswordResetToken" VARCHAR(500) NULL,
    "PasswordResetTokenExpiry" TIMESTAMP WITH TIME ZONE NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "LastLoginAt" TIMESTAMP WITH TIME ZONE NULL,
    
    CONSTRAINT "CHK_Users_Email" CHECK ("Email" ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$')
);

-- Create indexes for Users
CREATE INDEX "IX_Users_Email" ON "Users" ("Email");
CREATE INDEX "IX_Users_GoogleId" ON "Users" ("GoogleId") WHERE "GoogleId" IS NOT NULL;
CREATE INDEX "IX_Users_CreatedAt" ON "Users" ("CreatedAt");

COMMENT ON TABLE "Users" IS 'Application users and authentication data';
COMMENT ON COLUMN "Users"."Email" IS 'User email address (unique identifier)';
COMMENT ON COLUMN "Users"."PasswordHash" IS 'Hashed password for authentication';
COMMENT ON COLUMN "Users"."GoogleId" IS 'Google OAuth ID for social login';

-- =============================================
-- 2. Categories Table
-- =============================================
CREATE TABLE IF NOT EXISTS "Categories" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL,
    "Name" VARCHAR(100) NOT NULL,
    "Color" TEXT NOT NULL DEFAULT '#3B82F6',
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_Categories_Users_UserId" 
        FOREIGN KEY ("UserId") 
        REFERENCES "Users" ("Id") 
        ON DELETE CASCADE,
    
    CONSTRAINT "UQ_Categories_UserId_Name" UNIQUE ("UserId", "Name")
);

-- Create indexes for Categories
CREATE INDEX "IX_Categories_UserId" ON "Categories" ("UserId");
CREATE INDEX "IX_Categories_Name" ON "Categories" ("Name");

COMMENT ON TABLE "Categories" IS 'User-defined task categories';
COMMENT ON COLUMN "Categories"."Color" IS 'Hex color code for category display';

-- =============================================
-- 3. Tasks Table
-- =============================================
CREATE TABLE IF NOT EXISTS "Tasks" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL,
    "Title" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(2000) NOT NULL DEFAULT '',
    "DueDate" TIMESTAMP WITH TIME ZONE NULL,
    "Priority" VARCHAR(50) NOT NULL DEFAULT 'Low',
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Pending',
    "CategoryId" INTEGER NULL,
    "IsOverdue" BOOLEAN NOT NULL DEFAULT FALSE,
    "CompletedAt" TIMESTAMP WITH TIME ZONE NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NULL,
    
    CONSTRAINT "FK_Tasks_Users_UserId" 
        FOREIGN KEY ("UserId") 
        REFERENCES "Users" ("Id") 
        ON DELETE CASCADE,
    
    CONSTRAINT "FK_Tasks_Categories_CategoryId" 
        FOREIGN KEY ("CategoryId") 
        REFERENCES "Categories" ("Id") 
        ON DELETE SET NULL,
    
    CONSTRAINT "CHK_Tasks_Priority" CHECK ("Priority" IN ('Low', 'Medium', 'High')),
    CONSTRAINT "CHK_Tasks_Status" CHECK ("Status" IN ('Pending', 'InProgress', 'Completed'))
);

-- Create indexes for Tasks
CREATE INDEX "IX_Tasks_UserId" ON "Tasks" ("UserId");
CREATE INDEX "IX_Tasks_CategoryId" ON "Tasks" ("CategoryId") WHERE "CategoryId" IS NOT NULL;
CREATE INDEX "IX_Tasks_DueDate" ON "Tasks" ("DueDate") WHERE "DueDate" IS NOT NULL;
CREATE INDEX "IX_Tasks_Status" ON "Tasks" ("Status");
CREATE INDEX "IX_Tasks_Priority" ON "Tasks" ("Priority");
CREATE INDEX "IX_Tasks_UserId_Status" ON "Tasks" ("UserId", "Status");
CREATE INDEX "IX_Tasks_UserId_DueDate" ON "Tasks" ("UserId", "DueDate") WHERE "DueDate" IS NOT NULL;
CREATE INDEX "IX_Tasks_IsOverdue" ON "Tasks" ("IsOverdue") WHERE "IsOverdue" = TRUE;

-- Full-text search index for task title and description
CREATE INDEX "IX_Tasks_Search" ON "Tasks" USING gin(to_tsvector('english', "Title" || ' ' || "Description"));

COMMENT ON TABLE "Tasks" IS 'User tasks and to-do items';
COMMENT ON COLUMN "Tasks"."Priority" IS 'Task priority: Low, Medium, High';
COMMENT ON COLUMN "Tasks"."Status" IS 'Task status: Pending, InProgress, Completed';
COMMENT ON COLUMN "Tasks"."IsOverdue" IS 'Computed field indicating if task is past due date';

-- =============================================
-- 4. Reminders Table
-- =============================================
CREATE TABLE IF NOT EXISTS "Reminders" (
    "Id" SERIAL PRIMARY KEY,
    "TaskId" INTEGER NOT NULL,
    "ReminderTime" TIMESTAMP WITH TIME ZONE NOT NULL,
    "IsSent" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "SentAt" TIMESTAMP WITH TIME ZONE NULL,
    "ReminderType" VARCHAR(50) NOT NULL DEFAULT 'OneTime',
    
    CONSTRAINT "FK_Reminders_Tasks_TaskId" 
        FOREIGN KEY ("TaskId") 
        REFERENCES "Tasks" ("Id") 
        ON DELETE CASCADE,
    
    CONSTRAINT "CHK_Reminders_Type" CHECK ("ReminderType" IN ('OneTime', 'Daily', 'Weekly'))
);

-- Create indexes for Reminders
CREATE INDEX "IX_Reminders_TaskId" ON "Reminders" ("TaskId");
CREATE INDEX "IX_Reminders_ReminderTime" ON "Reminders" ("ReminderTime");
CREATE INDEX "IX_Reminders_IsSent" ON "Reminders" ("IsSent") WHERE "IsSent" = FALSE;
CREATE INDEX "IX_Reminders_ReminderTime_IsSent" ON "Reminders" ("ReminderTime", "IsSent") WHERE "IsSent" = FALSE;

COMMENT ON TABLE "Reminders" IS 'Task reminders and notifications';
COMMENT ON COLUMN "Reminders"."ReminderType" IS 'Reminder type: OneTime, Daily, Weekly';
COMMENT ON COLUMN "Reminders"."IsSent" IS 'Flag indicating if reminder was sent';

-- =============================================
-- 5. NotificationSettings Table
-- =============================================
CREATE TABLE IF NOT EXISTS "NotificationSettings" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL UNIQUE,
    "EmailReminders" BOOLEAN NOT NULL DEFAULT TRUE,
    "OverdueAlerts" BOOLEAN NOT NULL DEFAULT TRUE,
    "TaskUpdates" BOOLEAN NOT NULL DEFAULT TRUE,
    "DailyDigest" BOOLEAN NOT NULL DEFAULT FALSE,
    "WeeklySummary" BOOLEAN NOT NULL DEFAULT TRUE,
    "DailyDigestTime" INTERVAL NOT NULL DEFAULT '8 hours',
    "WeeklySummaryDay" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_NotificationSettings_Users_UserId" 
        FOREIGN KEY ("UserId") 
        REFERENCES "Users" ("Id") 
        ON DELETE CASCADE,
    
    CONSTRAINT "CHK_NotificationSettings_Day" CHECK ("WeeklySummaryDay" BETWEEN 0 AND 6)
);

-- Create indexes for NotificationSettings
CREATE UNIQUE INDEX "IX_NotificationSettings_UserId" ON "NotificationSettings" ("UserId");

COMMENT ON TABLE "NotificationSettings" IS 'User notification preferences';
COMMENT ON COLUMN "NotificationSettings"."WeeklySummaryDay" IS 'Day of week (0=Sunday, 6=Saturday)';
COMMENT ON COLUMN "NotificationSettings"."DailyDigestTime" IS 'Time of day for daily digest email';

-- =============================================
-- Display success message
-- =============================================
SELECT 'All tables created successfully!' as status;
SELECT 
    table_name,
    (SELECT COUNT(*) FROM information_schema.columns WHERE table_name = t.table_name) as column_count
FROM information_schema.tables t
WHERE table_schema = 'public' 
    AND table_type = 'BASE TABLE'
ORDER BY table_name;
