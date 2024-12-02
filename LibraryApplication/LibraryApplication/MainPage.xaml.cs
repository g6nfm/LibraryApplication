using Microsoft.Maui.Controls;

namespace LibraryApplication;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    // Navigation Event Handlers
    private async void OnCheckedOutBooksClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CheckedOutBooksPage());
    }



    private async void OnSearchClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SearchPage());
    }

    // Visual Feedback Event Handlers
    private async void OnButtonPressed(object sender, EventArgs e)
    {
        var button = (Button)sender;

        // Darken the current color for the pressed state
        string originalColor = button.BackgroundColor.ToHex(); // Save original color
        button.CommandParameter = originalColor; // Store original color in CommandParameter

        // Darken the color slightly
        button.BackgroundColor = Color.FromArgb(DarkenColor(originalColor));
        await button.ScaleTo(0.95, 50); // Slight shrink effect
    }

    private async void OnButtonReleased(object sender, EventArgs e)
    {
        var button = (Button)sender;

        // Retrieve the original color from CommandParameter
        if (button.CommandParameter is string originalColorHex)
        {
            button.BackgroundColor = Color.FromArgb(originalColorHex);
        }

        await button.ScaleTo(1, 50); // Restore original size
    }

    // Helper method to darken a color
    private string DarkenColor(string hexColor)
    {
        // Convert hex to RGB
        int r = Convert.ToInt32(hexColor.Substring(1, 2), 16);
        int g = Convert.ToInt32(hexColor.Substring(3, 2), 16);
        int b = Convert.ToInt32(hexColor.Substring(5, 2), 16);

        // Darken each channel by 20%
        r = (int)(r * 0.8);
        g = (int)(g * 0.8);
        b = (int)(b * 0.8);

        // Ensure the values are within bounds
        r = Math.Clamp(r, 0, 255);
        g = Math.Clamp(g, 0, 255);
        b = Math.Clamp(b, 0, 255);

        // Convert back to hex
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}
