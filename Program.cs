using System.Reflection.Metadata;
using System.Xml.Linq;
User admin = new User("Admin", "Admin", true, 0, "none", "admin");
admin.NewEmployee("Marek", "Kunik", true, 0, "kunik.ma.2024@skola.ssps.cz", "heslo123");

while (true)
{

}
class User
{
    public string name;
    public string surname;
    public bool isEmployee;
    public int readerNumber;
    public string contact;
    public string password;

    public User(string name, string surname, bool isEmployee, int readerNumber, string contact, string password)
    {
        this.name = name;
        this.surname = surname;
        this.isEmployee = isEmployee;
        this.readerNumber = readerNumber;
        this.contact = contact;
        this.password = password;

        books = new List<Book>();
        readers = new List<User>();
        employees = new List<User>();
    }

    public List<Book> books;
    public List<User> readers;
    public List<User> employees;

    public void NewBook(string name, string author, int ISBN, int year, string genre, int exemplars)
    {
        Book book = new Book(name, author, ISBN, year, genre, exemplars);
        books.Add(book);
    }

    public void NewReader(string name, string surname, bool isEmployee, int readerNumber, string contact, string password)
    {
        User reader = new User(name, surname, isEmployee=false, readerNumber, contact, password);  
        readers.Add(reader);
    }

    public void NewEmployee(string name, string surname, bool isEmployee, int readerNumber, string contact, string password)
    {
        User employee = new User(name, surname, isEmployee=true, readerNumber, contact, password);
        employees.Add(employee);
    }
}

class Book
{
    public string name;
    public string author;
    public int ISBN;
    public int year;
    public string genre;
    public int exemplars;

    public Book(string name, string author, int iSBN, int year, string genre, int exemplars)
    {
        this.name = name;
        this.author = author;
        ISBN = iSBN;
        this.year = year;
        this.genre = genre;
        this.exemplars = exemplars;
    }

}