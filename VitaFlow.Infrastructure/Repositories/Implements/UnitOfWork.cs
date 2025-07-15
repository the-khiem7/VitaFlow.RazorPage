using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VitaFlow.Infrastructure.Data;
using VitaFlow.Infrastructure.Repositories.Interfaces;

namespace VitaFlow.Infrastructure.Repositories.Implements
{
    public class UnitOfWork : IUnitOfWork, IGenericRepositoryFactory
    {
        private readonly ApplicationDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> GetRepository<T>() where T : class
        {
            if (!_repositories.ContainsKey(typeof(T)))
            {
                _repositories[typeof(T)] = new GenericRepository<T>(_context);
            }
            return (IGenericRepository<T>)_repositories[typeof(T)];
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
} 