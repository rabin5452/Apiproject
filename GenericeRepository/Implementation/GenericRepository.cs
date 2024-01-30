using Microsoft.EntityFrameworkCore;
using Practiseproject.Data;
using Practiseproject.GenericeRepository.Interface;

namespace Practiseproject.GenericeRepository.Implementation
{
    public class GenericRepository<T> :IGenericRepository<T> where T : class
    {
        private readonly ProjectDBContext _dbContext;
        private DbSet<T> _dbSet;
        public GenericRepository(ProjectDBContext dBContext)
        {
            _dbContext = dBContext;
            _dbSet = _dbContext.Set<T>();
        }
        public List<T> GetAll()
        {
            return _dbSet.AsNoTracking().ToList();
        }

        public T GetById(int id)
        {
            return _dbSet.Find(id);
        }

        public T Add(T entity)
        {
            _dbSet.Add(entity);
            _dbContext.SaveChanges();
            return entity;
        }

        public T Update(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified; // Mark the entity as modified
            _dbContext.SaveChanges();
            return entity;
        }

        public void Delete(int id)
        {
            T entityToDelete = _dbSet.Find(id);
            if (entityToDelete != null)
            {
                _dbSet.Remove(entityToDelete);
                _dbContext.SaveChanges();
            }
        }
        public IQueryable<T> GetDatas()
        {

            return _dbSet.AsNoTracking().AsQueryable();
        }


    }
}
