-- =============================================
-- TaskScheduler Useful Queries
-- PostgreSQL Version
-- Common queries for monitoring and maintenance
-- =============================================

\c task_scheduler_db;

-- =============================================
-- 1. View all users with their task counts
-- =============================================
SELECT 
    u."Id",
    u."Email",
    u."FirstName" || ' ' || u."LastName" as "FullName",
    COUNT(DISTINCT t."Id") as "TotalTasks",
    COUNT(DISTINCT CASE WHEN t."Status" = 'Completed' THEN t."Id" END) as "CompletedTasks",
    COUNT(DISTINCT CASE WHEN t."Status" = 'Pending' THEN t."Id" END) as "PendingTasks",
    COUNT(DISTINCT CASE WHEN t."IsOverdue" = TRUE THEN t."Id" END) as "OverdueTasks",
    u."CreatedAt" as "JoinedDate",
    u."LastLoginAt"
FROM "Users" u
LEFT JOIN "Tasks" t ON u."Id" = t."UserId"
GROUP BY u."Id", u."Email", u."FirstName", u."LastName", u."CreatedAt", u."LastLoginAt"
ORDER BY u."CreatedAt" DESC;

-- =============================================
-- 2. View tasks with all details
-- =============================================
SELECT 
    t."Id",
    t."Title",
    t."Description",
    t."Status",
    t."Priority",
    t."DueDate",
    t."IsOverdue",
    t."CompletedAt",
    c."Name" as "CategoryName",
    c."Color" as "CategoryColor",
    u."Email" as "UserEmail",
    u."FirstName" || ' ' || u."LastName" as "UserName",
    COUNT(r."Id") as "ReminderCount"
FROM "Tasks" t
INNER JOIN "Users" u ON t."UserId" = u."Id"
LEFT JOIN "Categories" c ON t."CategoryId" = c."Id"
LEFT JOIN "Reminders" r ON t."Id" = r."TaskId"
GROUP BY t."Id", t."Title", t."Description", t."Status", t."Priority", 
         t."DueDate", t."IsOverdue", t."CompletedAt", 
         c."Name", c."Color", u."Email", u."FirstName", u."LastName"
ORDER BY t."DueDate" ASC NULLS LAST;

-- =============================================
-- 3. Find overdue tasks
-- =============================================
SELECT 
    t."Id",
    t."Title",
    u."Email" as "UserEmail",
    t."DueDate",
    NOW() - t."DueDate" as "OverdueBy",
    t."Priority",
    c."Name" as "Category"
FROM "Tasks" t
INNER JOIN "Users" u ON t."UserId" = u."Id"
LEFT JOIN "Categories" c ON t."CategoryId" = c."Id"
WHERE t."IsOverdue" = TRUE 
  AND t."Status" != 'Completed'
ORDER BY t."DueDate" ASC;

-- =============================================
-- 4. Find pending reminders
-- =============================================
SELECT 
    r."Id",
    t."Title" as "TaskTitle",
    u."Email" as "UserEmail",
    r."ReminderTime",
    r."ReminderType",
    r."ReminderTime" - NOW() as "TimeUntilReminder"
FROM "Reminders" r
INNER JOIN "Tasks" t ON r."TaskId" = t."Id"
INNER JOIN "Users" u ON t."UserId" = u."Id"
WHERE r."IsSent" = FALSE
  AND r."ReminderTime" > NOW()
ORDER BY r."ReminderTime" ASC;

-- =============================================
-- 5. User productivity statistics
-- =============================================
SELECT 
    u."Email",
    COUNT(t."Id") as "TotalTasks",
    COUNT(CASE WHEN t."Status" = 'Completed' THEN 1 END) as "Completed",
    COUNT(CASE WHEN t."Status" = 'Pending' THEN 1 END) as "Pending",
    COUNT(CASE WHEN t."Status" = 'InProgress' THEN 1 END) as "InProgress",
    ROUND(
        100.0 * COUNT(CASE WHEN t."Status" = 'Completed' THEN 1 END) / 
        NULLIF(COUNT(t."Id"), 0), 2
    ) as "CompletionRate%",
    COUNT(CASE WHEN t."IsOverdue" = TRUE AND t."Status" != 'Completed' THEN 1 END) as "CurrentlyOverdue",
    COUNT(DISTINCT c."Id") as "CategoriesUsed"
