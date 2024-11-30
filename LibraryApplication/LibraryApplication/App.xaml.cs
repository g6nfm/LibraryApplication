using System.Diagnostics;

namespace LibraryApplication
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        // initializes the db
        protected override async void OnStart()
        {
            
            base.OnStart();

            await Services.DatabaseHelper.SetupDatabaseAsync();

        }
    }


}
