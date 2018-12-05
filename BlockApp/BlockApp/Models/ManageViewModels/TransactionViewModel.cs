using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BlockApp.Enum;

namespace BlockApp.Models.ManageViewModels
{
    public class TransactionViewModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }

        [DisplayName("Price")]
        [DisplayFormat(DataFormatString = "{0:0.00000000}", ApplyFormatInEditMode = true)]
        public decimal Amount { get; set; }
        
        public string Description { get; set; }
        public StatusTransaction Status { get; set; }
        public string Hash { get; set; }
        
        [Required]
        [RegularExpression(@"^(?=.{111}$)xpub[a-zA-Z0-9]*",
            ErrorMessage = "Please enter valid electum master public key.")]
        [Display(Name = "xPub key", Prompt = "Enter your xPub key")]
        public string XPubKey { get; set; }
        
        [Display(Name = "Unconfirmed deposit script", Prompt = "Unconfirmed deposit script")]
        public string UnconfirmedDepositTx { get; set; }
        
        [Display(Name = "Partially confirmed deposit script", Prompt = "Enter your partially confirmed deposit script")]
        public string BuyerConfirmedDepositTx { get; set; }
        
        [Display(Name = "Unconfirmed withdraw script", Prompt = "Unconfirmed withdraw script")]
        public string UnconfirmedWithdrawTx { get; set; }
        
        [Display(Name = "Partially confirmed withdraw script", Prompt = "Enter your partially confirmed withdraw script")]
        public string BuyerConfirmedWithdrawTx { get; set; }

        [Display(Name = "Fee address", Prompt = "Fee address")]
        public string FeeAddress { get; set; }
        
        [Required]
        [RegularExpression(@"^(?=.{42}$)0x[a-zA-Z0-9]*",
            ErrorMessage = "Please enter valid ethereum address.")]
        [Display(Name = "Address", Prompt = "Enter your ethereum address")]
        public string BuyerAddress { get; set; }
        
        [DisplayName("Fee value")]
        public string FeeValue { get; set; }
        
        public string RedeemScript { get; set; }

        [Display(Prompt = "Transaction hash id")]
        public string TrxDepositId { get; set; }

        [Display(Prompt = "Transaction hash id")]
        public string TrxWithdrawId { get; set; }

        public WalletType WalletType { get; set; }
        
        [Display(Name = "Contract address", Prompt = "Smart contract address")]
        public string ContractAddress { get; set; }
        
        [Display(Name = "ABI", Prompt = "Application binary interface")]
        public string ABI { get; set; }
        
        [DisplayName("Balance")]
        public string Balance { get; set; }
        
        [Required]
        [EmailAddress]
        [Display(Name = "Email", Prompt = "Enter your email")]
        public string Email { get; set; }
        
        public DateTime Created { get; set; }

        public IEnumerable<LogViewModel> Logs { get; set; }
        
        public string StatusMessage { get; set; }
    }
}