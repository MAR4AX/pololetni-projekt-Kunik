Library library = new Library("knihovna");
library.NewEmployee("Marek", "Kuník", true, 0, "kunik.ma.2024@skola.ssps.cz", "heslo123");

string LogInContact;
string LogInPassword;
while (true)
{
    Console.WriteLine("Zadejte e-mail nebo telefonní číslo");
    LogInContact = Console.ReadLine();
    Console.WriteLine("Zadejte heslo");
    LogInPassword = Console.ReadLine();

    foreach (User user in library.users)
    {
       if(LogInContact == user.contact &&  LogInPassword == user.password)
        {
            Console.WriteLine();
        }
    }
}

class Library
{
    public string name;

    public List<Book> books;
    public List<User> readers;
    public List<User> employees;
    public List<User> users;

    public Library(string name)
    {
        this.name = name;

        books = new List<Book>();
        readers = new List<User>();
        employees = new List<User>();
        users = new List<User>();
    }
    public void NewBook(string name, string author, int ISBN, int year, string genre, int exemplars)
    {
        Book book = new Book(name, author, ISBN, year, genre, exemplars);
        books.Add(book);
    }

    public void NewReader(string name, string surname, bool isEmployee, int readerNumber, string contact, string password)
    {
        User user = new User(name, surname, isEmployee=false, readerNumber, contact, password);  
        readers.Add(user);
        users.Add(user);
    }

    public void NewEmployee(string name, string surname, bool isEmployee, int readerNumber, string contact, string password)
    {
        User user = new User(name, surname, isEmployee=true, readerNumber=0, contact, password);
        employees.Add(user);
        users.Add(user);
    }
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