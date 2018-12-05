using System;
using System.Threading.Tasks;
using BlockApp.Data.Entities;
using BlockApp.Enum;
using BlockApp.Helpers;
using BlockApp.Interfaces;
using BlockApp.Models;
using BlockApp.Models.ManageViewModels;

namespace BlockApp.Data.Repositories
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<Transaction> GetByHash(string hash)
        {
            return await Get(m => m.Hash == hash);
        }
        
        public async Task<string> CreateTransactions(ShopInfoModel shopInfoModel, string userId, WalletType walletType)
        {
            var hash = Guid.NewGuid().ToString().Replace("-", "");
            
            await Create(new Transaction
            {
                Amount = shopInfoModel.Amount,
                Description = shopInfoModel.Description,
                Status = StatusTransaction.Created,
                UserId = userId,
                Hash = hash,
                WalletType = walletType,
                EtherId = new Random().Next(0, int.MaxValue)
            });

            return hash;
        }
        
        public async Task SavePartiallyDepositScript(string hash, string script)
        {
            var entry = await Get(m => m.Hash == hash);

            entry.BuyerConfirmedDepositTx = script;
            entry.Status = StatusTransaction.PartiallyDeposit;
            
            await Update(entry);
        }
        
        public async Task SavePartiallyWithdrawScript(string hash, string script)
        {
            var entry = await Get(m => m.Hash == hash);

            entry.BuyerConfirmedWithdrawTx = script;
            entry.Status = StatusTransaction.PartiallyWithdraw;
            
            await Update(entry);
        }
    }
}