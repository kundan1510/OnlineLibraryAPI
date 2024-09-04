using Azure;
using Azure.Data.Tables;


namespace OnlineLibraryCore.Entities.User
{
    public class UserEntity : ITableEntity
    {
        public string PartitionKey { get; set; } // Use a constant value like "USER" or the user's role.
        public string RowKey { get; set; } // Use a unique identifier like the email address.
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? TimeStamp { get; set; }
    }
}
