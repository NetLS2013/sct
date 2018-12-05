using System.ComponentModel.DataAnnotations;

namespace BlockApp.Models
{
    public class MultisigModel
    {
        public string Hash { get; set; }

        [Required]
        [RegularExpression(@"^(?=.{111}$)xpub[a-zA-Z0-9]*",
            ErrorMessage = "Please enter valid electum master public key.")]
        public string XPubKey { get; set; }

        public string PartiallySignedScript { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }
    }
}