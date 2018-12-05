using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlockApp.Enum;

namespace BlockApp.Data.Entities
{
    public class Transaction
    {
        [Key]
        public string Id { get; set; }
        public string UserId { get; set; }
        [Column(TypeName = "decimal(18, 8)")]
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public StatusTransaction Status { get; set; }
        public string Hash { get; set; }
        public string XPubKey { get; set; }
        public string UnconfirmedDepositTx { get; set; }
        public string BuyerConfirmedDepositTx { get; set; }
        public string UnconfirmedWithdrawTx { get; set; }
        public string BuyerConfirmedWithdrawTx { get; set; }

        public string RedeemScript { get; set; }
        public string TrxDepositId { get; set; }
        public string TrxWithdrawId { get; set; }

        public string Address { get; set; }
        public string PassPhrase { get; set; }
        [Column(TypeName = "decimal(18, 8)")]
        public decimal Fee { get; set; }

        public string ContractAddress { get; set; }

        public string Email { get; set; }
        public WalletType WalletType { get; set; }

        public int EtherId { get; set; }
    }
}