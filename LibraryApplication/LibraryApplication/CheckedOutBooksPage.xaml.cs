namespace LibraryApplication
{
    public partial class CheckedOutBooksPage : ContentPage
    {
        public CheckedOutBooksPage()
        {
            InitializeComponent(); // This links to the XAML
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            string email = EmailEntry.Text?.Trim();

            if (string.IsNullOrEmpty(email))
            {
                await DisplayAlert("Error", "Please enter your email.", "OK");
                return;
            }

            try
            {
                var db = Services.DatabaseHelper.GetConnection();

                // Query for member by email
                var members = await db.QueryAsync<Member>("SELECT * FROM Members WHERE email = ?", email);

                if (members.Count > 0)
                {
                    // Member found
                    var member = members.First();
                    int memberId = member.member_id;

                    // Fetch loaned books
                    var books = await db.QueryAsync<CheckedOutBook>(
                        "SELECT Books.title AS Title, Authors.name AS AuthorName, Categories.name AS CategoryName, " +
                        "Loans.loan_date AS LoanDate, Loans.due_date AS DueDate " +
                        "FROM Loans " +
                        "INNER JOIN Books ON Loans.book_id = Books.book_id " +
                        "INNER JOIN Authors ON Books.author_id = Authors.author_id " +
                        "INNER JOIN Categories ON Books.category_id = Categories.category_id " +
                        "WHERE Loans.member_id = ? AND Loans.return_date IS NULL", memberId);

                    if (books.Count > 0)
                    {
                        // Populate the CollectionView with loaned books
                        CheckedOutBooksCollectionView.ItemsSource = books;
                    }
                    else
                    {
                        // No loaned books found
                        await DisplayAlert("No Loans", "You currently have no checked out books.", "OK");
                        CheckedOutBooksCollectionView.ItemsSource = null;
                    }
                }
                else
                {
                    // Email not found
                    await DisplayAlert("Error", "This email does not belong to a member.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching loans: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while fetching your loans.", "OK");
            }
        }


        private async void OnReturnBookClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button?.CommandParameter is CheckedOutBook selectedBook)
            {
                bool confirm = await DisplayAlert("Return Book", $"Do you want to return '{selectedBook.Title}'?", "Yes", "No");
                if (!confirm) return;

                try
                {
                    var db = Services.DatabaseHelper.GetConnection();
                    DateTime returnDate = DateTime.Now;

                    // Update the `return_date` column for the loan
                    await db.ExecuteAsync("UPDATE Loans SET return_date = ? WHERE loan_id = ?", returnDate.ToString("yyyy-MM-dd"), selectedBook.LoanId);

                    await DisplayAlert("Success", $"'{selectedBook.Title}' has been returned.", "OK");

                    // Refresh the list of checked-out books
                    OnSubmitClicked(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error returning book: {ex.Message}");
                    await DisplayAlert("Error", "An error occurred while returning the book.", "OK");
                }
            }
        }

        public class Member
        {
            public int member_id { get; set; }
            public string name { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public DateTime membership_date { get; set; }
        }


        public class CheckedOutBook
        {
            public int LoanId { get; set; } // Unique loan identifier
            public string Title { get; set; }
            public string AuthorName { get; set; } // New: Author's name
            public string CategoryName { get; set; } // New: Category name
            public string LoanDate { get; set; }
            public string DueDate { get; set; }
        }

    }
}
