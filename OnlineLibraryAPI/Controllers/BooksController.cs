using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineLibraryCore.Entities;
using System.Text.Json;

namespace OnlineLibraryAPI.Controllers
{
    //[Authorize]
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BooksController(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            // Access the private container and retrieve the books.json file
            var containerClient = _blobServiceClient.GetBlobContainerClient("bookjson");
            var blobClient = containerClient.GetBlobClient("books.json.bak");

            // Download the content
            var response = await blobClient.DownloadContentAsync();
            var booksJson = response.Value.Content.ToString();

            // Deserialize JSON into a list of Book objects
            var books = JsonSerializer.Deserialize<List<Book>>(booksJson, options);

            //var books = GetAllBooks();

            return Ok(books);
        }

        [HttpGet("{bookId}")]
        public async Task<IActionResult> GetBookById(string bookId)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            // Access the private container and retrieve the books.json file
            var containerClient = _blobServiceClient.GetBlobContainerClient("bookjson");
            var blobClient = containerClient.GetBlobClient("books.json.bak");

            // Download the content
            var response = await blobClient.DownloadContentAsync();
            var booksJson = response.Value.Content.ToString();

            // Deserialize JSON into a list of Book objects
            var books = JsonSerializer.Deserialize<List<Book>>(booksJson, options);
            var book = books?.Find(x => x.Id == bookId);

            return Ok(book);
        }
    }
}

