using System;
using System.Threading.Tasks;
using BlockApp.Data.Entities;
using BlockApp.Enum;

namespace BlockApp.Interfaces
{
    public interface ISubscriptionsRepository : IGenericRepository<Subscriptions>
    {
        /// <summary>
        /// Get subscription data by user identifier.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <returns>Returns subscription object.</returns>
        Task<Subscriptions> GetByUserId(string userId);
        
        /// <summary>
        /// Create an empty entry with user identifier for init unique increment id to access the wallet address.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        Task CreatePaymentAddress(string userId);
        
        /// <summary>
        /// Update subscription row.
        /// </summary>
        /// <param name="address">Wallet address.</param>
        /// <param name="walletType">Wallet type.</param>
        /// <param name="id">Subscriptions identifier.</param>
        Task SavePaymentAddress(string address, WalletType walletType, int id);
        
        /// <summary>
        /// Update the expiration date and subscription status as paid.
        /// </summary>
        /// <param name="expired">Date expires.</param>
        /// <param name="id">User identifier.</param>
        Task PaidToAddress(DateTime expired, int id);
    }
}