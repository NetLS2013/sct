using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlockApp.Enum;

namespace BlockApp.Data.Entities
{
    public class Subscriptions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Address { get; set; }
        public DateTime Expired { get; set; }
        public WalletType WalletType { get; set; }
        public bool Paid { get; set; }
    }
}