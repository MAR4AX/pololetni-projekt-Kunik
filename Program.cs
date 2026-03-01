using System.Globalization;
using System.Reflection.PortableExecutable;

Library library = new Library("Smíchov");
library.NewEmployee("Marek", "Kuník", true, 0, "email@example.com", "heslo123");
library.NewBook("Proměna", "Franz Kafka", "9788072534678", 1915, "novela", 100, "0");
library.NewReader("Josef", "Novak", false, library.readers.Count+1, "pepa.novak@gmail.com", "pepa8");

string LogInContact;
string LogInPassword;
    Console.WriteLine("Zadejte e-mail nebo telefonní číslo");
    LogInContact = Console.ReadLine();
    Console.WriteLine("Zadejte heslo");
    LogInPassword = Console.ReadLine();
while (true)
{

    foreach (User user in library.users)
    {
        if (LogInContact == user.contact && LogInPassword == user.password)
        {
            Console.Clear();
            Console.WriteLine($"knihovna {library.name}");
            Console.WriteLine();
            Console.WriteLine("[M]Můj profil");
            Console.WriteLine("[V]Vyhledat knihu");
            if (user.isEmployee)
            {
                Console.WriteLine("[C]Čtenáři");
                Console.WriteLine("[K]Knihy");
            }
            var key = Console.ReadKey();

            switch (key.Key)
            {
                case(ConsoleKey.M) :
                    Console.Clear();
                    Console.WriteLine($"jméno: {user.name} {user.surname}    přihlšovací údaj: {user.contact}");
                    if (user.isEmployee)
                    {
                        Console.WriteLine("ZAMĚSTNANEC");
                    }
                    else
                    {
                        Console.WriteLine($"číslo čtenářské kartičky: { user.readerNumber}");
                    }
                    Console.WriteLine("zmacknete jakoukoliv klavesu na vraceni...");
                    Console.ReadKey();
                    break;

                case(ConsoleKey.V) :
                    Console.Clear();
                    Console.WriteLine("zadejte údaj knihy (název, autor, rok vydání, ISBN, žánr)");
                    string search = Console.ReadLine();
                    foreach(Book book in library.books)
                    {
                        if(search == book.name || search == book.author || search == $"{book.year}" || search == book.ISBN || search == book.genre)
                        { Console.WriteLine($"jméno: {book.name}, autor: {book.author}, rok vydání: {book.year}, ISBN: {book.ISBN}, žánr: {book.genre}, počet exepmlářů na skladě: {book.exemplars}"); }
                        Console.ReadKey();
                        break ;
                    }
                    break;  

                case(ConsoleKey.C) :
                    if (user.isEmployee)
                    {
                        Console.Clear();
                        Console.WriteLine("[V]Vyhledat ctenare       [N]Novy ctenar       [Z]Zablokovat ctenare");
                        var klavesa = Console.ReadKey();
                        switch (klavesa.Key)
                        {
                            case (ConsoleKey.V) :
                                Console.Clear();
                                Console.WriteLine("zadejte údaj čtenáře");
                                string Usearch = Console.ReadLine();
                                foreach (User reader in library.readers) 
                                {
                                    if(Usearch == reader.name || Usearch == reader.surname ||  Usearch ==$"{reader.readerNumber}" || Usearch == reader.contact)
                                    {
                                        Console.WriteLine($"jmeno: {reader.name}, prijmeni: {reader.surname}, cislo karticky: {reader.readerNumber}, kontakt: {reader.contact}");
                                    }
                                    Console.ReadKey();
                                    break ;
                                }
                                break;

                            case (ConsoleKey.N) :
                                Console.Clear() ;
                                Console.WriteLine("zadejte jmeno noveho uctu:");
                                string inputName = Console.ReadLine();
                                Console.WriteLine("zadejte prijmeni noveho uctu");
                                string inputSurname = Console.ReadLine();
                                Console.WriteLine("zadejte kontakt noveho uctu");
                                string inputContact = Console.ReadLine();
                                Console.WriteLine("zadejte heslo noveho uctu");
                                string inputPassword = Console.ReadLine();
                                library.NewReader(inputName, inputSurname, false, library.readers.Count+1, inputContact, inputPassword);
                                foreach (User reader in library.readers)
                                {
                                    if(inputContact  == reader.contact)
                                    {
                                        Console.WriteLine($"jmeno: {reader.name}, prijmeni: {reader.surname}, cislo karticky: {reader.readerNumber}, kontakt: {reader.contact}");
                                    }
                                    Console.ReadKey();
                                    break;
                                }
                                break;
                            case (ConsoleKey.Z) :
                                Console.WriteLine("zadejte cislo karticky ctenare ktereho chcete smazat");
                                int inputNumber = Convert.ToInt32( Console.ReadLine() );
                                foreach (User reader in library.readers)
                                {
                                    if(inputNumber == reader.readerNumber)
                                    {
                                        Console.WriteLine($"jmeno: {reader.name}, prijmeni: {reader.surname}, cislo karticky: {reader.readerNumber}, kontakt: {reader.contact}");
                                        Console.WriteLine();
                                        Console.WriteLine("opravdu chcete smazat? [Y]/[N]");
                                        var decision = Console.ReadKey();
                                        if(decision.Key == ConsoleKey.Y)
                                        {
                                            library.readers.Remove(reader);
                                        }
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                    break;
                case(ConsoleKey.K) :
                    if(user.isEmployee)
                    {
                        Console.Clear();
                        Console.WriteLine("[P]Pridat knihu       [O]Odebrat knihu");
                       var bookChoice = Console.ReadKey();
                        switch (bookChoice.Key)
                        {
                            case(ConsoleKey.P) :
                                Console.Clear();
                                Console.WriteLine("zadejte jmeno knihy:");
                                string inputJmeno = Console.ReadLine();
                                Console.WriteLine("zadejte autora:");
                                string inputAuthor = Console.ReadLine();
                                Console.WriteLine("zadejte ISBN:");
                                string inputISBN = Console.ReadLine();
                                Console.WriteLine("zadejte rok vydání:");
                                int inputYear = Convert.ToInt32( Console.ReadLine() );
                                Console.WriteLine("zadejte žánr:");
                                string inputGenre = Console.ReadLine();
                                Console.WriteLine("zadejte množství exemplářů:");
                                int inputExemplars = Convert.ToInt32( Console.ReadLine() );
                                library.NewBook(inputJmeno, inputAuthor, inputISBN, inputYear, inputGenre, inputExemplars, "0");
                                foreach(Book book in library.books)
                                {
                                    if(inputISBN == book.ISBN)
                                    {
                                        Console.WriteLine($"jmeno: {book.name}, autor: {book.author}, ISBN: {book.ISBN}, rok vydání: {book.year}, žánr: {book.genre}, exemplářů na skladě: {book.exemplars}");
                                    }
                                }
                                break;

                            case(ConsoleKey.O) :
                                Console.WriteLine("zadejte ISBN knihy:");
                                string inputRemove = Console.ReadLine();
                                foreach(Book book in library.books)
                                    if(inputRemove == book.ISBN)
                                    {
                                        Console.WriteLine($"jmeno: {book.name}, autor: {book.author}, ISBN: {book.ISBN}, rok vydání: {book.year}, žánr: {book.genre}, exemplářů na skladě: {book.exemplars}");
                                        Console.WriteLine();
                                        Console.WriteLine("opravdu chcete smazat? [Y]/[N]");
                                        var bookDecision = Console.ReadKey();
                                        if (bookDecision.Key == ConsoleKey.Y)
                                        {
                                            library.books.Remove(book);
                                        }
                                        break ;
                                    }
                                break;
                        }
                    }
                    break;
            }
        }
    }
}
class Library
{
    public string name;

    public List<Book> books;
    public List<Book> exemplars;
    public List<User> readers;
    public List<User> employees;
    public List<User> users;

    public Library(string name)
    {
        this.name = name;

        books = new List<Book>();
        exemplars = new List<Book>();
        readers = new List<User>();
        employees = new List<User>();
        users = new List<User>();
    }

    public void NewBook(string name, string author, string ISBN, int year, string genre, int exemplars, string serial_number)
    {
        Book book = new Book(name, author, ISBN, year, genre, exemplars, serial_number="0");
        books.Add(book);
        for (int i = 0; i < book.exemplars; i++)  //system pro tvorbu exemplářů, seriové číslo bude později tvořeno jinak
        {
            Book exemplar = new Book(name, author, ISBN, year, genre, exemplars=1, serial_number=ISBN+i);
            books.Add(exemplar);
        }
    }

    public void NewReader(string name, string surname, bool isEmployee, int readerNumber, string contact, string password)
    {
        User user = new User(name, surname, isEmployee = false, readerNumber = readers.Count+1, contact, password); //čtenářské číslo se později bude dělat nejspíš jinak
        readers.Add(user);
        users.Add(user);
    }

    public void NewEmployee(string name, string surname, bool isEmployee, int readerNumber, string contact, string password)
    {
        User user = new User(name, surname, isEmployee = true, readerNumber = 0, contact, password);
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
    public string ISBN;
    public int year;
    public string genre;
    public int exemplars;
    public string serial_number;

    public Book(string name, string author, string ISBN, int year, string genre, int exemplars, string serial_number)
    {
        this.name = name;
        this.author = author;
        this.ISBN = ISBN;
        this.year = year;
        this.genre = genre;
        this.exemplars = exemplars;
        this.serial_number = serial_number;
    }

}