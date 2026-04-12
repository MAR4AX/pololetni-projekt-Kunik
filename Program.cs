using System.Globalization;
using MongoDB.Driver;
using MongoDB.Bson;


var connectionString = "mongodb+srv://marekkunik:Sk1b1d1_H4wkTu4h@pololetniprojekt.wmxdz4s.mongodb.net/?appName=PololetniProjekt";
var client = new MongoClient(connectionString);
var db = client.GetDatabase("PololetniProjekt");

var colUsers      = db.GetCollection<User>("Users");
var colReaders    = db.GetCollection<User>("Readers");
var colEmployees  = db.GetCollection<User>("Employees");
var colBooks      = db.GetCollection<Book>("Books");
var colExemplars  = db.GetCollection<Book>("ExemplarsList");
var colVypujcky   = db.GetCollection<Vypujcka>("Vypujcky");
var colRezervace  = db.GetCollection<Rezervace>("Rezervace");


Library library = new Library("Smíchov");
library.employees     = colEmployees.Find(_ => true).ToList();
library.readers       = colReaders.Find(_ => true).ToList();
library.users         = colUsers.Find(_ => true).ToList();
library.books         = colBooks.Find(_ => true).ToList();
library.exemplarsList = colExemplars.Find(_ => true).ToList();
library.vypujcky      = colVypujcky.Find(_ => true).ToList();
library.rezervace     = colRezervace.Find(_ => true).ToList();

string LogInContact;
string LogInPassword;

