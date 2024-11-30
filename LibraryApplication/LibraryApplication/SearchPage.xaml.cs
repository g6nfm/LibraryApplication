using LibraryApplication.Services;

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
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred while performing the search: {ex.Message}", "OK");
        }
    }

    private async Task<List<BookResult>> SearchBooksAsync(string query, string searchField)
    {
        var db = DatabaseHelper.GetConnection();

        string sql = searchField switch
        {
            "Title" => "SELECT Books.title AS Title, Authors.name AS AuthorName, Categories.name AS CategoryName " +
                       "FROM Books " +
                       "INNER JOIN Authors ON Books.author_id = Authors.author_id " +
                       "INNER JOIN Categories ON Books.category_id = Categories.category_id " +
                       "WHERE Books.title LIKE ?",
            "Author" => "SELECT Books.title AS Title, Authors.name AS AuthorName, Categories.name AS CategoryName " +
                        "FROM Books " +
                        "INNER JOIN Authors ON Books.author_id = Authors.author_id " +
                        "INNER JOIN Categories ON Books.category_id = Categories.category_id " +
                        "WHERE Authors.name LIKE ?",
            "Category" => "SELECT Books.title AS Title, Authors.name AS AuthorName, Categories.name AS CategoryName " +
                          "FROM Books " +
                          "INNER JOIN Authors ON Books.author_id = Authors.author_id " +
                          "INNER JOIN Categories ON Books.category_id = Categories.category_id " +
                          "WHERE Categories.name LIKE ?",
            _ => throw new InvalidOperationException("Invalid search field")
        };

        return await db.QueryAsync<BookResult>(sql, $"%{query}%");
    }

    // Model to represent search results
    public class BookResult
    {
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string CategoryName { get; set; }
    }
}
