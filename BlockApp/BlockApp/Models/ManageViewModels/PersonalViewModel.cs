using System.ComponentModel.DataAnnotations;

namespace BlockApp.Models.ManageViewModels
{
    public class PersonalViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email", Prompt = "Enter your email")]
        public string Email { get; set; }

        [RegularExpression(@"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$",
            ErrorMessage = "Please enter valid phone number.")]
        [Display(Name = "Phone number", Prompt = "Enter your phone number")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(24, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(24, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string LastName { get; set; }
    }
}