using TaskMgr.Api.Domain.Entities;

namespace TaskMgr.Api.Domain.Interfaces;

/// <summary>
/// Unit of Work interface for managing database transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Repository for User entities
    /// </summary>
    IRepository<User> Users { get; }
    
    /// <summary>
    /// Repository for Project entities
    /// </summary>
    IRepository<Project> Projects { get; }
    
    /// <summary>
    /// Repository for TaskItem entities
    /// </summary>
    IRepository<TaskItem> Tasks { get; }
    
    /// <summary>
    /// Saves all changes to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities affected</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Begins a database transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Database transaction</returns>
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Database transaction interface
/// </summary>
public interface IDbTransaction : IDisposable
{
    /// <summary>
    /// Commits the transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task CommitAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rolls back the transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
