using System;
using System.Collections.Generic;
using System.Linq;

List<Book> books = new();

Console.WriteLine("Introdu titlul unei carti:");
string title = Console.ReadLine() ?? string.Empty;
books.Add(new Book(title, "Un autor", DateTime.Now));

Console.WriteLine($"Carte adaugata: {books[0].Title}, {books[0].Author}, {books[0].YearPublished}");

var borrower = new Borrower(1, "John Doe", books);
var updatedBorrower = borrower with
{
    BorrowedBooks = borrower.BorrowedBooks
        .Append(new Book("Narnia", "C.S.Lewis", DateTime.Now.AddYears(-150)))
        .ToList()
};

DisplayInfo(updatedBorrower);
DisplayInfo(books[0]);
DisplayInfo("Ceva ca sa zica unknown type");

var filteredBooks = books.Where(static b => b.YearPublished.Year > 2010).ToList();
foreach (var book in filteredBooks)
{
    Console.WriteLine($"Carti filtrate: {book.Title}, {book.Author}, {book.YearPublished}");
}

static void DisplayInfo(object obj)
{
    switch (obj)
    {
        case Book b:
            Console.WriteLine($"Book: {b.Title}, {b.YearPublished}");
            break;
        case Borrower br:
            Console.WriteLine($"Borrower: {br.Name}, Books Count: {br.BorrowedBooks.Count}");
            break;
        default:
            Console.WriteLine("Unknown type");
            break;
    }
}

public record Book(string Title, string Author, DateTime YearPublished);
public record Borrower(int Id, string Name, List<Book> BorrowedBooks);
public class Librarian(string name, string email, string librarySection)
{
    public string Name { get; init; } = name;
    public string Email { get; init; } = email;
    public string LibrarySection { get; init; } = librarySection;
}
