using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BlockApp.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>Returns IQueryable object.</returns>
        IQueryable<TEntity> GetAll();
 
        /// <summary>
        /// Get the entitity matching the given predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>Returns entity.</returns>
        Task<TEntity> Get(Expression<Func<TEntity, bool>> predicate);
 
        /// <summary>
        /// Create the entity.
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        Task Create(TEntity entity);
 
        /// <summary>
        /// Update the entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        Task Update(TEntity entity);
 
        /// <summary>
        /// Delete the given entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        Task Delete(TEntity entity);
    }
}