using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using TaskScheduler.Domain.Interfaces;
using TaskScheduler.Infrastructure.Data;

namespace TaskScheduler.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;

        public IUserRepository Users { get; }
        public ITaskRepository Tasks { get; }
        public IReminderRepository Reminders { get; }
        public ICategoryRepository Categories { get; }
        public INotificationSettingsRepository NotificationSettings { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            Tasks = new TaskRepository(_context);
            Reminders = new ReminderRepository(_context);
            Categories = new CategoryRepository(_context);
            NotificationSettings = new NotificationSettingsRepository(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollBackAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollBackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}