using Azure;
using Azure.Data.Tables;

namespace OnlineLibraryCore.Entities
{
    public class SubscriptionEntity : ITableEntity
    {
        public string PartitionKey { get; set; } // ""SUBSCRIPTION""
        public string RowKey { get; set; } // Email
        public string BookId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Cost { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
