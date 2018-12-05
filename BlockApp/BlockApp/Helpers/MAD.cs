namespace BlockApp.Helpers
{
    using System;

    public class MAD
    {
        /// <summary>
        /// Coefficient enlarge of payment.
        /// </summary>
        private const decimal Trust = 2m;

        /// <summary>
        /// Calculate deposit size.
        /// </summary>
        /// <param name="amount">
        /// The amount.
        /// </param>
        /// <returns>
        /// The tuple of seller and buyer deposits>.
        /// </returns>
        public static Tuple<decimal, decimal> Deposit(decimal amount)
        {
            return new Tuple<decimal, decimal>(Trust * amount, Trust * amount);
        }

        /// <summary>
        /// Calculate withdraw size.
        /// </summary>
        /// <param name="amount">
        /// The amount.
        /// </param>
        /// <returns>
        /// The The tuple of seller and buyer withdraws>.
        /// </returns>
        public static Tuple<decimal, decimal> Withdraw(decimal amount)
        {
            return new Tuple<decimal, decimal>(Trust * amount + amount, Trust * amount - amount);
        }
    }
}