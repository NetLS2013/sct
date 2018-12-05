namespace BlockApp.Helpers
{
    using System.Text.RegularExpressions;

    public class Validator
    {
        public static bool ValidateBitcoinTransactionId(string id)
        {
            var regex = new Regex(@"^(?=.{64}$)[a-zA-Z0-9]*");
            return regex.IsMatch(id);
        }
    }
}