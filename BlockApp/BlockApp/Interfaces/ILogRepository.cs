using System.Collections.Generic;
using System.Threading.Tasks;
using BlockApp.Data.Entities;

namespace BlockApp.Interfaces
{
    public interface ILogRepository : IGenericRepository<Log>
    {
        /// <summary>
        /// Add new log to transaction history.
        /// </summary>
        /// <param name="transactionId">Transaction identifier.</param>
        /// <param name="description">Description message.</param>
        Task Add(string transactionId, string description);
        
        /// <summary>
        /// Get transaction log history by transaction.
        /// </summary>
        /// <param name="transactionId">Transaction identifier.</param>
        /// <returns>Returns list of logs.</returns>
        IEnumerable<Log> GetByTrx(string transactionId);
    }
}