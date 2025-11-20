-- =============================================
-- TaskScheduler Functions & Triggers
-- PostgreSQL Version
-- =============================================

\c task_scheduler_db;

-- =============================================
-- 1. Function to update UpdatedAt timestamp
-- =============================================
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW."UpdatedAt" = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION update_updated_at_column() IS 'Automatically updates UpdatedAt timestamp on row update';

-- =============================================
-- 2. Trigger for Tasks UpdatedAt
-- =============================================
DROP TRIGGER IF EXISTS trigger_tasks_updated_at ON "Tasks";
CREATE TRIGGER trigger_tasks_updated_at
    BEFORE UPDATE ON "Tasks"
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- =============================================
-- 3. Trigger for NotificationSettings UpdatedAt
-- =============================================
DROP TRIGGER IF EXISTS trigger_notification_settings_updated_at ON "NotificationSettings";
CREATE TRIGGER trigger_notification_settings_updated_at
    BEFORE UPDATE ON "NotificationSettings"
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- =============================================
-- 4. Function to check if task is overdue
-- =============================================
CREATE OR REPLACE FUNCTION update_task_overdue_status()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW."DueDate" IS NOT NULL AND NEW."Status" != 'Completed' THEN
        NEW."IsOverdue" = (NEW."DueDate" < NOW());
    ELSE
        NEW."IsOverdue" = FALSE;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION update_task_overdue_status() IS 'Automatically updates IsOverdue flag based on DueDate';

-- =============================================
-- 5. Trigger to update IsOverdue on insert/update
-- =============================================
DROP TRIGGER IF EXISTS trigger_task_overdue_check ON "Tasks";
CREATE TRIGGER trigger_task_overdue_check
    BEFORE INSERT OR UPDATE OF "DueDate", "Status" ON "Tasks"
    FOR EACH ROW
    EXECUTE FUNCTION update_task_overdue_status();

-- =============================================
-- 6. Function to auto-create NotificationSettings
-- =============================================
CREATE OR REPLACE FUNCTION create_default_notification_settings()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO "NotificationSettings" ("UserId")
    VALUES (NEW."Id")
    ON CONFLICT ("UserId") DO NOTHING;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION create_default_notification_settings() IS 'Creates default notification settings for new users';

-- =============================================
-- 7. Trigger to create NotificationSettings on user creation
-- =============================================
DROP TRIGGER IF EXISTS trigger_create_notification_settings ON "Users";
CREATE TRIGGER trigger_create_notification_settings
    AFTER INSERT ON "Users"
    FOR EACH ROW
    EXECUTE FUNCTION create_default_notification_settings();

-- =============================================
-- 8. Function to update CompletedAt when task is completed
-- =============================================
CREATE OR REPLACE FUNCTION update_task_completed_at()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW."Status" = 'Completed' AND OLD."Status" != 'Completed' THEN
        NEW."CompletedAt" = NOW();
    ELSIF NEW."Status" != 'Completed' THEN
        NEW."CompletedAt" = NULL;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION update_task_completed_at() IS 'Sets CompletedAt timestamp when task status changes to Completed';

-- =============================================
-- 9. Trigger for automatic CompletedAt
-- =============================================
DROP TRIGGER IF EXISTS trigger_task_completed_at ON "Tasks";
CREATE TRIGGER trigger_task_completed_at
    BEFORE UPDATE OF "Status" ON "Tasks"
    FOR EACH ROW
    EXECUTE FUNCTION update_task_completed_at();

-- =============================================
-- Display success message
-- =============================================
SELECT 'Functions and triggers created successfully!' as status;
