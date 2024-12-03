namespace LibraryApplication
{
    public partial class PasswordPage : ContentPage
    {
        public PasswordPage()
        {
            InitializeComponent();
        }

        private async void OnPasswordSubmitClicked(object sender, EventArgs e)
        {
            string password = PasswordEntry.Text?.Trim();

            if (password == "12345")
            {
                await Navigation.PushAsync(new MembersPage());
            }
            else
            {
                await DisplayAlert("Error", "Incorrect password. Access denied.", "OK");
            }
        }
    }
}