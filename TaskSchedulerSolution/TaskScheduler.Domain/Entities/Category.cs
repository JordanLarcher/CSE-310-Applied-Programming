using System;
using System.Collections.Generic;

namespace TaskScheduler.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation Properties
        public virtual User User { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }

        public Category()
        {
            Tasks = new HashSet<Task>();
            CreatedAt = DateTime.UtcNow;
            Color = "#3B82F6";
        }
    }
}