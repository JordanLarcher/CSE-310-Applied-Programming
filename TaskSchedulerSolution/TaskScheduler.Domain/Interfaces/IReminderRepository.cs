namespace TaskScheduler.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskScheduler.Domain.Entities;

public interface IReminderRepository : IRepository<Reminder>
{
    System.Threading.Tasks.Task<IEnumerable<Reminder>> GetDueRemindersAsync(DateTime currentTime);
    System.Threading.Tasks.Task<IEnumerable<Reminder>> GetRemindersByTaskIdAsync(int taskId);
    System.Threading.Tasks.Task<IEnumerable<Reminder>> GetUnsentRemindersAsync();
    System.Threading.Tasks.Task MarkAsSentAsync(int reminderId);
}