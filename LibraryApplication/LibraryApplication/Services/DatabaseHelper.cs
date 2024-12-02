using SQLite;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LibraryApplication.Services
{
    public static class DatabaseHelper
    {
        private static SQLiteAsyncConnection db;

        // Initialize the database and execute SQL script
        public static async Task Initialize()
        {
            if (db != null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "LibraryApp.db");
            Debug.WriteLine($"Database Path: {dbPath}");

            //await db.ExecuteAsync("PRAGMA foreign_keys = ON;");

            try
            {
                // Initialize SQLite connection
                db = new SQLiteAsyncConnection(dbPath);
                Debug.WriteLine("SQLite connection established.");
                await db.ExecuteAsync("PRAGMA foreign_keys = ON;");

                // Execute the embedded SQL script
                string createTablesScript = GetEmbeddedResource("LibraryApplication.Resources.Database.create_tables.sql");
                await ExecuteSqlScript(createTablesScript);

                Debug.WriteLine("SQL script executed successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during database initialization: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
            }
        }

        // Get the database connection (optional for raw queries)
        public static SQLiteAsyncConnection GetConnection()
        {
            if (db == null)
                throw new InvalidOperationException("Database is not initialized. Call Initialize() first.");
            return db;
        }

        // Execute a raw SQL script
        private static async Task ExecuteSqlScript(string script)
        {
            try
            {
                var commands = script.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var command in commands)
                {
                    var trimmedCommand = command.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedCommand))
                    {
                        try
                        {
                            await db.ExecuteAsync(trimmedCommand);
                            Debug.WriteLine($"Executed SQL Command: {trimmedCommand}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error executing SQL command: {ex.Message}");
                            Debug.WriteLine($"Command: {trimmedCommand}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing SQL script: {ex.Message}");
            }
        }

        // Read embedded SQL script
        private static string GetEmbeddedResource(string resourceName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                    throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading embedded resource: {ex.Message}");
                throw;
            }
        }

        public static async Task SetupDatabaseAsync()
        {
            await Initialize();  
            await SeedData();    
        }

        public static async Task SeedData()
        {
            try
            {
                // Seed Members
                var members = new (int, string, string, string, string)[]
                {
                    (1, "John Doe", "john.doe@example.com", "123-456-7890", "2021-01-01"),
                    (2, "Jane Smith", "jane.smith@example.com", "234-567-8901", "2021-02-15"),
                    (3, "Alice Johnson", "alice.johnson@example.com", "345-678-9012", "2021-03-10"),
                    (4, "Bob Brown", "bob.brown@example.com", "456-789-0123", "2021-04-05"),
                    (5, "Emily Davis", "emily.davis@example.com", "567-890-1234", "2021-05-20"),
                    (6, "Michael Green", "michael.green@example.com", "678-901-2345", "2021-06-15"),
                    (7, "Sarah White", "sarah.white@example.com", "789-012-3456", "2021-07-10"),
                    (8, "David Black", "david.black@example.com", "890-123-4567", "2021-08-05"),
                    (9, "Emma Blue", "emma.blue@example.com", "901-234-5678", "2021-09-15"),
                    (10, "James Gray", "james.gray@example.com", "012-345-6789", "2021-10-01"),
                    (11, "Olivia Gold", "olivia.gold@example.com", "123-456-7890", "2021-11-20"),
                    (12, "Sophia Red", "sophia.red@example.com", "234-567-8901", "2021-12-05"),
                    (13, "Liam Silver", "liam.silver@example.com", "345-678-9012", "2022-01-01"),
                    (14, "Isabella Yellow", "isabella.yellow@example.com", "456-789-0123", "2022-02-15"),
                    (15, "Mason Purple", "mason.purple@example.com", "567-890-1234", "2022-03-10"),
                    (16, "Ava Brown", "ava.brown@example.com", "678-901-2345", "2022-04-05"),
                    (17, "Lucas Pink", "lucas.pink@example.com", "789-012-3456", "2022-05-20"),
                    (18, "Mia Cyan", "mia.cyan@example.com", "890-123-4567", "2022-06-15"),
                    (19, "Ethan Lime", "ethan.lime@example.com", "901-234-5678", "2022-07-10"),
                    (20, "Charlotte Violet", "charlotte.violet@example.com", "012-345-6789", "2022-08-05")
                };
                foreach (var (id, name, email, phone, date) in members)
                {
                    await db.ExecuteAsync("INSERT INTO Members (member_id, name, email, phone, membership_date) VALUES (?, ?, ?, ?, ?)", id, name, email, phone, date);
                }

                // Seed Authors
                var authors = new (int, string, string)[]
                {
                    (1, "J.K. Rowling", "British author, best known for the Harry Potter series."),
                    (2, "J.R.R. Tolkien", "English writer, poet, philologist, author of The Lord of the Rings."),
                    (3, "George Orwell", "English novelist and journalist, author of Animal Farm and 1984."),
                    (4, "Jane Austen", "English novelist, author of Pride and Prejudice."),
                    (5, "Stephen King", "American author of horror, suspense, and fantasy."),
                    (6, "Agatha Christie", "English writer known for detective novels."),
                    (7, "Harper Lee", "American author of To Kill a Mockingbird."),
                    (8, "F. Scott Fitzgerald", "American novelist, author of The Great Gatsby."),
                    (9, "Isaac Asimov", "American writer and professor, author of Foundation."),
                    (10, "Mary Shelley", "English writer, author of Frankenstein."),
                    (11, "Leo Tolstoy", "Russian novelist, author of War and Peace."),
                    (12, "Charles Dickens", "English writer, author of Great Expectations."),
                    (13, "Mark Twain", "American author, writer of Tom Sawyer."),
                    (14, "Ernest Hemingway", "American novelist, author of The Old Man and the Sea."),
                    (15, "Arthur Conan Doyle", "Scottish writer, creator of Sherlock Holmes."),
                    (16, "C.S. Lewis", "Irish writer, author of The Chronicles of Narnia."),
                    (17, "Emily Brontë", "English novelist, author of Wuthering Heights."),
                    (18, "Victor Hugo", "French poet and novelist, author of Les Misérables."),
                    (19, "H.G. Wells", "English writer, father of modern science fiction."),
                    (20, "J.D. Salinger", "American writer, author of The Catcher in the Rye.")
                };
                foreach (var (id, name, bio) in authors)
                {
                    await db.ExecuteAsync("INSERT INTO Authors (author_id, name, bio) VALUES (?, ?, ?)", id, name, bio);
                }

                // Seed Categories
                var categories = new (int, string, string)[]
                        {
                    (1, "Fantasy", "Books with magical or supernatural elements."),
                    (2, "Science Fiction", "Books exploring futuristic and scientific concepts."),
                    (3, "Horror", "Books intended to scare or thrill the reader."),
                    (4, "Mystery", "Books involving solving crimes or uncovering secrets."),
                    (5, "Classics", "Famous and enduring works of literature."),
                    (6, "Romance", "Books focusing on romantic relationships."),
                    (7, "Historical Fiction", "Books set in a past era with fictionalized narratives."),
                    (8, "Adventure", "Books filled with exciting journeys and quests."),
                    (9, "Biography", "Books about real people and their lives."),
                    (10, "Poetry", "Books with collections of poems."),
                    (11, "Thriller", "Books with suspenseful and intense plots."),
                    (12, "Humor", "Books designed to make the reader laugh."),
                    (13, "Drama", "Books with emotionally intense plots."),
                    (14, "Western", "Books set in the American Old West."),
                    (15, "Young Adult", "Books targeted for teenagers."),
                    (16, "Self-Help", "Books to improve one's life."),
                    (17, "Philosophy", "Books that explore human thought."),
                    (18, "Psychology", "Books about human behavior and mind."),
                    (19, "Art", "Books about art and creativity."),
                    (20, "Travel", "Books about exploring new places.")
                };
                foreach (var (id, name, desc) in categories)
                {
                    await db.ExecuteAsync("INSERT INTO Categories (category_id, name, description) VALUES (?, ?, ?)", id, name, desc);
                }

                // Seed Books
                var books = new (int, string, int, int, string, int, int)[]
                {
                    (1, "Harry Potter and the Sorcerer's Stone", 1, 1, "9781234560001", 1997, 5),
                    (2, "Harry Potter and the Chamber of Secrets", 1, 1, "9781234560002", 1998, 4),
                    (3, "The Hobbit", 2, 1, "9781234560003", 1937, 3),
                    (4, "1984", 3, 2, "9781234560004", 1949, 8),
                    (5, "Pride and Prejudice", 4, 6, "9781234560005", 1813, 5),
                    (6, "The Shining", 5, 3, "9781234560006", 1977, 6),
                    (7, "Murder on the Orient Express", 6, 4, "9781234560007", 1934, 7),
                    (8, "To Kill a Mockingbird", 7, 5, "9781234560008", 1960, 4),
                    (9, "The Great Gatsby", 8, 5, "9781234560009", 1925, 5),
                    (10, "Foundation", 9, 2, "9781234560010", 1951, 3),
                    (11, "Frankenstein", 10, 3, "9781234560011", 1818, 2),
                    (12, "War and Peace", 11, 7, "9781234560012", 1869, 6),
                    (13, "Great Expectations", 12, 5, "9781234560013", 1861, 3),
                    (14, "Adventures of Huckleberry Finn", 13, 8, "9781234560014", 1884, 4),
                    (15, "The Old Man and the Sea", 14, 5, "9781234560015", 1952, 7),
                    (16, "The Chronicles of Narnia", 16, 1, "9781234560016", 1956, 8),
                    (17, "Wuthering Heights", 17, 6, "9781234560017", 1847, 4),
                    (18, "Les Misérables", 18, 7, "9781234560018", 1862, 5),
                    (19, "The Time Machine", 19, 2, "9781234560019", 1895, 3),
            (20, "The Catcher in the Rye", 20, 5, "9781234560020", 1951, 2)
                };
                foreach (var (id, title, author, category, isbn, year, copies) in books)
                {
                    await db.ExecuteAsync("INSERT INTO Books (book_id, title, author_id, category_id, isbn, publication_year, copies_available) VALUES (?, ?, ?, ?, ?, ?, ?)",
                                          id, title, author, category, isbn, year, copies);
                }

                // Seed Dewey Decimal
                var deweys = new (int, string)[]
                {
                    (1, "823.92"),
                    (2, "823.92"),
                    (3, "823.91"),
                    (4, "823.912"),
                    (5, "823.914"),
                    (6, "813.54"),
                    (7, "823.914"),
                    (8, "813.52"),
                    (9, "813.52"),
                    (10, "823.912"),
                    (11, "823.91"),
                    (12, "813.912"),
                    (13, "813.53"),
                    (14, "813.52"),
                    (15, "813.52"),
                    (16, "823.92"),
                    (17, "813.91"),
                    (18, "813.912"),
                    (19, "823.912"),
                    (20, "813.914")

                };
                foreach (var (book, dewey) in deweys)
                {
                    await db.ExecuteAsync("INSERT INTO Dewey_Decimal (book_id, dewey_num) VALUES (?, ?)", book, dewey);
                }


                // Seed Location
                var locations = new (int, int, int, int, string, string)[]
                {
                    (1, 1, 1, 1, "823.92", "LOC1"),
                    (1, 2, 1, 2, "823.92", "LOC2"),
                    (1, 3, 1, 3, "823.91", "LOC3"),
                    (1, 4, 2, 4, "823.912", "LOC4"),
                    (2, 1, 6, 5, "823.914", "LOC5"),
                    (2, 2, 3, 6, "813.54", "LOC6"),
                    (2, 3, 4, 7, "823.914", "LOC7"),
                    (2, 4, 5, 8, "813.52", "LOC8"),
                    (3, 1, 5, 9, "813.52", "LOC9"),
                    (3, 2, 2, 10, "823.912", "LOC10"),
                    (3, 3, 3, 11, "823.91", "LOC11"),
                    (3, 4, 7, 12, "813.912", "LOC12"),
                    (4, 1, 7, 13, "813.53", "LOC13"),
                    (4, 2, 8, 14, "813.52", "LOC14"),
                    (4, 3, 8, 15, "813.52", "LOC15"),
                    (4, 4, 1, 16, "823.92", "LOC16"),
                    (5, 1, 6, 17, "813.91", "LOC17"),
                    (5, 2, 7, 18, "813.912", "LOC18"),
                    (5, 3, 2, 19, "823.912", "LOC19"),
                    (5, 4, 5, 20, "813.914", "LOC20")
                };
                foreach (var (floor, shelf, category, book, dewey, location) in locations)
                {
                    await db.ExecuteAsync("INSERT INTO Location (floor, shelf, category_id, book_id, dewey_Num, location_id) VALUES (?, ?, ?, ?, ?, ?)",
                                          floor, shelf, category, book, dewey, location);
                }

                // Seed Loans
                var loans = new (int, int, int, string, string, string, string)[]
                {
                    (1, 1, 1, "2023-01-01", "2023-01-15", "2023-01-14", "LOC1"),
                    (2, 3, 2, "2023-02-01", "2023-02-15", null, "LOC3"),
                    (3, 5, 3, "2023-03-01", "2023-03-15", "2023-03-10", "LOC5"),
                    (4, 7, 4, "2023-04-01", "2023-04-15", null, "LOC7"),
                    (5, 9, 5, "2023-05-01", "2023-05-15", null, "LOC9"),
                    (6, 11, 6, "2023-06-01", "2023-06-15", "2023-06-14", "LOC11"),
                    (7, 13, 7, "2023-07-01", "2023-07-15", "2023-07-10", "LOC13"),
                    (8, 15, 8, "2023-08-01", "2023-08-15", null, "LOC15"),
                    (9, 17, 9, "2023-09-01", "2023-09-15", null, "LOC17"),
                    (10, 19, 10, "2023-10-01", "2023-10-15", "2023-10-10", "LOC19"),
                    (11, 2, 11, "2023-01-05", "2023-01-19", null, "LOC2"),
                    (12, 4, 12, "2023-02-10", "2023-02-24", "2023-02-22", "LOC4"),
                    (13, 6, 13, "2023-03-15", "2023-03-29", "2023-03-28", "LOC6"),
                    (14, 8, 14, "2023-04-20", "2023-05-04", null, "LOC8"),
                    (15, 10, 15, "2023-05-25", "2023-06-08", "2023-06-05", "LOC10"),
                    (16, 12, 16, "2023-06-30", "2023-07-14", null, "LOC12"),
                    (17, 14, 17, "2023-08-04", "2023-08-18", "2023-08-16", "LOC14"),
                    (18, 16, 18, "2023-09-09", "2023-09-23", null, "LOC16"),
                    (19, 18, 19, "2023-10-14", "2023-10-28", "2023-10-27", "LOC18"),
                    (20, 20, 20, "2023-11-19", "2023-12-03", null, "LOC20")
                };
                foreach (var (id, book, member, loanDate, dueDate, returnDate, location) in loans)
                {
                    await db.ExecuteAsync("INSERT INTO Loans (loan_id, book_id, member_id, loan_date, due_date, return_date, location_id) VALUES (?, ?, ?, ?, ?, ?, ?)",
                                          id, book, member, loanDate, dueDate, returnDate, location);
                }

                

                Debug.WriteLine("Seeding completed successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during seeding: {ex.Message}");
            }
        }
    }
}
