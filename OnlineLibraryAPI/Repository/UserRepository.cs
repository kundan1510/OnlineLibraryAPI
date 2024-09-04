using Azure;
using Azure.Data.Tables;
using OnlineLibraryCore.Entities.User;

namespace OnlineLibraryAPI.Repository
{
    public class UserRepository
    {
        private readonly TableClient _tableClient;

        public UserRepository(string storageConnectionString, string tableName)
        {
            // Create a TableServiceClient object which will be used to get the TableClient
            var serviceClient = new TableServiceClient(storageConnectionString);

            // Get a reference to the table (create it if it doesn't exist)
            _tableClient = serviceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        // Retrieve a user by PartitionKey ("USER") and RowKey (email)
        public async Task<UserEntity> GetUserAsync(string email)
        {
            try
            {
                return await _tableClient.GetEntityAsync<UserEntity>("USER", email);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null; // User not found
            }
        }

        // Create a new user
        public async Task CreateUserAsync(UserEntity user)
        {
            await _tableClient.AddEntityAsync(user);
        }

        // Update an existing user
        public async Task UpdateUserAsync(UserEntity user)
        {
            await _tableClient.UpdateEntityAsync(user, user.ETag, TableUpdateMode.Replace);
        }

        // Delete an existing user
        public async Task DeleteUserAsync(string email)
        {
            await _tableClient.DeleteEntityAsync("USER", email);
        }
    }
}
