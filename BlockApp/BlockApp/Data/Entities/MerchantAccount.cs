using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlockApp.Models;

namespace BlockApp.Data.Entities
{
    public class MerchantAccount
    {
        [Key, ForeignKey("UserOf")]
        public string Id { get; set; }
        public string MerchantId { get; set; }
        public string MerchantSecret { get; set; }
        public string XPubKey { get; set; }
        public string EthereumAddress { get; set; }
        public string RedirectUri { get; set; }
        
        public virtual ApplicationUser UserOf { get; set; }
    }
}