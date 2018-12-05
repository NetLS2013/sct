using NBitcoin;

namespace BlockApp.Interfaces
{
    using System;

    public interface IBitcoinService
    {
        /// <summary>
        /// Create multi signature address using extended public keys.
        /// </summary>
        /// <param name="pubkey1">
        /// The pubkey of first user.
        /// </param>
        /// <param name="pubkey2">
        /// The pubkey of second user.
        /// </param>
        /// <returns>
        /// The string redeem script.
        /// </returns>
        string CreateMultiSigAddress(string pubkey1, string pubkey2);

        /// <summary>
        /// Create outgoing transaction from multisig address in coinb. 
        /// Return transaction hex string and redeem script string. 
        /// </summary>
        /// <param name="redeemScript">
        /// The redeem script. String format, NOT hex.
        /// </param>
        /// <param name="amount1">
        /// The first amount .
        /// </param>
        /// <param name="amount2">
        /// The second amount.
        /// </param>
        /// <returns>
        /// The tuple of transaction hex and redeem script/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Can be throw when params invalid, amount less zero, redeem script is invalid
        /// </exception>
        Tuple<string, string> CreateTransaction(string redeemScript, decimal amount1, decimal amount2);

        /// <summary>
        /// Create transaction for electrum wallet. 
        /// </summary>
        /// <param name="xpubkey1">
        /// The xpubkey the first wallet.
        /// </param>
        /// <param name="xpubkey2">
        /// The xpubkey the second wallet.
        /// </param>
        /// <param name="amount1">
        /// The amount for first address.
        /// </param>
        /// <param name="amount2">
        /// The amount for second address.
        /// </param>
        /// <returns>
        /// The tuple of transaction hex and redeem script/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        /// <exception cref="Exception">
        /// </exception>
        Tuple<string, string> CreateTransactionElectrum(
            string xpubkey1,
            string xpubkey2,
            decimal amount1,
            decimal amount2);

        /// <summary>
        /// Create outgoing transaction from multisig address in electrum. 
        /// </summary>
        /// <param name="redeemScript">
        /// The redeem script. String format, NOT hex.
        /// </param>
        /// <param name="xpubkey1">
        /// The xpubkey the first wallet.
        /// </param>
        /// <param name="xpubkey2">
        /// The xpubkey the secound wallet.
        /// </param>
        /// <param name="amount1">
        /// The amount for first address.
        /// </param>
        /// <param name="amount2">
        /// The amount for secound address.
        /// </param>
        /// <returns>
        /// The string of transaction code/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Can be throw when params invalid, amount less zero
        /// </exception>
        /// <exception cref="Exception">
        /// Cannot get fee from blockchain, no node connection
        /// </exception>
        string CreateOutTransactionElectrum(
            string redeemScript,
            string xpubkey1,
            string xpubkey2,
            decimal amount1,
            decimal amount2);

        /// <summary>
        /// Check is deposit transaction exist and confirmed six times.
        /// </summary>
        /// <param name="trx">
        /// The transaction hex.
        /// </param>
        /// <param name="redeem">
        /// The redeem script.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool CheckInTransaction(string trx, string redeem);

        /// <summary>
        /// Check is withdraw transaction exist and confirmed six times.
        /// </summary>
        /// <param name="trx">
        /// The transaction hex.
        /// </param>
        /// <param name="redeem">
        /// The redeem script.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool CheckOutTransaction(string trx, string redeem);

        /// <summary>
        /// Get address from xpubkey by id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="extPubKey">
        /// Extended public key.
        /// </param>
        /// <returns>
        /// The string of address />.
        /// </returns>
        string GenerateAddress(int id, string extPubKey);

        /// <summary>
        /// Check is in wallet exists enough bitcoins.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <param name="amount">
        /// The amount.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool CheckEqualsBalance(string address, decimal amount);
    }
}