while (true)
{
    Console.WriteLine("Zadejte e-mail nebo telefonní číslo");
    LogInContact = Console.ReadLine();
    Console.WriteLine("Zadejte heslo");
    LogInPassword = Console.ReadLine();

    User loggedUser = null;
    foreach (User u in library.users)
    {
        if (LogInContact == u.contact && LogInPassword == u.password)
        {
            loggedUser = u;
            break;
        }
    }

    if (loggedUser == null)
    {
        Console.WriteLine("Nesprávné přihlašovací údaje. Zkuste znovu.");
        continue;
    }

    bool loggedIn = true;
    while (loggedIn)
    {
        Console.Clear();
        Console.WriteLine($"=== Knihovna {library.name} ===");
        Console.WriteLine();
        Console.WriteLine("[M] Můj profil");
        Console.WriteLine("[V] Vyhledat knihu");
        if (loggedUser.isEmployee)
        {
            Console.WriteLine("[C] Čtenáři");
            Console.WriteLine("[K] Knihy");
            Console.WriteLine("[P] Výpůjčky (zaměstnanec)");
            Console.WriteLine("[S] Statistiky a přehledy");
        }
        else
        {
            Console.WriteLine("[P] Moje výpůjčky a rezervace");
        }
        Console.WriteLine("[O] Odhlásit se");

        var key = Console.ReadKey();
        Console.WriteLine();

        switch (key.Key)
        {
            case ConsoleKey.M:
                Console.Clear();
                Console.WriteLine($"Jméno: {loggedUser.name} {loggedUser.surname}");
                Console.WriteLine($"Přihlašovací údaj: {loggedUser.contact}");
                if (loggedUser.isEmployee)
                {
                    Console.WriteLine("Role: ZAMĚSTNANEC");
                }
                else
                {
                    Console.WriteLine($"Číslo čtenářské kartičky: {loggedUser.readerNumber}");
                    decimal dluh = library.SpocitejDluhCtenare(loggedUser);
                    if (dluh > 0)
                        Console.WriteLine($"Dluh za pozdní vrácení: {dluh} CZK");
                    else
                        Console.WriteLine("Žádný dluh.");
                    
                    Console.WriteLine($"pocet aktivnivh vypujcek: {library.vypujcky.FindAll(v => v.ctenar == loggedUser && !v.vraceno).Count()}");
                    Console.WriteLine($"pocet aktivnivh rezervaci: {library.rezervace.FindAll(r => r.ctenar == loggedUser && !r.splnena).Count()}");
                }
                Console.WriteLine();
                Console.WriteLine("Zmáčkněte libovolnou klávesu...");
                Console.ReadKey();
                break;

            case ConsoleKey.V:
                Console.Clear();
                Console.WriteLine("Zadejte údaj knihy (název, autor, rok vydání, ISBN, žánr):");
                string search = Console.ReadLine();
                bool found = false;

                List<Book> nalezeneKnihy = new List<Book>();
                foreach (Book book in library.books)
                {
                    if (search == book.name || search == book.author || search == $"{book.year}" || search == book.ISBN || search == book.genre)
                    {
                        int volne = library.PocetVolnychExemplaru(book.ISBN);
                        Console.WriteLine($"Název: {book.name}, Autor: {book.author}, Rok: {book.year}, ISBN: {book.ISBN}, Žánr: {book.genre}, Exemplářů celkem: {book.exemplars}, Volných: {volne}");
                        nalezeneKnihy.Add(book);
                        found = true;
                    }
                }

                if (!found)
                {
                    Console.WriteLine("Kniha nenalezena.");
                    Console.WriteLine();
                    Console.WriteLine("Zmáčkněte libovolnou klávesu...");
                    Console.ReadKey();
                    break;
                }

                if (!loggedUser.isEmployee)
                {
                    Console.WriteLine();
                    Console.WriteLine("[Y] Vypůjčit / rezervovat knihu   [N] Zpět");
                    if (Console.ReadKey().Key == ConsoleKey.Y)
                    {
                        Console.WriteLine();

                        string zvoleneISBN;
                        if (nalezeneKnihy.Count == 1)
                            zvoleneISBN = nalezeneKnihy[0].ISBN;
                        else
                        {
                            Console.Write("Zadejte ISBN knihy, kterou chcete: ");
                            zvoleneISBN = Console.ReadLine();
                        }

                        Book vybrana = nalezeneKnihy.Find(b => b.ISBN == zvoleneISBN);
                        if (vybrana == null)
                        {
                            Console.WriteLine("Zadané ISBN neodpovídá žádné nalezené knize.");
                            Console.ReadKey();
                            break;
                        }

                        Book volnyEx = library.NajdiVolnyExemplar(zvoleneISBN);
                        if (volnyEx != null)
                        {
                            Vypujcka novaVyp = new Vypujcka(loggedUser, volnyEx, DateTime.Today, DateTime.Today.AddDays(30));
                            library.vypujcky.Add(novaVyp);
                            colVypujcky.InsertOne(novaVyp); 

                            volnyEx.isLoaned = true;
                            colExemplars.UpdateOne(
                                Builders<Book>.Filter.Eq(e => e.Id, volnyEx.Id),
                                Builders<Book>.Update.Set(e => e.isLoaned, true));

                            Console.WriteLine($"Výpůjčka zaregistrována: {volnyEx.name} [{volnyEx.serial_number}]");
                            Console.WriteLine($"Vrátit do: {novaVyp.datumVraceni:d}");
                            Console.WriteLine("Knihu si vyzvedněte na přepážce.");
                        }
                        else
                        {
                            int aktualniPoradi = library.rezervace.FindAll(r => r.ISBN == zvoleneISBN && !r.splnena).Count + 1;
                            Console.WriteLine($"Žádný exemplář není volný. Vaše pořadí ve frontě by bylo: {aktualniPoradi}.");
                            Console.WriteLine("Chcete se zařadit do fronty rezervací? [Y/N]");
                            if (Console.ReadKey().Key == ConsoleKey.Y)
                            {
                                Rezervace novaRez = library.VytvorRezervaci(loggedUser, zvoleneISBN);
                                colRezervace.InsertOne(novaRez); 
                                Console.WriteLine($"Rezervace vytvořena. Vaše pořadí: {aktualniPoradi}.");
                                Console.WriteLine("Budete upozorněni, jakmile bude exemplář k dispozici.");
                            }
                        }
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Zmáčkněte libovolnou klávesu...");
                Console.ReadKey();
                break;

            case ConsoleKey.C:
                if (!loggedUser.isEmployee)
                {
                    break;
                }
                else
                Console.Clear();
                Console.WriteLine("[V] Vyhledat čtenáře   [N] Nový čtenář   [Z] Smazat čtenáře");
                var klavesa = Console.ReadKey();
                Console.WriteLine();
                switch (klavesa.Key)
                {
                    case ConsoleKey.V:
                        Console.Clear();
                        Console.WriteLine("Zadejte údaj čtenáře:");
                        string Usearch = Console.ReadLine();
                        foreach (User reader in library.readers)
                        {
                            if (Usearch == reader.name || Usearch == reader.surname ||
                                Usearch == $"{reader.readerNumber}" || Usearch == reader.contact)
                            {
                                decimal dluh = library.SpocitejDluhCtenare(reader);
                                Console.WriteLine($"Jméno: {reader.name} {reader.surname}, " +
                                    $"Kartička: {reader.readerNumber}, Kontakt: {reader.contact}, Dluh: {dluh} CZK");
                            }
                        }
                        Console.WriteLine();
                        Console.WriteLine("Zmáčkněte libovolnou klávesu...");
                        Console.ReadKey();
                        break;

                    case ConsoleKey.N:
                        Console.Clear();
                        Console.Write("Jméno: ");
                        string iName = Console.ReadLine();
                        Console.Write("Příjmení: ");
                        string iSurname = Console.ReadLine();
                        Console.Write("Kontakt: ");
                        string iContact = Console.ReadLine();
                        Console.Write("Heslo: ");
                        string iPassword = Console.ReadLine();
                        User novyCtenar = library.NewReader(iName, iSurname, false, 0, iContact, iPassword);
                        colReaders.InsertOne(novyCtenar);
                        colUsers.InsertOne(novyCtenar);
                        Console.WriteLine("Čtenář přidán.");
                        Console.ReadKey();
                        break;

                    case ConsoleKey.Z:
                        Console.Write("Číslo kartičky čtenáře ke smazání: ");
                        int delNum = Convert.ToInt32(Console.ReadLine());
                        User toDelete = library.readers.Find(r => r.readerNumber == delNum);
                        if (toDelete != null)
                        {
                            Console.WriteLine($"{toDelete.name} {toDelete.surname} – opravdu smazat? [Y/N]");
                            if (Console.ReadKey().Key == ConsoleKey.Y)
                            {
                                library.readers.Remove(toDelete);
                                library.users.Remove(toDelete);
                                colReaders.DeleteOne(Builders<User>.Filter.Eq(u => u.Id, toDelete.Id));
                                colUsers.DeleteOne(Builders<User>.Filter.Eq(u => u.Id, toDelete.Id));
                                Console.WriteLine("Smazáno.");
                            }
                        }
                        else Console.WriteLine("Čtenář nenalezen.");
                        Console.ReadKey();
                        break;
                }
                break;

            case ConsoleKey.K:
                if (!loggedUser.isEmployee) break;
                Console.Clear();
                Console.WriteLine("[P] Přidat knihu   [O] Odebrat knihu");
                var bookChoice = Console.ReadKey();
                Console.WriteLine();
                switch (bookChoice.Key)
                {
                    case ConsoleKey.P:
                        Console.Clear();
                        Console.Write("Název: ");
                        string bName = Console.ReadLine();
                        Console.Write("Autor: ");
                        string bAuthor = Console.ReadLine();
                        Console.Write("ISBN: ");
                        string bISBN = Console.ReadLine();
                        Console.Write("Rok vydání: ");
                        int bYear = Convert.ToInt32(Console.ReadLine());
                        Console.Write("Žánr: ");
                        string bGenre = Console.ReadLine();
                        Console.Write("Počet exemplářů: ");
                        int bEx = Convert.ToInt32(Console.ReadLine());
                        var (novaKniha, noveExemplare) = library.NewBook(bName, bAuthor, bISBN, bYear, bGenre, bEx, "0");
                        colBooks.InsertOne(novaKniha);          
                        colExemplars.InsertMany(noveExemplare); 
                        Console.WriteLine("Kniha přidána.");
                        Console.ReadKey();
                        break;

                    case ConsoleKey.O:
                        Console.Write("ISBN knihy k odebrání: "); string remISBN = Console.ReadLine();
                        Book toRemove = library.books.Find(b => b.ISBN == remISBN);
                        if (toRemove != null)
                        {
                            Console.WriteLine($"{toRemove.name} – opravdu odebrat? [Y/N]");
                            if (Console.ReadKey().Key == ConsoleKey.Y)
                            {
                                library.books.Remove(toRemove);
                                library.exemplarsList.RemoveAll(e => e.ISBN == remISBN);
                                colBooks.DeleteOne(Builders<Book>.Filter.Eq(b => b.Id, toRemove.Id));
                                colExemplars.DeleteMany(Builders<Book>.Filter.Eq(b => b.ISBN, remISBN));
                            }
                        }
                        else Console.WriteLine("Kniha nenalezena.");
                        Console.ReadKey();
                        break;
                }
                break;

            case ConsoleKey.P:
                if (loggedUser.isEmployee)
                    ZamestnanecVypujckyMenu(library, loggedUser, colVypujcky, colExemplars, colRezervace);
                else
                    CtenarVypujckyMenu(library, loggedUser, colVypujcky, colRezervace);
                break;

            case ConsoleKey.S:
                if (!loggedUser.isEmployee)
                    break;
                else
                    Console.WriteLine($"pocet aktivních vypujcek: {library.vypujcky.FindAll(v => v.vraceno != true)}");
                    Console.WriteLine($"pocet vypujcek po terminu: {library.vypujcky.FindAll(v => v.vraceno != true && DateTime.Today > v.datumVraceni)}");
                break;

            case ConsoleKey.O:
                Console.Clear();
                loggedIn = false;
                break;
        }
    }
}


static void ZamestnanecVypujckyMenu(
    Library library, User zamestnanec,
    IMongoCollection<Vypujcka> colVypujcky,
    IMongoCollection<Book> colExemplars,
    IMongoCollection<Rezervace> colRezervace)
{
    Console.Clear();
    Console.WriteLine("[N] Nová výpůjčka   [R] Vrátit knihu   [E] Prodloužit výpůjčku");
    Console.WriteLine("[Z] Rezervace       [S] Sankce / dluhy čtenáře");
    var volba = Console.ReadKey();
    Console.WriteLine();

    switch (volba.Key)
    {
        case ConsoleKey.N:
            Console.Clear();
            Console.Write("Číslo čtenářské kartičky: ");
            int karticky = Convert.ToInt32(Console.ReadLine());
            User ctenar = library.readers.Find(r => r.readerNumber == karticky);
            if (ctenar == null) { Console.WriteLine("Čtenář nenalezen."); Console.ReadKey(); break; }

            Console.Write("ISBN knihy k vypůjčení: ");
            string vypISBN = Console.ReadLine();

            Book volnyExemp = library.NajdiVolnyExemplar(vypISBN);
            if (volnyExemp == null)
            {
                Console.WriteLine("Žádný volný exemplář. Nabídnout rezervaci? [Y/N]");
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    Rezervace novaRez = library.VytvorRezervaci(ctenar, vypISBN);
                    colRezervace.InsertOne(novaRez); 
                    Console.WriteLine("Rezervace vytvořena – čtenář bude zařazen do fronty.");
                }
                Console.ReadKey();
                break;
            }

            Vypujcka nova = new Vypujcka(ctenar, volnyExemp, DateTime.Today, DateTime.Today.AddDays(30));
            library.vypujcky.Add(nova);
            colVypujcky.InsertOne(nova); 

            volnyExemp.isLoaned = true;
            colExemplars.UpdateOne(
                Builders<Book>.Filter.Eq(e => e.Id, volnyExemp.Id),
                Builders<Book>.Update.Set(e => e.isLoaned, true));

            Console.WriteLine($"Výpůjčka vytvořena: {volnyExemp.name} [{volnyExemp.serial_number}]");
            Console.WriteLine($"Čtenář: {ctenar.name} {ctenar.surname}");
            Console.WriteLine($"Datum výpůjčky: {nova.datumVypujcky:d}  |  Vrátit do: {nova.datumVraceni:d}");
            Console.ReadKey();
            break;

        case ConsoleKey.R:
            Console.Clear();
            Console.Write("Sériové číslo exempláře: ");
            string serial = Console.ReadLine();

            Vypujcka vracena = library.vypujcky.Find(v => v.exemplar.serial_number == serial && !v.vraceno);
            if (vracena == null) { Console.WriteLine("Aktivní výpůjčka nenalezena."); Console.ReadKey(); break; }

            vracena.datumSkutecnehoVraceni = DateTime.Today;
            vracena.vraceno = true;
            vracena.exemplar.isLoaned = false;

            if (DateTime.Today > vracena.datumVraceni)
            {
                int dnyPoTerminu = (DateTime.Today - vracena.datumVraceni).Days;
                decimal pokuta = dnyPoTerminu * 10m;
                vracena.pokuta = pokuta;
                vracena.pokutaZaplacena = false;
                Console.WriteLine($"Kniha vrácena POZDĚ ({dnyPoTerminu} dní). Pokuta: {pokuta} CZK");
            }
            else
            {
                Console.WriteLine("Kniha vrácena včas. Žádná pokuta.");
            }

            colVypujcky.ReplaceOne(
                Builders<Vypujcka>.Filter.Eq(v => v.Id, vracena.Id),
                vracena);

            colExemplars.UpdateOne(
                Builders<Book>.Filter.Eq(e => e.Id, vracena.exemplar.Id),
                Builders<Book>.Update.Set(e => e.isLoaned, false));

            library.AktualizujRezervacePoDostupcnosti(vracena.exemplar.ISBN, colRezervace);
            Console.ReadKey();
            break;

        case ConsoleKey.E:
            Console.Clear();
            Console.Write("Sériové číslo exempláře k prodloužení: ");
            string serialProd = Console.ReadLine();

            Vypujcka kProdlouzeni = library.vypujcky.Find(v => v.exemplar.serial_number == serialProd && !v.vraceno);
            if (kProdlouzeni == null) { Console.WriteLine("Aktivní výpůjčka nenalezena."); Console.ReadKey(); break; }

            if (kProdlouzeni.pocetProdlouzeni >= 2)
            {
                Console.WriteLine("Výpůjčku nelze dále prodloužit – byl dosažen limit 2 prodloužení.");
                Console.ReadKey();
                break;
            }

            bool maRezervaci = library.rezervace.Exists(r => r.ISBN == kProdlouzeni.exemplar.ISBN && !r.splnena);
            if (maRezervaci)
            {
                Console.WriteLine("Výpůjčku nelze prodloužit – na tuto knihu čeká jiný čtenář (rezervace).");
                Console.ReadKey();
                break;
            }

            kProdlouzeni.datumVraceni = kProdlouzeni.datumVraceni.AddDays(30);
            kProdlouzeni.pocetProdlouzeni++;

            colVypujcky.ReplaceOne(
                Builders<Vypujcka>.Filter.Eq(v => v.Id, kProdlouzeni.Id),
                kProdlouzeni);

            Console.WriteLine($"Výpůjčka prodloužena. Nový termín vrácení: {kProdlouzeni.datumVraceni:d}");
            Console.WriteLine($"Zbývající prodloužení: {2 - kProdlouzeni.pocetProdlouzeni}");
            Console.ReadKey();
            break;

        case ConsoleKey.Z:
            Console.Clear();
            Console.WriteLine("=== Aktuální rezervace ===");
            if (library.rezervace.Count == 0)
            {
                Console.WriteLine("Žádné rezervace.");
            }
            else
            {
                foreach (Rezervace rez in library.rezervace)
                {
                    if (!rez.splnena)
                        Console.WriteLine($"ISBN: {rez.ISBN} | Čtenář: {rez.ctenar.name} {rez.ctenar.surname} " +
                            $"| Pořadí: {rez.poradí} | Stav: {rez.Stav}");
                }
            }
            Console.ReadKey();
            break;

        case ConsoleKey.S:
            Console.Clear();
            Console.Write("Číslo kartičky čtenáře: ");
            int sankceKarticky = Convert.ToInt32(Console.ReadLine());
            User sankCtenar = library.readers.Find(r => r.readerNumber == sankceKarticky);
            if (sankCtenar == null)
            {
                Console.WriteLine("Čtenář nenalezen.");
                Console.ReadKey();
                break;
            }

            var dluhy = library.vypujcky.FindAll(v => v.ctenar.Id == sankCtenar.Id && v.pokuta > 0 && !v.pokutaZaplacena);
            if (dluhy.Count == 0)
            {
                Console.WriteLine("Čtenář nemá žádné nesplacené dluhy.");
            }
            else
            {
                decimal celkem = 0;
                foreach (Vypujcka v in dluhy)
                {
                    Console.WriteLine($"Kniha: {v.exemplar.name} | Pokuta: {v.pokuta} CZK | Vráceno: {(v.vraceno ? v.datumSkutecnehoVraceni?.ToString("d") : "dosud ne")}");
                    celkem += v.pokuta;
                }
                Console.WriteLine($"Celkem dluh: {celkem} CZK");
                Console.WriteLine("Označit vše jako zaplaceno? [Y/N]");
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    dluhy.ForEach(v =>
                    {
                        v.pokutaZaplacena = true;
                        colVypujcky.ReplaceOne(
                            Builders<Vypujcka>.Filter.Eq(x => x.Id, v.Id),
                            v);
                    });
                    Console.WriteLine("Dluh označen jako zaplacený.");
                }
            }
            Console.ReadKey();
            break;
    }
}

