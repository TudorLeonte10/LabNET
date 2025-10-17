using Lab3.Exceptions; 
using Lab3.Helpers;
using Lab3.Interfaces;
using Lab3.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lab3.Controllers
{
    [ApiController]
    [Route("api/book")]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;

        public BooksController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBooks([FromQuery] QueryParameters queryParameters)
        {
            var books = await _bookRepository.GetAllBooksAsync(queryParameters);
            if (books == null || !books.Any())
                throw new NotFoundException("No books found matching the query.");

            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            if (id <= 0)
                throw new ValidationException("Invalid book ID.");

            var book = await _bookRepository.GetBookByIdAsync(id);
            if (book == null)
                throw new NotFoundException($"Book with ID {id} not found.");

            return Ok(book);
        }

        [HttpPost]
        public async Task<IActionResult> AddBook([FromBody] Book book)
        {
            if (book == null)
                throw new ValidationException("Book data is missing.");

            if (string.IsNullOrWhiteSpace(book.Title))
                throw new ValidationException("Book title is required.");

            if (string.IsNullOrWhiteSpace(book.Author))
                throw new ValidationException("Book author is required.");

            if (book.Year <= 0)
                throw new ValidationException("Book year must be a positive value.");

            await _bookRepository.AddBookAsync(book);
            return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book book)
        {
            if (id <= 0)
                throw new ValidationException("Invalid book ID.");

            var existingBook = await _bookRepository.GetBookByIdAsync(id);
            if (existingBook == null)
                throw new NotFoundException($"Book with ID {id} not found.");

            if (string.IsNullOrWhiteSpace(book.Title))
                throw new ValidationException("Book title is required.");

            if (string.IsNullOrWhiteSpace(book.Author))
                throw new ValidationException("Book author is required.");

            await _bookRepository.UpdateBookAsync(book, id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            if (id <= 0)
                throw new ValidationException("Invalid book ID.");

            var existingBook = await _bookRepository.GetBookByIdAsync(id);
            if (existingBook == null)
                throw new NotFoundException($"Book with ID {id} not found.");

            await _bookRepository.DeleteBookAsync(id);
            return NoContent();
        }
    }
}
