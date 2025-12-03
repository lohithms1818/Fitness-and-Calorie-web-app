using FitnessApp.Domain.Entities;
using FitnessApp.Domain.Interfaces;
using FitnessApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Infrastructure.Repositories;

public class UserSubscriptionRepository : Repository<UserSubscription>, IUserSubscriptionRepository
{
    public UserSubscriptionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<UserSubscription?> GetActiveSubscriptionByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId && 
                        s.Status == SubscriptionStatus.Active && 
                        s.EndDate >= DateTime.UtcNow)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UserSubscription>> GetSubscriptionsByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
    {
        return await _dbSet
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId);
    }

    public async Task<bool> HasActiveSubscriptionAsync(string userId)
    {
        return await _dbSet
            .AnyAsync(s => s.UserId == userId && 
                          s.Status == SubscriptionStatus.Active && 
                          s.EndDate >= DateTime.UtcNow);
    }
}
