using LibraryApplication.Services;
using System.Diagnostics;

namespace LibraryApplication;

public partial class SearchPage : ContentPage
{
    public SearchPage()
    {
        InitializeComponent();
        SearchFieldPicker.SelectedIndex = 0; // Default to "Title"
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        string query = SearchEntry.Text?.Trim();
        string searchField = SearchFieldPicker.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(searchField))
        {
            await DisplayAlert("Error", "Please enter a search query and select a search field.", "OK");
            return;
        }

        try
        {
            // Fetch data based on the search criteria
            var books = await SearchBooksAsync(query, searchField);
            BooksCollectionView.ItemsSource = books;

            // Update results count
            ResultsCountLabel.Text = $"Results: {books.Count}";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred while performing the search: {ex.Message}", "OK");
        }
    }

    private async void OnLoanBookClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.CommandParameter is BookResult selectedBook)
        {
            try
            {
                string email = await DisplayPromptAsync("Loan Book", "Enter your email:");
                if (string.IsNullOrEmpty(email))
                {
                    await DisplayAlert("Error", "Email is required to loan a book.", "OK");
                    return;
                }

                var db = DatabaseHelper.GetConnection();

                // Check for existing member
                var existingMember = (await db.QueryAsync<Member>(
                    "SELECT * FROM Members WHERE email = ?", email)).FirstOrDefault();

                int memberId;

                if (existingMember != null)
                {
                    memberId = existingMember.member_id;
                }
                else
                {
                    // Prompt user for required details
                    string name = await DisplayPromptAsync("New Member", "Enter your full name:");
                    string phone = await DisplayPromptAsync("New Member", "Enter your phone number (XXX-XXX-XXXX):");
                    DateTime membershipDate = DateTime.Now;

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone))
                    {
                        await DisplayAlert("Error", "All fields are required to create a new member.", "OK");
                        return;
                    }

                    // Insert new member
                    await db.ExecuteAsync(
                        "INSERT INTO Members (name, email, phone, membership_date) VALUES (?, ?, ?, ?)",
                        name, email, phone, membershipDate.ToString("yyyy-MM-dd"));

                    // Retrieve the newly created member_id
                    memberId = await db.ExecuteScalarAsync<int>(
                        "SELECT member_id FROM Members WHERE email = ?", email);
                }

                // Insert loan
                DateTime loanDate = DateTime.Now;
                DateTime dueDate = loanDate.AddDays(14); // Example 14-day loan period

                await db.ExecuteAsync(
                    "INSERT INTO Loans (book_id, member_id, loan_date, due_date, location_id) VALUES (?, ?, ?, ?, ?)",
                    selectedBook.BookId, memberId, loanDate.ToString("yyyy-MM-dd"), dueDate.ToString("yyyy-MM-dd"), "LOC1");


                await DisplayAlert("Success", $"{selectedBook.Title} has been loaned to {email}.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loaning book: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while loaning the book.", "OK");
            }
        }
    }


    private void OnBookSelected(object sender, SelectionChangedEventArgs e)
    {
        var selectedBook = e.CurrentSelection.FirstOrDefault() as BookResult;
        if (selectedBook != null)
        {
            DisplayAlert("Book Selected", $"You selected: {selectedBook.Title}", "OK");
        }
    }

    private async Task<List<BookResult>> SearchBooksAsync(string query, string searchField)
    {
        var db = DatabaseHelper.GetConnection();

        string sql = searchField switch
        {
            "Title" => "SELECT Books.book_id AS BookId, Books.title AS Title, Authors.name AS AuthorName, Categories.name AS CategoryName, " +
                       "COALESCE(Dewey_Decimal.dewey_num, 'N/A') AS DeweyDecimal, " +
                       "COALESCE(Location.location_id || ' (Floor: ' || Location.floor || ', Shelf: ' || Location.shelf || ')', 'Location not available') AS LocationDetails " +
                       "FROM Books " +
                       "INNER JOIN Authors ON Books.author_id = Authors.author_id " +
                       "INNER JOIN Categories ON Books.category_id = Categories.category_id " +
                       "LEFT JOIN Location ON Books.book_id = Location.book_id " +
                       "LEFT JOIN Dewey_Decimal ON Books.book_id = Dewey_Decimal.book_id " +
                       "WHERE Books.title LIKE ?",

            "Author" => "SELECT Books.book_id AS BookId, Books.title AS Title, Authors.name AS AuthorName, Categories.name AS CategoryName, " +
                        "COALESCE(Dewey_Decimal.dewey_num, 'N/A') AS DeweyDecimal, " +
                        "COALESCE(Location.location_id || ' (Floor: ' || Location.floor || ', Shelf: ' || Location.shelf || ')', 'Location not available') AS LocationDetails " +
                        "FROM Books " +
                        "INNER JOIN Authors ON Books.author_id = Authors.author_id " +
                        "INNER JOIN Categories ON Books.category_id = Categories.category_id " +
                        "LEFT JOIN Location ON Books.book_id = Location.book_id " +
                        "LEFT JOIN Dewey_Decimal ON Books.book_id = Dewey_Decimal.book_id " +
                        "WHERE Authors.name LIKE ?",

            "Category" => "SELECT Books.book_id AS BookId, Books.title AS Title, Authors.name AS AuthorName, Categories.name AS CategoryName, " +
                          "COALESCE(Dewey_Decimal.dewey_num, 'N/A') AS DeweyDecimal, " +
                          "COALESCE(Location.location_id || ' (Floor: ' || Location.floor || ', Shelf: ' || Location.shelf || ')', 'Location not available') AS LocationDetails " +
                          "FROM Books " +
                          "INNER JOIN Authors ON Books.author_id = Authors.author_id " +
                          "INNER JOIN Categories ON Books.category_id = Categories.category_id " +
                          "LEFT JOIN Location ON Books.book_id = Location.book_id " +
                          "LEFT JOIN Dewey_Decimal ON Books.book_id = Dewey_Decimal.book_id " +
                          "WHERE Categories.name LIKE ?",

            _ => throw new InvalidOperationException("Invalid search field")
        };

        try
        {
            Debug.WriteLine($"Executing SQL: {sql}");
            Debug.WriteLine($"Query Parameter: %{query}%");
            var results = await db.QueryAsync<BookResult>(sql, $"%{query}%");
            Debug.WriteLine($"Books Found: {results.Count}");
            return results;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SQL Error: {ex.Message}");
            throw;
        }
    }

    private async void OnAddToCartClicked(object sender, EventArgs e)
    {
        // Get the button's bound BookResult
        var button = sender as Button;
        if (button?.CommandParameter is BookResult selectedBook)
        {
            try
            {
                // Prompt user for confirmation
                bool addToCart = await DisplayAlert("Add to Cart",
                    $"Do you want to add '{selectedBook.Title}' to your cart?", "Yes", "No");
                if (!addToCart) return;

                // Get current member ID (Assume it's stored in a session or settings)
                int memberId = 1;

                // Insert into the Cart table
                var db = DatabaseHelper.GetConnection();
                await db.ExecuteAsync("INSERT INTO Cart (member_id, book_id) VALUES (?, ?)", memberId, selectedBook.BookId);

                // Show confirmation
                await DisplayAlert("Success", $"{selectedBook.Title} has been added to your cart.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding to cart: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while adding the book to your cart.", "OK");
            }
        }
    }

    // Model to represent search results
    public class BookResult
    {
        public int BookId { get; set; } // Change from bookID to BookId
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string CategoryName { get; set; }
        public string DeweyDecimal { get; set; } = "N/A"; // Default value
        public string LocationDetails { get; set; } = "Location not available"; // Default value
    }

    public class Member
    {
        public int member_id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public DateTime membership_date { get; set; }
    }

}