static void CtenarVypujckyMenu(
    Library library, User ctenar,
    IMongoCollection<Vypujcka> colVypujcky,
    IMongoCollection<Rezervace> colRezervace)
{
    Console.Clear();
    Console.WriteLine("[M] Moje výpůjčky   [P] Požádat o prodloužení   [R] Moje rezervace   [D] Moje dluhy");
    var volba = Console.ReadKey();
    Console.WriteLine();

    switch (volba.Key)
    {
        case ConsoleKey.M:
            Console.Clear();
            Console.WriteLine("=== Moje výpůjčky ===");
            var mojVyp = library.vypujcky.FindAll(v => v.ctenar.Id == ctenar.Id && !v.vraceno);
            if (mojVyp.Count == 0) Console.WriteLine("Žádné aktivní výpůjčky.");
            foreach (Vypujcka v in mojVyp)
            {
                bool poTerminu = DateTime.Today > v.datumVraceni;
                Console.WriteLine($"{v.exemplar.name} [{v.exemplar.serial_number}] | Vrátit do: {v.datumVraceni:d} | Prodlouženo: {v.pocetProdlouzeni}× {(poTerminu ? " PO TERMÍNU" : "")}");
            }
            Console.WriteLine("[H]Historie výpůjček");
            var volbaHistorie = Console.ReadKey();
            if(volbaHistorie.Key == ConsoleKey.H)
            {
                foreach(Vypujcka vypujcka in library.vypujcky)
                {
                    bool poTerminu = DateTime.Today > vypujcka.datumVraceni;
                    Console.WriteLine($"{vypujcka.exemplar.name} [{vypujcka.exemplar.serial_number}] {(vypujcka.vraceno ? $"vraceno {vypujcka.datumSkutecnehoVraceni}" : "Stale vypujceno")} Prodlouženo: {vypujcka.pocetProdlouzeni}  {(poTerminu ? " PO TERMÍNU" : "")}");
                }
            }
            Console.ReadKey();
            break;

        case ConsoleKey.P:
            Console.Clear();
            Console.Write("Sériové číslo exempláře: ");
            string s = Console.ReadLine();
            Vypujcka vyp = library.vypujcky.Find(v => v.exemplar.serial_number == s && v.ctenar.Id == ctenar.Id && !v.vraceno);
            if (vyp == null)
            {
                Console.WriteLine("Výpůjčka nenalezena.");
                Console.ReadKey();
                break;
            }
            if (vyp.pocetProdlouzeni >= 2)
            {
                Console.WriteLine("Limit prodloužení dosažen.");
                Console.ReadKey();
                break;
            }
            if (library.rezervace.Exists(r => r.ISBN == vyp.exemplar.ISBN && !r.splnena))
            {
                Console.WriteLine("Nelze prodloužit – na tuto knihu čeká jiný čtenář.");
                Console.ReadKey(); break;
            }
            vyp.datumVraceni = vyp.datumVraceni.AddDays(30);
            vyp.pocetProdlouzeni++;

            colVypujcky.ReplaceOne(
                Builders<Vypujcka>.Filter.Eq(v => v.Id, vyp.Id),
                vyp);

            Console.WriteLine($"Prodlouženo. Nový termín: {vyp.datumVraceni:d}");
            Console.ReadKey();
            break;

        case ConsoleKey.R:
            Console.Clear();
            Console.WriteLine("=== Moje rezervace ===");
            var mojRez = library.rezervace.FindAll(r => r.ctenar.Id == ctenar.Id && !r.splnena);
            if (mojRez.Count == 0)
                Console.WriteLine("Žádné aktivní rezervace.");
            foreach (Rezervace r in mojRez)
                Console.WriteLine($"ISBN: {r.ISBN} | Pořadí ve frontě: {r.poradí} | Stav: {r.Stav}");
            Console.ReadKey();
            break;

        case ConsoleKey.D:
            Console.Clear();
            decimal dluh = library.SpocitejDluhCtenare(ctenar);
            Console.WriteLine($"Váš aktuální dluh: {dluh} CZK");
            Console.ReadKey();
            break;
    }
}


