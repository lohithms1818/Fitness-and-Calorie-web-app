using FitnessApp.Domain.Interfaces;
using FitnessApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace FitnessApp.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation for managing transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    
    private ISubscriptionPlanRepository? _subscriptionPlans;
    private IUserSubscriptionRepository? _userSubscriptions;
    private IFitnessClassRepository? _fitnessClasses;
    private IClassBookingRepository? _classBookings;
    private IPaymentTransactionRepository? _paymentTransactions;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public ISubscriptionPlanRepository SubscriptionPlans => 
        _subscriptionPlans ??= new SubscriptionPlanRepository(_context);

    public IUserSubscriptionRepository UserSubscriptions => 
        _userSubscriptions ??= new UserSubscriptionRepository(_context);

    public IFitnessClassRepository FitnessClasses => 
        _fitnessClasses ??= new FitnessClassRepository(_context);

    public IClassBookingRepository ClassBookings => 
        _classBookings ??= new ClassBookingRepository(_context);

    public IPaymentTransactionRepository PaymentTransactions => 
        _paymentTransactions ??= new PaymentTransactionRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
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
        _context.Dispose();
    }
}
