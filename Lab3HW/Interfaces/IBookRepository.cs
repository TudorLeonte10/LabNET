using Lab3.Models;
using Lab3.Helpers;
using System.Collections;

namespace Lab3.Interfaces
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllBooksAsync(QueryParameters queryParameters);
        Task<Book?> GetBookByIdAsync(int id);
        Task AddBookAsync(Book book);
        Task UpdateBookAsync(Book book, int id);
        Task DeleteBookAsync(int id);
    }
}