class Vypujcka
{
    public ObjectId Id { get; set; }
    public User ctenar;
    public Book exemplar;
    public DateTime datumVypujcky;
    public DateTime datumVraceni;
    public DateTime? datumSkutecnehoVraceni;
    public bool vraceno = false;
    public int pocetProdlouzeni = 0;
    public decimal pokuta = 0;
    public bool pokutaZaplacena = false;

    public Vypujcka(User ctenar, Book exemplar, DateTime datumVypujcky, DateTime datumVraceni)
    {
        this.ctenar = ctenar;
        this.exemplar = exemplar;
        this.datumVypujcky = datumVypujcky;
        this.datumVraceni = datumVraceni;
    }
}

class Rezervace
{
    public ObjectId Id { get; set; }
    public User ctenar;
    public string ISBN;
    public int poradí;
    public bool splnena;
    public string pripravena = "Čeká se na vrácení exempláře";
    public string Stav => pripravena;

    public Rezervace(User ctenar, string ISBN, int poradi)
    {
        this.ctenar = ctenar;
        this.ISBN = ISBN;
        this.poradí = poradi;
        this.splnena = false;
    }
}

class Library
{
    public string name;
    public List<Book> books;
    public List<Book> exemplarsList;
    public List<User> readers;
    public List<User> employees;
    public List<User> users;
    public List<Vypujcka> vypujcky;
    public List<Rezervace> rezervace;

