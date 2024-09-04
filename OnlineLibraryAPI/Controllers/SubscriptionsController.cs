using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineLibraryCore.Entities;
using System.Collections.Concurrent;

namespace OnlineLibraryAPI.Controllers
{
    //[Authorize]
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly TableServiceClient _tableServiceClient;

        public SubscriptionsController(TableServiceClient tableServiceClient)
        {
            _tableServiceClient = tableServiceClient;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubscription([FromBody] Subscription subscription)
        {
            // Access the table storage
            var tableClient = _tableServiceClient.GetTableClient("subscriptions");
            await tableClient.CreateIfNotExistsAsync();

            // Add the subscription
            var entity = new TableEntity("SUBSCRIPTION", subscription.UserEmail)
            {
                {"BookId", subscription.BookId},
                {"StartDate", subscription.StartDate},
                {"EndDate", subscription.EndDate},
                {"Cost", subscription.Cost}
            };

            await tableClient.AddEntityAsync(entity);

            return CreatedAtAction(nameof(CreateSubscription), subscription);
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserSubscriptions(string email)
        {
            var tableClient = _tableServiceClient.GetTableClient("subscriptions");

            // Query user's subscriptions
            var partitionKey = "SUBSCRIPTION";
            var query = tableClient.QueryAsync<TableEntity>(filter: $"RowKey eq '{email}' and PartitionKey eq '{partitionKey}'");

            var subscriptions = new List<Subscription>();
            await foreach (var entity in query)
            {
                subscriptions.Add(new Subscription
                {
                    UserEmail = entity.RowKey,
                    BookId = entity.GetString("BookId"),
                    StartDate = entity.GetDateTime("StartDate").Value,
                    EndDate = entity.GetDateTime("EndDate").Value,
                    Cost = entity.GetDouble("Cost").Value
                });
            }

            return Ok(subscriptions);
        }


        [HttpPut]
        public async Task<IActionResult> UpdateSubscription([FromBody] Subscription subscription)
        {
            //await _subscriptionRepository.UpdateSubscriptionAsync(subscription);

            var tableClient = _tableServiceClient.GetTableClient("subscriptions");
            await tableClient.CreateIfNotExistsAsync();

            // Add the subscription
            var entity = new TableEntity("SUBSCRIPTION", subscription.UserEmail)
            {
                {"BookId", subscription.BookId},
                {"StartDate", subscription.StartDate},
                {"EndDate", subscription.EndDate},
                {"Cost", subscription.Cost}
            };

            await tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
            return NoContent();
        }

        [HttpDelete("{email}/{bookid}")]
        public async Task<IActionResult> CancelSubscription(string email, string bookid)
        {
            //await _subscriptionRepository.DeleteSubscriptionAsync(userId, subscriptionId);
            var tableClient = _tableServiceClient.GetTableClient("subscriptions");

            // Query subscriptions with the help of bookid
            var partitionKey = "SUBSCRIPTION";
            var query = tableClient.QueryAsync<TableEntity>(filter: $"RowKey eq '{email}' and PartitionKey eq '{partitionKey}'and BookId eq '{bookid}'");

            var subscriptions = new List<SubscriptionEntity>();
            await foreach (var entity in query)
            {
                subscriptions.Add(new SubscriptionEntity
                {
                    RowKey = entity.RowKey,
                    BookId = entity.GetString("BookId"),
                    StartDate = entity.GetDateTime("StartDate").Value,
                    EndDate = entity.GetDateTime("EndDate").Value,
                    Cost = entity.GetDouble("Cost").Value,
                    PartitionKey = entity.GetString("PartitionKey")
                });
            }

            var subscription = subscriptions.FirstOrDefault();

            await tableClient.DeleteEntityAsync(subscription);
            return NoContent();
        }

    }
}

