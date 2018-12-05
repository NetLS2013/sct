namespace BlockApp.Helpers
{
    using System.Numerics;

    /// <summary>
    /// The currency converter.
    /// Wei is a 10^18 part of ethereum, and smart contract pass only uint values, so we need convert ether to wei
    /// </summary>
    public class CurrencyConverter
    {
        /// <summary>
        /// Convert wei integer to ether decimal.
        /// </summary>
        /// <param name="amount">
        /// The amount.
        /// </param>
        /// <returns>
        /// The <see cref="decimal"/>.
        /// </returns>
        public static decimal WeiToEther(BigInteger amount)
        {
            return ((decimal)amount) / 1000000000000000000;
        }

        /// <summary>
        /// Convert ether decimal to wei integer. 
        /// </summary>
        /// <param name="amount">
        /// The amount.
        /// </param>
        /// <returns>
        /// The <see cref="BigInteger"/>.
        /// </returns>
        public static BigInteger EtherToWei(decimal amount)
        {
            return new BigInteger(amount * 1000000000000000000);
        }
    }
}