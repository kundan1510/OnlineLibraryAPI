using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OnlineLibraryCore.Entities;
using System.Text;
using System.Xml;

namespace OnlineLibraryAPI.Controllers
{
    //[Authorize(Roles = "Admin")]
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly TableServiceClient _tableServiceClient;

        public AdminController(BlobServiceClient blobServiceClient, TableServiceClient tableServiceClient)
        {
            _blobServiceClient = blobServiceClient;
            _tableServiceClient = tableServiceClient;
        }
        // Only users with the "Admin" role can access this action
        [HttpPost("add-book")]
        public async Task<IActionResult> AddBookAsync([FromBody] Book book)
        {
            // Your code to add a book
            if (book == null)
            {
                return BadRequest("Book data is required.");
            }

            // Validate the incoming book data
            if (string.IsNullOrEmpty(book.Title) || book.maxCopies <= 0)
            {
                return BadRequest("Invalid book data.");
            }

            try
            {
                // Generate a unique ID for the book
                if (string.IsNullOrEmpty(book.Id))
                {
                    book.Id = Guid.NewGuid().ToString();
                }

                // Initialize a list to hold existing books
                List<Book> existingBooks;

                // Fetch the current list of books from Azure Blob Storage
                var blobClient = _blobServiceClient.GetBlobContainerClient("bookjson").GetBlobClient("books.json.bak");
                if (await blobClient.ExistsAsync())
                {
                    var downloadResponse = await blobClient.DownloadAsync();
                    using (var reader = new StreamReader(downloadResponse.Value.Content))
                    {
                        var json = await reader.ReadToEndAsync();
                        existingBooks = JsonConvert.DeserializeObject<List<Book>>(json) ?? new List<Book>();
                    }
                }
                else
                {
                    existingBooks = new List<Book>();
                }

                // Add the new book to the list
                existingBooks.Add(book);

                // Convert the list back to JSON and upload it to Azure Blob Storage
                var updatedJson = JsonConvert.SerializeObject(existingBooks, Newtonsoft.Json.Formatting.Indented);
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(updatedJson)))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                // Return a success response
                return Ok(new { message = "Book added successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception and return a server error response
                //_logger.LogError(ex, "Error adding book");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding the book.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-book/{id}")]
        public async Task<IActionResult> DeleteBook(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Book ID is required.");
            }

            try
            {
                // Fetch the current list of books from Azure Blob Storage
                var blobClient = _blobServiceClient.GetBlobContainerClient("bookjson").GetBlobClient("books.json.bak");
                List<Book> existingBooks;

                if (await blobClient.ExistsAsync())
                {
                    var downloadResponse = await blobClient.DownloadAsync();
                    using (var reader = new StreamReader(downloadResponse.Value.Content))
                    {
                        var json = await reader.ReadToEndAsync();
                        existingBooks = JsonConvert.DeserializeObject<List<Book>>(json) ?? new List<Book>();
                    }
                }
                else
                {
                    return NotFound("Books data not found.");
                }

                // Find the book to delete
                var bookToDelete = existingBooks.FirstOrDefault(b => b.Id == id);
                if (bookToDelete == null)
                {
                    return NotFound("Book not found.");
                }

                // Check if the book has any active subscriptions
                var hasActiveSubscriptions = await CheckActiveSubscriptions(id);
                if (hasActiveSubscriptions)
                {
                    return BadRequest("Cannot delete the book as it has active subscriptions.");
                }

                // Remove the book from the list
                existingBooks.Remove(bookToDelete);

                // Convert the updated list back to JSON and upload it to Azure Blob Storage
                var updatedJson = JsonConvert.SerializeObject(existingBooks, Newtonsoft.Json.Formatting.Indented);
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(updatedJson)))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                // Return a success response
                return Ok(new { message = "Book deleted successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception and return a server error response
                //_logger.LogError(ex, "Error deleting book");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the book.");
            }
        }



        [HttpPut("update-book")]
        public async Task<IActionResult> UpdateBook([FromBody] Book updatedBook)
        {
            if (string.IsNullOrEmpty(updatedBook.Id))
            {
                return BadRequest("Book ID is required.");
            }

            try
            {
                // Fetch the current list of books from Azure Blob Storage
                var blobClient = _blobServiceClient.GetBlobContainerClient("bookjson").GetBlobClient("books.json.bak");
                List<Book> existingBooks;

                if (await blobClient.ExistsAsync())
                {
                    var downloadResponse = await blobClient.DownloadAsync();
                    using (var reader = new StreamReader(downloadResponse.Value.Content))
                    {
                        var json = await reader.ReadToEndAsync();
                        existingBooks = JsonConvert.DeserializeObject<List<Book>>(json) ?? new List<Book>();
                    }
                }
                else
                {
                    return NotFound("Books data not found.");
                }

                // Find the book to delete
                var bookToDelete = existingBooks.FirstOrDefault(b => b.Id == updatedBook.Id);
                if (bookToDelete == null)
                {
                    return NotFound("Book not found.");
                }

                // Check if the book has any active subscriptions
                //var hasActiveSubscriptions = await CheckActiveSubscriptions(id);
                //if (hasActiveSubscriptions)
                //{
                //    return BadRequest("Cannot delete the book as it has active subscriptions.");
                //}

                // Remove the book from the list
                existingBooks.Remove(bookToDelete);
                existingBooks.Add(updatedBook);

                // Convert the updated list back to JSON and upload it to Azure Blob Storage
                var updatedJson = JsonConvert.SerializeObject(existingBooks, Newtonsoft.Json.Formatting.Indented);
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(updatedJson)))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                // Return a success response
                return Ok(new { message = "Book updated successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception and return a server error response
                //_logger.LogError(ex, "Error deleting book");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the book.");
            }
        }

        private async Task<bool> CheckActiveSubscriptions(string bookId)
        {
            // Assuming you have a TableClient for accessing the subscriptions
            var tableClient = _tableServiceClient.GetTableClient("Subscriptions");

            // Query to check if there are any active subscriptions for the book
            var query = tableClient.QueryAsync<SubscriptionEntity>(filter: $"PartitionKey eq '{bookId}' and IsActive eq true");

            await foreach (var subscription in query)
            {
                // If any active subscription exists, return true
                if (subscription != null)
                {
                    return true;
                }
            }

            return false; // No active subscriptions found
        }


    }
}