    public Library(string name)
    {
        this.name = name;
        books = new List<Book>();
        exemplarsList = new List<Book>();
        readers = new List<User>();
        employees = new List<User>();
        users = new List<User>();
        vypujcky = new List<Vypujcka>();
        rezervace = new List<Rezervace>();
    }

    public (Book, List<Book>) NewBook(string name, string author, string ISBN, int year, string genre, int exemplars, string serial_number)
    {
        Book book = new Book(name, author, ISBN, year, genre, exemplars, "0");
        books.Add(book);
        List<Book> noveExemplare = new List<Book>();
        for (int i = 0; i < book.exemplars; i++)
        {
            Book exemplar = new Book(name, author, ISBN, year, genre, 1, ISBN + i);
            exemplarsList.Add(exemplar);
            noveExemplare.Add(exemplar);
        }
        return (book, noveExemplare);
    }

    public User NewReader(string name, string surname, bool isEmployee, int readerNumber, string contact, string password)
    {
        User user = new User(name, surname, false, readers.Count + 1, contact, password);
        readers.Add(user);
        users.Add(user);
        return user;
    }

    public void NewEmployee(string name, string surname, bool isEmployee, int readerNumber, string contact, string password)
    {
        User user = new User(name, surname, true, 0, contact, password);
        employees.Add(user);
        users.Add(user);
    }

