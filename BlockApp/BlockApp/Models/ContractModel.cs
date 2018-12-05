using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockApp.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ContractModel
    {
        public string Hash { get; set; }

        [RegularExpression(@"^(?=.{42}$)0x[a-zA-Z0-9]*",
            ErrorMessage = "Please enter valid ethereum address.")]
        [Required]
        public string Address { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }
    }
}