FROM "Users" u
LEFT JOIN "Tasks" t ON u."Id" = t."UserId"
LEFT JOIN "Categories" c ON u."Id" = c."UserId"
GROUP BY u."Id", u."Email"
ORDER BY "TotalTasks" DESC;

-- =============================================
-- 6. Tasks by category
-- =============================================
SELECT 
    c."Name" as "Category",
    c."Color",
    u."Email" as "Owner",
    COUNT(t."Id") as "TaskCount",
    COUNT(CASE WHEN t."Status" = 'Completed' THEN 1 END) as "Completed",
    COUNT(CASE WHEN t."Status" = 'Pending' THEN 1 END) as "Pending"
FROM "Categories" c
INNER JOIN "Users" u ON c."UserId" = u."Id"
LEFT JOIN "Tasks" t ON c."Id" = t."CategoryId"
GROUP BY c."Id", c."Name", c."Color", u."Email"
ORDER BY "TaskCount" DESC;

-- =============================================
-- 7. Recent activity (last 7 days)
-- =============================================
SELECT 
    'Task Created' as "ActivityType",
    t."Title" as "Description",
    u."Email" as "User",
    t."CreatedAt" as "Timestamp"
FROM "Tasks" t
INNER JOIN "Users" u ON t."UserId" = u."Id"
WHERE t."CreatedAt" >= NOW() - INTERVAL '7 days'

UNION ALL

SELECT 
    'Task Completed' as "ActivityType",
    t."Title" as "Description",
    u."Email" as "User",
    t."CompletedAt" as "Timestamp"
FROM "Tasks" t
INNER JOIN "Users" u ON t."UserId" = u."Id"
WHERE t."CompletedAt" >= NOW() - INTERVAL '7 days'
  AND t."CompletedAt" IS NOT NULL

UNION ALL

SELECT 
    'User Registered' as "ActivityType",
    u."FirstName" || ' ' || u."LastName" as "Description",
    u."Email" as "User",
    u."CreatedAt" as "Timestamp"
FROM "Users" u
WHERE u."CreatedAt" >= NOW() - INTERVAL '7 days'

ORDER BY "Timestamp" DESC;

-- =============================================
-- 8. Database size and table statistics
-- =============================================
SELECT 
    schemaname as "Schema",
    tablename as "TableName",
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) as "TotalSize",
    pg_size_pretty(pg_relation_size(schemaname||'.'||tablename)) as "TableSize",
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename) - pg_relation_size(schemaname||'.'||tablename)) as "IndexSize",
    n_live_tup as "RowCount"
FROM pg_stat_user_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;

-- =============================================
-- 9. Find tasks with no reminders
-- =============================================
SELECT 
    t."Id",
    t."Title",
    t."DueDate",
    u."Email" as "UserEmail",
    c."Name" as "Category"
FROM "Tasks" t
INNER JOIN "Users" u ON t."UserId" = u."Id"
LEFT JOIN "Categories" c ON t."CategoryId" = c."Id"
LEFT JOIN "Reminders" r ON t."Id" = r."TaskId"
WHERE r."Id" IS NULL
  AND t."Status" != 'Completed'
  AND t."DueDate" > NOW()
ORDER BY t."DueDate" ASC;

-- =============================================
-- 10. Users with disabled notifications
-- =============================================
SELECT 
    u."Email",
    ns."EmailReminders",
    ns."OverdueAlerts",
    ns."TaskUpdates",
    ns."DailyDigest",
    ns."WeeklySummary"
FROM "Users" u
INNER JOIN "NotificationSettings" ns ON u."Id" = ns."UserId"
WHERE ns."EmailReminders" = FALSE 
   OR ns."OverdueAlerts" = FALSE
   OR ns."TaskUpdates" = FALSE;
