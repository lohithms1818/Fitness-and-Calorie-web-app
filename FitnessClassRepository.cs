using FitnessApp.Domain.Entities;
using FitnessApp.Domain.Interfaces;
using FitnessApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Infrastructure.Repositories;

public class FitnessClassRepository : Repository<FitnessClass>, IFitnessClassRepository
{
    public FitnessClassRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<FitnessClass?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Instructor)
            .Include(c => c.MinimumPlan)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<FitnessClass>> GetUpcomingLiveClassesAsync(int count = 10)
    {
        return await _dbSet
            .Include(c => c.Instructor)
            .Where(c => c.IsLive && c.ScheduledAt > DateTime.UtcNow)
            .OrderBy(c => c.ScheduledAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<FitnessClass>> GetRecordedClassesAsync(int page = 1, int pageSize = 20)
    {
        return await _dbSet
            .Include(c => c.Instructor)
            .Where(c => c.ClassType == ClassType.Recorded && !string.IsNullOrEmpty(c.VideoUrl))
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<FitnessClass>> GetClassesByInstructorAsync(string instructorId)
    {
        return await _dbSet
            .Where(c => c.InstructorId == instructorId)
            .OrderByDescending(c => c.ScheduledAt ?? c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FitnessClass>> GetClassesByCategoryAsync(ClassCategory category)
    {
        return await _dbSet
            .Include(c => c.Instructor)
            .Where(c => c.Category == category)
            .OrderByDescending(c => c.ScheduledAt ?? c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FitnessClass>> SearchClassesAsync(string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return await _dbSet
            .Include(c => c.Instructor)
            .Where(c => c.Title.ToLower().Contains(lowerSearchTerm) || 
                        c.Description.ToLower().Contains(lowerSearchTerm))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetBookingCountAsync(int classId)
    {
        return await _context.ClassBookings
            .CountAsync(b => b.ClassId == classId && 
                            b.Status != BookingStatus.Cancelled);
    }
}
