namespace BlockApp.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Log
    {
        [Key]
        public string Id { get; set; }
        public string TransactionId { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }
}