using FitnessApp.Domain.Entities;
using FitnessApp.Domain.Interfaces;
using FitnessApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Infrastructure.Repositories;

public class PaymentTransactionRepository : Repository<PaymentTransaction>, IPaymentTransactionRepository
{
    public PaymentTransactionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PaymentTransaction>> GetTransactionsByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(t => t.Subscription)
                .ThenInclude(s => s!.Plan)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<PaymentTransaction?> GetByStripePaymentIntentIdAsync(string paymentIntentId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.StripePaymentIntentId == paymentIntentId);
    }

    public async Task<IEnumerable<PaymentTransaction>> GetTransactionsBySubscriptionIdAsync(int subscriptionId)
    {
        return await _dbSet
            .Where(t => t.SubscriptionId == subscriptionId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}
