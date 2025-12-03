using FitnessApp.Domain.Entities;
using FitnessApp.Domain.Interfaces;
using FitnessApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Infrastructure.Repositories;

public class ClassBookingRepository : Repository<ClassBooking>, IClassBookingRepository
{
    public ClassBookingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ClassBooking>> GetBookingsByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(b => b.FitnessClass)
                .ThenInclude(c => c.Instructor)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClassBooking>> GetBookingsByClassIdAsync(int classId)
    {
        return await _dbSet
            .Include(b => b.User)
            .Where(b => b.ClassId == classId)
            .OrderBy(b => b.BookedAt)
            .ToListAsync();
    }

    public async Task<ClassBooking?> GetBookingAsync(string userId, int classId)
    {
        return await _dbSet
            .Include(b => b.FitnessClass)
            .FirstOrDefaultAsync(b => b.UserId == userId && b.ClassId == classId);
    }

    public async Task<bool> HasBookingAsync(string userId, int classId)
    {
        return await _dbSet
            .AnyAsync(b => b.UserId == userId && 
                          b.ClassId == classId && 
                          b.Status != BookingStatus.Cancelled);
    }

    public async Task<int> GetUserBookingCountForMonthAsync(string userId, int month, int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);
        
        return await _dbSet
            .CountAsync(b => b.UserId == userId && 
                            b.BookedAt >= startDate && 
                            b.BookedAt < endDate &&
                            b.Status != BookingStatus.Cancelled);
    }
}
