using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;

namespace FRPAMSystem.DataTier.Repository.Implement
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ForestryResourcePlanningDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(ForestryResourcePlanningDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            var entityType = typeof(TEntity);

            if (_repositories.TryGetValue(entityType, out var repository))
            {
                return (IGenericRepository<TEntity>)repository;
            }

            var newRepository = new GenericRepository<TEntity>(_context);
            _repositories.Add(entityType, newRepository);

            return newRepository;
        }

        public int Commit()
        {
            return _context.SaveChanges();
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}