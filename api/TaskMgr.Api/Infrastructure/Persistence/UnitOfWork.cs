using Microsoft.EntityFrameworkCore.Storage;
using TaskMgr.Api.Data;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Domain.Interfaces;

namespace TaskMgr.Api.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation using Entity Framework
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly TaskMgrDbContext _context;
    private bool _disposed = false;

    // Lazy-loaded repositories
    private IRepository<User>? _users;
    private IRepository<Project>? _projects;
    private IRepository<TaskItem>? _tasks;

    /// <summary>
    /// Initializes a new instance of the UnitOfWork class
    /// </summary>
    /// <param name="context">Database context</param>
    public UnitOfWork(TaskMgrDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public IRepository<User> Users =>
        _users ??= new Repository<User>(_context);

    /// <inheritdoc />
    public IRepository<Project> Projects =>
        _projects ??= new Repository<Project>(_context);

    /// <inheritdoc />
    public IRepository<TaskItem> Tasks =>
        _tasks ??= new Repository<TaskItem>(_context);

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new DbTransactionWrapper(transaction);
    }

    /// <summary>
    /// Releases all resources used by the UnitOfWork
    /// </summary>
    /// <param name="disposing">True if disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Wrapper for Entity Framework database transaction
/// </summary>
internal class DbTransactionWrapper : IDbTransaction
{
    private readonly IDbContextTransaction _transaction;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the DbTransactionWrapper class
    /// </summary>
    /// <param name="transaction">EF Core transaction</param>
    public DbTransactionWrapper(IDbContextTransaction transaction)
    {
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.RollbackAsync(cancellationToken);
    }

    /// <summary>
    /// Releases all resources used by the transaction
    /// </summary>
    /// <param name="disposing">True if disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction.Dispose();
        }
        _disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
