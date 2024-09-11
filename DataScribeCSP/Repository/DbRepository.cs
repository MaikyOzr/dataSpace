using DataScribeCSP.Data;
using DataScribeCSP.Models;
using Microsoft.EntityFrameworkCore;

namespace DataScribeCSP.Repository
{
    public class DbRepository : IDbRepository
    {
        private readonly AppDbContext _context;
        private bool disposed = false;

        public DbRepository()
        {
            _context = new AppDbContext();
        }

        public DbRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return users;
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public void Update(User user)
        {
            _context.Update(user).State = EntityState.Modified;
        }

        public async Task Delete(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null) { _context.Remove(user); }
        }

        public async Task<User> GetById(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return user;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }


    public interface IDbRepository
    {
        Task<IEnumerable<User>> GetUsers();
        Task<User> GetById(int userId);
        Task AddAsync(User user);
        void Update(User user);
        Task Delete(int userId);
        Task SaveAsync();

    }
}