    public Book NajdiVolnyExemplar(string ISBN)
    {
        return exemplarsList.Find(e => e.ISBN == ISBN && !e.isLoaned);
    }

    public int PocetVolnychExemplaru(string ISBN)
    {
        return exemplarsList.FindAll(e => e.ISBN == ISBN && !e.isLoaned).Count;
    }

    public Rezervace VytvorRezervaci(User ctenar, string ISBN)
    {
        int poradi = rezervace.FindAll(r => r.ISBN == ISBN && !r.splnena).Count + 1;
        Rezervace rez = new Rezervace(ctenar, ISBN, poradi);
        rezervace.Add(rez);
        return rez;
    }

    public void AktualizujRezervacePoDostupcnosti(string ISBN, IMongoCollection<Rezervace> colRezervace)
    {
        Rezervace prvni = rezervace
            .FindAll(r => r.ISBN == ISBN && !r.splnena)
            .OrderBy(r => r.poradí)
            .FirstOrDefault();

        if (prvni != null)
        {
            prvni.pripravena = "Připraveno k vyzvednutí";
            colRezervace.ReplaceOne(
                Builders<Rezervace>.Filter.Eq(r => r.Id, prvni.Id),
                prvni);
            Console.WriteLine($"Upozornění: Čtenář {prvni.ctenar.name} {prvni.ctenar.surname} má rezervaci na tuto knihu – exemplář je nyní připraven k vyzvednutí.");
        }
    }

    public decimal SpocitejDluhCtenare(User ctenar)
    {
        decimal celkem = 0;
        foreach (Vypujcka v in vypujcky)
        {
            if (v.ctenar.Id != ctenar.Id) continue;
            if (!v.pokutaZaplacena)
            {
                if (v.vraceno && v.pokuta > 0)
                    celkem += v.pokuta;
                else if (!v.vraceno && DateTime.Today > v.datumVraceni)
                {
                    int dny = (DateTime.Today - v.datumVraceni).Days;
                    celkem += dny * 10m;
                }
            }
        }
        return celkem;
    }
}

class User
{
    public ObjectId Id { get; set; }
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
    public ObjectId Id { get; set; }
    public string name;
    public string author;
    public string ISBN;
    public int year;
    public string genre;
    public int exemplars;
    public string serial_number;
    public bool isLoaned = false;

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
