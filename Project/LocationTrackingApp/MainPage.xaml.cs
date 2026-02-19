using LocationTrackingApp.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace LocationTrackingApp
{
    public partial class MainPage : ContentPage
    {
        private readonly LocationSyncService _locationService;

        //int count = 0;

        public MainPage()
        {
            _locationService = new();
            //Location initialLocation = new(30.2672, -97.715942);
            //MapSpan mapSpan = new(initialLocation, 0.1, 0.1);
            //Microsoft.Maui.Controls.Maps.Map map = new(mapSpan);
            //Content = map;

            //Location location = new Location(42.361145, -71.057083);
            //MapSpan mapSpan = new MapSpan(location, 0.1, 0.1);
            //Microsoft.Maui.Controls.Maps.Map map = new(mapSpan);
            //Content = map;

            var map = new Microsoft.Maui.Controls.Maps.Map();
            Content = map;

            //InitializeComponent();
        }


        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            var points = await _locationService.GetPoints();
            map.MapElements.Clear();

            foreach (var point in points)
            {
                var circle = new Microsoft.Maui.Controls.Maps.Circle
                {
                    Center = new Location(point.Latitude, point.Longitude),
                    Radius = new Distance(50), // 50 meters
                    StrokeColor = Color.FromRgba(255, 0, 0, 0), // Transparent border
                    FillColor = Color.FromRgba(255, 0, 0, 0.2) // Low opacity red
                };

                map.MapElements.Add(circle);
            }
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
