using System.ComponentModel.DataAnnotations;
using BlockApp.Enum;
using Microsoft.AspNetCore.Mvc;

namespace BlockApp.Models
{
    public class TransactionModel
    {
        /// <summary>
        /// Unique transaction identifier of the payment.
        /// </summary>
        [Required]
        [FromQuery(Name = "trx_id")]
        public string TrxId { get; set; }
        
        
        /// <summary>
        /// Merchant ID, registered in Safecryptotrades.
        /// </summary>
        [Required]
        [FromQuery(Name = "merchant_id")]
        public string MerchantId { get; set; }
        
        /// <summary>
        /// Merchant Secret, registered in Safecryptotrades.
        /// </summary>
        [Required]
        [FromQuery(Name = "merchant_secret")]
        public string MerchantSecret { get; set; }
        
        /// <summary>
        /// As a developer, you're responsible for responding to this request and returning an appropriate value.
        /// Below is an example of the data issued to your callback URL.
        /// <remarks>
        /// Response body:
        ///
        ///     POST /api/payment/TestShop
        ///     {
        ///        "code": "1",
        ///        "amount": 0.002,
        ///        "description": "desc"
        ///     }
        ///
        /// </remarks>
        /// </summary>
        [Required]
        [FromQuery(Name = "callback_url")]
        public string CallbackUrl { get; set; }
        
        /// <summary>
        /// Wallet type. For the moment we support electrum(1) and ethereum(2).
        /// </summary>
        [Required]
        [FromQuery(Name = "wallet_type")]
        public WalletType WalletType { get; set; }
    }
}