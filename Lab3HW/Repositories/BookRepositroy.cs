using Lab3.Data;
using Lab3.Interfaces;
using Lab3.Models;
using System.Collections;
using Lab3.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Lab3.Repositories
{
    public class BookRepositroy : IBookRepository
    {
        private readonly AppDbContext _context;

        public BookRepositroy(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddBookAsync(Book book)
        {
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBookAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            _context.Books.Remove(book!);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync(QueryParameters parameters)
        {
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(parameters.Author))
                query = query.Where(b=> b.Author.Contains(parameters.Author));

            query = parameters.SortBy?.ToLower() switch
            {
                "title" => parameters.SortDirection == "desc"
                            ? query.OrderByDescending(b => b.Title)
                            : query.OrderBy(b => b.Title),
                "year" => parameters.SortDirection == "desc"
                            ? query.OrderByDescending(b => b.Year)
                            : query.OrderBy(b => b.Year),
                _ => query.OrderBy(b => b.Id)
            };

            return await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task UpdateBookAsync(Book book, int id)
        {
            var bookToUpdate = await _context.Books.FindAsync(id);
            if (bookToUpdate != null)
            {
                bookToUpdate.Title = book.Title;
                bookToUpdate.Author = book.Author;
                bookToUpdate.Year = book.Year;
                await _context.SaveChangesAsync();
            }
        }
    }
}
