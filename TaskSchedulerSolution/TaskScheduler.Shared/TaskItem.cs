using System;
using System.ComponentModel.DataAnnotations;

namespace TaskScheduler.Shared
{
    // REQUIREMENT: Classes
    // This is the main data model for our application.
    // It's a 'class' because it represents a complex object
    // with data and identity that will be passed by reference.
    public class TaskItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title is too long (100 char max)")]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(1);
        public bool IsComplete { get; set; }
        public DateTime? ReminderTime { get; set; }
        public bool IsReminderSent { get; set; }

        // This property uses a 'struct' as required.
        public ReminderConfig Config { get; set; } = new ReminderConfig(true, 15);
    }

    // REQUIREMENT: Structures (Structs)
    // This is a 'struct' because it's a small, lightweight
    // value type that just holds data. It's more efficient
    // for small, simple data containers.
    public struct ReminderConfig
    {
        public bool LogToFile;
        public int ReminderLeadTimeMinutes;

        // Structs can have constructors
        public ReminderConfig(bool logToFile, int leadTime)
        {
            LogToFile = logToFile;
            ReminderLeadTimeMinutes = leadTime;
        }
    }
}
