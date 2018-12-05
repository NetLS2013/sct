using System.Threading.Tasks;
using BlockApp.Data.Entities;
using BlockApp.Enum;
using BlockApp.Models;

namespace BlockApp.Interfaces
{
    public interface ITransactionRepository: IGenericRepository<Transaction>
    {
        /// <summary>
        /// Create transaction by concrete user with concrete currency.
        /// </summary>
        /// <param name="trx">
        /// The transaction details.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="walletType">
        /// The currency type.
        /// </param>
        /// <returns>
        /// The string of transaction id.
        /// </returns>
        Task<string> CreateTransactions(ShopInfoModel trx, string userId, WalletType walletType);

        /// <summary>
        /// Get transaction by hash from database.
        /// </summary>
        /// <param name="hash">
        /// The hash.
        /// </param>
        /// <returns>
        /// The transaction details.
        /// </returns>
        Task<Transaction> GetByHash(string hash);

        /// <summary>
        /// Save partially deposit script by transaction hash.
        /// </summary>
        /// <param name="hash">
        /// The hash.
        /// </param>
        /// <param name="script">
        /// The script.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task SavePartiallyDepositScript(string hash, string script);

        /// <summary>
        /// Save partially withdraw script by transaction hash.
        /// </summary>
        /// <param name="hash">
        /// The hash.
        /// </param>
        /// <param name="script">
        /// The script.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task SavePartiallyWithdrawScript(string hash, string script);
    }
}