using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlockApp.Data.Entities;
using BlockApp.Interfaces;

namespace BlockApp.Data.Repositories
{
    public class LogRepository : GenericRepository<Log>, ILogRepository
    {
        public LogRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task Add(string transactionId, string description)
        {
            await this.Create(new Log { Date = DateTime.Now, Description = description, TransactionId = transactionId });
        }

        public IEnumerable<Log> GetByTrx(string transactionId)
        {
            return this.GetAll()
                .Where(log => log.TransactionId == transactionId)
                .AsEnumerable()
                .OrderByDescending(o => o.Date);
        }
    }
}