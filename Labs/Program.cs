using System;
using System.Collections.Generic;
using System.Linq;

var librarian = new Librarian("Ana Pop", "ana.pop@library.com", "BestSection");
List<Book> books = new();

librarian.AddBook(books);
Console.WriteLine($"\nCarte adăugata: {books[0].Title}, {books[0].Author}, {books[0].YearPublished:d}");


var borrower = new Borrower(1, "John Doe", books);
var updatedBorrower = borrower with
{
    BorrowedBooks = borrower.BorrowedBooks
        .Append(new Book("Narnia", "C.S. Lewis", new DateTime(1870, 1, 1)))
        .ToList()
};


Console.WriteLine("\n=== Pattern Matching ===");
AboutObject(updatedBorrower);
AboutObject(books[0]);
AboutObject("Ceva ca sa dea unknown");


Console.WriteLine("\n=== Carti publicate după 2010 ===");
books.Add(new Book("Atomic Habits", "James Clear", new DateTime(2018, 1, 1)));
books.Add(new Book("The Midnight Library", "Matt Haig", new DateTime(2020, 1, 1)));
var filteredBooks = books.Where(static b => b.YearPublished.Year > 2010).ToList();


foreach (var book in filteredBooks)
{
    Console.WriteLine($" {book.Title}, {book.Author}, {book.YearPublished.Year}");
}

static void AboutObject(object obj)
{
    switch (obj)
    {
        case Book b:
            Console.WriteLine($"Book: {b.Title} ({b.YearPublished.Year})");
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

    public void AddBook(List<Book> books)
    {
        Console.WriteLine("Introdu titlul unei carti:");
        string title = Console.ReadLine() ?? "Titlu necunoscut";

        string author = "Chiar el";

        int year = 2000; 

        var book = new Book(title, author, new DateTime(year, 1, 1));
        books.Add(book);

        Console.WriteLine($"Bibliotecar {Name} ({LibrarySection}) a adăugat: '{book.Title}' de {book.Author} ({year})");
    }
}
