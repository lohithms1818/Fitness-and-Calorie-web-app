using FitnessApp.Domain.Entities;
using FitnessApp.Domain.Interfaces;
using FitnessApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Infrastructure.Repositories;

public class SubscriptionPlanRepository : Repository<SubscriptionPlan>, ISubscriptionPlanRepository
{
    public SubscriptionPlanRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetActivePlansAsync()
    {
        // SQLite doesn't support ordering by decimal, so convert to double first
        var plans = await _dbSet
            .Where(p => p.IsActive)
            .ToListAsync();
        
        return plans.OrderBy(p => (double)p.Price);
    }

    public async Task<SubscriptionPlan?> GetByStripeProductIdAsync(string stripeProductId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.StripeProductId == stripeProductId);
    }
}
