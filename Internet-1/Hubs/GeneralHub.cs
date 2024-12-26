using Internet_1.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore; 

namespace Internet_1.Hubs
{
    public class GeneralHub : Hub
    {
        private readonly AppDbContext _context;

        public GeneralHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task UpdateFileCount()
        {
            var count = await _context.SystemFiles.CountAsync();
            await Clients.All.SendAsync("onFileCountUpdate", count);
        }
    }
}
