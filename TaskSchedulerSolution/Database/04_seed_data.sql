-- =============================================
-- TaskScheduler Seed Data Script
-- PostgreSQL Version
-- Sample data for testing and demonstration
-- =============================================

\c task_scheduler_db;

-- =============================================
-- 1. Create Demo Users
-- Password for all demo users: "Password123!"
-- Hashed using BCrypt
-- =============================================
INSERT INTO "Users" ("Email", "PasswordHash", "FirstName", "LastName", "IsEmailVerified", "CreatedAt") VALUES
('john.doe@example.com', '$2a$11$YourHashedPasswordHere', 'John', 'Doe', TRUE, NOW() - INTERVAL '30 days'),
('jane.smith@example.com', '$2a$11$YourHashedPasswordHere', 'Jane', 'Smith', TRUE, NOW() - INTERVAL '20 days'),
('demo@taskscheduler.com', '$2a$11$YourHashedPasswordHere', 'Demo', 'User', TRUE, NOW() - INTERVAL '10 days')
ON CONFLICT ("Email") DO NOTHING;

-- =============================================
-- 2. Create Sample Categories
-- =============================================
INSERT INTO "Categories" ("UserId", "Name", "Color", "CreatedAt") VALUES
-- John Doe's categories
(1, 'Work', '#EF4444', NOW() - INTERVAL '25 days'),
(1, 'Personal', '#3B82F6', NOW() - INTERVAL '25 days'),
(1, 'Shopping', '#10B981', NOW() - INTERVAL '20 days'),
(1, 'Health', '#F59E0B', NOW() - INTERVAL '15 days'),

-- Jane Smith's categories
(2, 'Work Projects', '#8B5CF6', NOW() - INTERVAL '18 days'),
(2, 'Home', '#EC4899', NOW() - INTERVAL '18 days'),
(2, 'Fitness', '#14B8A6', NOW() - INTERVAL '10 days'),

-- Demo User's categories
(3, 'Getting Started', '#6366F1', NOW() - INTERVAL '8 days'),
(3, 'Tasks', '#F97316', NOW() - INTERVAL '8 days')
ON CONFLICT ("UserId", "Name") DO NOTHING;

-- =============================================
-- 3. Create Sample Tasks
-- =============================================
INSERT INTO "Tasks" ("UserId", "Title", "Description", "DueDate", "Priority", "Status", "CategoryId", "CreatedAt") VALUES
-- John Doe's tasks
(1, 'Complete project proposal', 'Finish the Q4 project proposal for the new client', NOW() + INTERVAL '3 days', 'High', 'InProgress', 1, NOW() - INTERVAL '5 days'),
(1, 'Team meeting preparation', 'Prepare agenda and slides for Monday team meeting', NOW() + INTERVAL '1 day', 'Medium', 'Pending', 1, NOW() - INTERVAL '2 days'),
(1, 'Code review', 'Review pull requests from team members', NOW() + INTERVAL '2 days', 'Medium', 'Pending', 1, NOW() - INTERVAL '1 day'),
(1, 'Buy groceries', 'Milk, eggs, bread, vegetables, chicken', NOW() + INTERVAL '1 day', 'Low', 'Pending', 3, NOW()),
(1, 'Call dentist', 'Schedule annual checkup appointment', NOW() + INTERVAL '7 days', 'Low', 'Pending', 4, NOW() - INTERVAL '3 days'),
(1, 'Gym workout', 'Cardio and strength training session', NOW() + INTERVAL '1 day', 'Medium', 'Pending', 4, NOW()),
(1, 'Finished task example', 'This task was completed last week', NOW() - INTERVAL '5 days', 'Medium', 'Completed', 2, NOW() - INTERVAL '10 days'),

-- Jane Smith's tasks
(2, 'Client presentation', 'Prepare and deliver presentation to client stakeholders', NOW() + INTERVAL '5 days', 'High', 'InProgress', 5, NOW() - INTERVAL '4 days'),
(2, 'Update documentation', 'Update API documentation with new endpoints', NOW() + INTERVAL '7 days', 'Medium', 'Pending', 5, NOW() - INTERVAL '2 days'),
(2, 'Home renovation planning', 'Get quotes from contractors for kitchen remodel', NOW() + INTERVAL '10 days', 'Low', 'Pending', 6, NOW() - INTERVAL '1 day'),
(2, 'Morning run', '5K run in the park', NOW() + INTERVAL '1 day', 'Medium', 'Pending', 7, NOW()),
(2, 'Yoga class', 'Evening yoga session at the studio', NOW() + INTERVAL '2 days', 'Low', 'Pending', 7, NOW()),

-- Demo User's tasks
(3, 'Welcome to TaskScheduler!', 'Explore the features and start organizing your tasks', NOW() + INTERVAL '7 days', 'Medium', 'Pending', 8, NOW() - INTERVAL '7 days'),
(3, 'Create your first category', 'Organize your tasks by creating custom categories', NOW() + INTERVAL '5 days', 'Low', 'Completed', 9, NOW() - INTERVAL '6 days'),
(3, 'Set up notifications', 'Configure your notification preferences in Settings', NOW() + INTERVAL '3 days', 'Low', 'Pending', 9, NOW() - INTERVAL '5 days')
ON CONFLICT DO NOTHING;

-- =============================================
-- 4. Create Sample Reminders
-- =============================================
INSERT INTO "Reminders" ("TaskId", "ReminderTime", "IsSent", "ReminderType", "CreatedAt") VALUES
-- Reminders for upcoming tasks
(1, NOW() + INTERVAL '2 days 9 hours', FALSE, 'OneTime', NOW()),
(2, NOW() + INTERVAL '12 hours', FALSE, 'OneTime', NOW()),
(3, NOW() + INTERVAL '1 day 10 hours', FALSE, 'OneTime', NOW()),
(8, NOW() + INTERVAL '4 days 8 hours', FALSE, 'OneTime', NOW()),
(11, NOW() + INTERVAL '2 days 9 hours', FALSE, 'OneTime', NOW())
ON CONFLICT DO NOTHING;

-- =============================================
-- 5. Update NotificationSettings for demo users
-- =============================================
-- Settings should be auto-created by trigger, but we can update them
UPDATE "NotificationSettings" 
SET 
    "EmailReminders" = TRUE,
    "DailyDigest" = TRUE,
    "WeeklySummary" = TRUE,
    "DailyDigestTime" = '8 hours',
    "WeeklySummaryDay" = 1
WHERE "UserId" IN (1, 2, 3);

-- =============================================
-- Display summary of seeded data
-- =============================================
SELECT 'Seed data created successfully!' as status;

SELECT 'Summary of seeded data:' as info;

SELECT 
    'Users' as entity,
    COUNT(*) as count
FROM "Users"
UNION ALL
SELECT 
    'Categories' as entity,
    COUNT(*) as count
FROM "Categories"
UNION ALL
SELECT 
    'Tasks' as entity,
    COUNT(*) as count
FROM "Tasks"
UNION ALL
SELECT 
    'Reminders' as entity,
    COUNT(*) as count
FROM "Reminders"
UNION ALL
SELECT 
    'NotificationSettings' as entity,
    COUNT(*) as count
FROM "NotificationSettings"
ORDER BY entity;

-- Show demo user credentials
SELECT 
    'Demo Users (Password: Password123!)' as info,
    "Email",
    "FirstName" || ' ' || "LastName" as "Name"
FROM "Users"
ORDER BY "CreatedAt";
