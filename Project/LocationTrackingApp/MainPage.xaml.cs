using Microsoft.Maui.Controls.Maps;

namespace LocationTrackingApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            var map = new Microsoft.Maui.Controls.Maps.Map();
            Content = map;
            //InitializeComponent();
        }

        //private void OnCounterClicked(object? sender, EventArgs e)
        //{
        //    count++;

        //    if (count == 1)
        //        CounterBtn.Text = $"Clicked {count} time";
        //    else
        //        CounterBtn.Text = $"Clicked {count} times";

        //    SemanticScreenReader.Announce(CounterBtn.Text);
        //}
    }
}
