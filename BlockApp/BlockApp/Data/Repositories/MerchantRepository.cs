using System;
using System.Threading.Tasks;
using BlockApp.Data.Entities;
using BlockApp.Helpers;
using BlockApp.Interfaces;
using BlockApp.Models;
using BlockApp.Models.ManageViewModels;

namespace BlockApp.Data.Repositories
{
    public interface IMerchantRepository : IGenericRepository<MerchantAccount>
    {
        Task<MerchantAccount> GetByUser(string userId);
        Task<bool> ValidateMerchant(string merchantId, string merchantSecret);
        Task CreateMerchant(string userId);
        Task SaveMerchant(string merchantSecret, MerchantViewModel xPupKey, string userId);
        Task<MerchantAccount> GetByMerchant(string merchantId);
    }
    
    public class MerchantRepository : GenericRepository<MerchantAccount>, IMerchantRepository
    {
        public MerchantRepository(ApplicationDbContext context) : base(context)
        {
            
        }

        public async Task<MerchantAccount> GetByUser(string userId)
        {
            return await Get(m => m.Id == userId);
        }

        public async Task<MerchantAccount> GetByMerchant(string merchantId)
        {
            return await Get(m => m.MerchantId == merchantId);
        }

        public async Task<bool> ValidateMerchant(string merchantId, string merchantSecret)
        {
            var merchantAccount = await Get(m => m.MerchantId == merchantId);

            if (merchantAccount == null)
                return false;

            return Crypto.GetSha256Hash(merchantAccount.MerchantSecret) == merchantSecret;
        }
        
        public async Task CreateMerchant(string userId)
        {
            await Create(new MerchantAccount
            {
                Id = userId,
                MerchantId = Guid.NewGuid().ToString().Replace("-", ""),
                MerchantSecret = Guid.NewGuid().ToString().Replace("-", "")
            });
        }

        public async Task SaveMerchant(string merchantSecret, MerchantViewModel merchantModel, string userId)
        {
            var entry = await Get(m => m.Id == userId);

            if (Crypto.GetSha256Hash(entry.MerchantSecret) != merchantSecret)
            {
                entry.MerchantSecret = merchantSecret;
            }

            entry.XPubKey = merchantModel.XPubKey;
            entry.EthereumAddress = merchantModel.EthereumAddress;
            entry.RedirectUri = merchantModel.RedirectUri?.Trim();
            
            await Update(entry);
        }
    }
}