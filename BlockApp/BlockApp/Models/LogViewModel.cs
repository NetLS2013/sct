namespace BlockApp.Models
{
    using System;

    public class LogViewModel
    {
        public string Id { get; set; }
        public string TransactionId { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }
}