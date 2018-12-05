namespace BlockApp.Interfaces
{
    using System;
    using System.Threading.Tasks;

    using Nethereum.Hex.HexTypes;

    public interface IEthereumService
    {
        /// <summary>
        /// Deploy smart contract into blockchain.
        /// Use params from user for fill properties of contract.
        /// Waste ether from system wallet.
        /// </summary>
        /// <param name="id">
        /// The id of account with transaction fee.
        /// </param>
        /// <param name="address1">
        /// The address of first user.
        /// </param>
        /// <param name="address2">
        /// The address of second user.
        /// </param>
        /// <param name="amount1">
        /// The amount of ether which first must send to contract.
        /// </param>
        /// <param name="amount2">
        /// The amount of ether which second must send to contract.
        /// </param>
        /// <param name="totalAmount">
        /// The total amount.
        /// </param>
        /// <param name="fee">
        /// The fee for deploy contract.
        /// </param>
        /// <param name="address">
        /// The address owner smart contract.
        /// </param>
        /// <param name="pass">
        /// The pass phrase for open account.
        /// </param>
        /// <returns>
        /// The tuple of contract address and ABI/>.
        /// </returns>
        Task<Tuple<string, string>> DeployContract(
            int id,
            string address1,
            string address2,
            decimal amount1,
            decimal amount2,
            decimal totalAmount,
            HexBigInteger fee,
            string address,
            string pass);

        /// <summary>
        /// Estimate gas size for deploy smart contract.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="address1">
        /// The address of first user.
        /// </param>
        /// <param name="address2">
        /// The address of second user.
        /// </param>
        /// <param name="amount1">
        /// The amount of ether which first must send to contract.
        /// </param>
        /// <param name="amount2">
        /// The amount of ether which second must send to contract.
        /// </param>
        /// <param name="totalAmount">
        /// The total amount.
        /// </param>
        /// <param name="address">
        /// The address owner smart contract.
        /// </param>
        /// <param name="pass">
        /// The pass phrase for open account.
        /// </param>
        /// <returns>
        /// The integer of gas size for deploy contract/>.
        /// </returns>
        Task<HexBigInteger> EstimateGasSize(
            int id,
            string address1,
            string address2,
            decimal amount1,
            decimal amount2,
            decimal totalAmount,
            string address,
            string pass);

        /// <summary>
        /// Create address by id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The tuple of account address and private key.
        /// </returns>
        Task<Tuple<string, string>> CreateAddress(int id);

        /// <summary>
        /// Create address by id and from seed and password.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="words">
        /// The seed words.
        /// </param>
        /// <param name="seedPassword">
        /// The seed password.
        /// </param>
        /// <returns>
        /// The tuple of account address and private key.
        /// </returns>
        Task<Tuple<string, string>> CreateAddress(int id, string words, string seedPassword);

        /// <summary>
        /// Get balance of address from blockchain.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <returns>
        /// The integer wei.
        /// </returns>
        Task<HexBigInteger> GetBalance(string address);

        /// <summary>
        /// Get application binary interface of smart contract.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetABI();
    }
}