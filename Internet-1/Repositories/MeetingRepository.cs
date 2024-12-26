// Repositories/MeetingRepository.cs
using Internet_1.Models;
using Microsoft.EntityFrameworkCore;

namespace Internet_1.Repositories
{
    public class MeetingRepository
    {
        private readonly AppDbContext _context;

        public MeetingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Meeting>> GetAllAsync()
        {
            return await _context.Meetings.Where(x => x.IsActive).OrderByDescending(x => x.Created).ToListAsync();
        }

        public async Task<Meeting> GetByIdAsync(int id)
        {
            return await _context.Meetings.FindAsync(id);
        }

        public async Task AddAsync(Meeting meeting)
        {
            await _context.Meetings.AddAsync(meeting);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Meeting meeting)
        {
            _context.Meetings.Update(meeting);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var meeting = await GetByIdAsync(id);
            if (meeting != null)
            {
                meeting.IsActive = false;
                await UpdateAsync(meeting);
            }
        }
    }
}