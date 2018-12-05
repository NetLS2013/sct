using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BlockApp.Enum;

namespace BlockApp.Models.ManageViewModels
{
    public class MerchantViewModel
    {
        [DisplayName("Merchant Id")]
        public string MerchantId { get; set; }
        
        [Required]
        [DisplayName("Merchant Secret")]
        public string MerchantSecret { get; set; }

        [RegularExpression(@"^(?=.{111}$)xpub[a-zA-Z0-9]*",
            ErrorMessage = "Please enter valid electum master public key.")]
        [Display(Name = "xPub key", Prompt = "Enter your xPub key")]
        public string XPubKey { get; set; }

        [RegularExpression(@"^(?=.{42}$)0x[a-zA-Z0-9]*",
            ErrorMessage = "Please enter valid ethereum address.")]
        [Display(Name = "Address", Prompt = "Enter your address")]
        public string EthereumAddress { get; set; }
        
        public string StatusMessage { get; set; }
        
        [DisplayName("Status:")]
        public bool Status { get; set; }
        
        [Display(Prompt = "Generate wallet address")]
        public string Address { get; set; }
        
        [DisplayName("Expired:")]
        public DateTime Expired { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:0.00000000}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }
        
        [DisplayName("Wallet")]
        public WalletType WalletType { get; set; }

        [Url]
        [Display(Name = "Valid redirect URI", Prompt = "example http://domain.com/get/order")]
        public string RedirectUri { get; set; }
    }
}