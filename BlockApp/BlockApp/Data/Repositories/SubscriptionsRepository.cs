using System;
using System.Threading.Tasks;
using BlockApp.Data.Entities;
using BlockApp.Enum;
using BlockApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlockApp.Data.Repositories
{
    public class SubscriptionsRepository : GenericRepository<Subscriptions>, ISubscriptionsRepository
    {
        public SubscriptionsRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task<Subscriptions> GetByUserId(string userId)
        {
            return await GetAll().LastOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task CreatePaymentAddress(string userId)
        {
            await Create(new Subscriptions
            {
                UserId = userId
            });
        }
        
        public async Task SavePaymentAddress(string address, WalletType walletType, int id)
        {
            var entry = await Get(s => s.Id == id);

            entry.Address = address;
            entry.WalletType = walletType;
            
            await Update(entry);
        }

        public async Task PaidToAddress(DateTime expired, int id)
        {
            var entry = await Get(s => s.Id == id);

            entry.Expired = expired;
            entry.Paid = true;
            
            await Update(entry);
        }
    }
}