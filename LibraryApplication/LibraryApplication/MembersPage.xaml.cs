using System;
using System.Linq;
using Microsoft.Maui.Controls;

namespace LibraryApplication
{
    public partial class MembersPage : ContentPage
    {
        public MembersPage()
        {
            InitializeComponent();
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            string query = SearchEntry.Text?.Trim();
            var db = Services.DatabaseHelper.GetConnection();

            try
            {
                var results = await db.QueryAsync<Member>(
                    "SELECT * FROM Members WHERE name LIKE ? OR email LIKE ?",
                    $"%{query}%", $"%{query}%");

                // Update the CollectionView
                MembersCollectionView.ItemsSource = results;

                // Update the Results Count
                ResultsCountLabel.Text = $"Results: {results.Count}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching members: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while searching for members.", "OK");
            }
        }

        public class Member
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public DateTime MembershipDate { get; set; }
        }
    }
}
