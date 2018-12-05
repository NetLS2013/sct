namespace BlockApp.Models.ManageViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class SetTransactionId
    {
        [Required]
        public string TransactionId { get; set; }

        [Required]
        [RegularExpression(@"^(?=.{64}$)[a-zA-Z0-9]*",
            ErrorMessage = "Please enter valid transaction id.")]
        public string TransactionHash { get; set; }
    }
}