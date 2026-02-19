using LocationTrackingApp.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace LocationTrackingApp
{
    public partial class MainPage : ContentPage
    {
        private readonly LocationSyncService _locationService;

        public MainPage(LocationSyncService locationService)
        {
            InitializeComponent();
            _locationService = locationService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RequestLocationPermission();
            await UpdateStatus();
        }

        private async Task RequestLocationPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission Required",
                        "Location permission is needed to track your position.", "OK");
                }
            }
        }

        private async void OnTrackToggleClicked(object sender, EventArgs e)
        {
            if (_locationService.IsTracking)
            {
                _locationService.StopTracking();
                trackButton.Text = "Start Tracking";
            }
            else
            {
                await _locationService.StartTracking();
                trackButton.Text = "Stop Tracking";
            }

            await UpdateStatus();
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            var points = await _locationService.GetPoints();
            map.MapElements.Clear();

            if (points.Count == 0)
            {
                await DisplayAlert("No Data", "No location points recorded yet. Start tracking first.", "OK");
                return;
            }

            DrawHeatMap(points);
            CenterMapOnPoints(points);
            await UpdateStatus();
        }

        private async void OnClearClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Clear Data",
                "Are you sure you want to delete all location data?", "Yes", "No");
            if (confirm)
            {
                await _locationService.ClearPoints();
                map.MapElements.Clear();
                await UpdateStatus();
            }
        }

        private void DrawHeatMap(List<Models.DeviceLocation> points)
        {
            // Pre-compute density for each point based on nearby neighbors
            const double proximityThresholdKm = 0.1; // 100 meters
            var densities = new int[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                int count = 0;
                for (int j = 0; j < points.Count; j++)
                {
                    if (i == j) continue;
                    double dist = HaversineDistanceKm(
                        points[i].Latitude, points[i].Longitude,
                        points[j].Latitude, points[j].Longitude);
                    if (dist <= proximityThresholdKm)
                        count++;
                }
                densities[i] = count;
            }

            int maxDensity = densities.Length > 0 ? densities.Max() : 1;
            if (maxDensity == 0) maxDensity = 1;

            for (int i = 0; i < points.Count; i++)
            {
                double normalizedDensity = (double)densities[i] / maxDensity;

                // Map density to color: blue (cold/low) → green → yellow → red (hot/high)
                Color fillColor = GetHeatColor(normalizedDensity);

                // Higher density = smaller, more opaque circles (concentrated heat)
                // Lower density = larger, more transparent circles (diffuse)
                double radius = 50 + (1 - normalizedDensity) * 100; // 50m to 150m
                double opacity = 0.15 + normalizedDensity * 0.45;   // 0.15 to 0.60

                var circle = new Circle
                {
                    Center = new Location(points[i].Latitude, points[i].Longitude),
                    Radius = new Distance(radius),
                    StrokeColor = Colors.Transparent,
                    FillColor = fillColor.WithAlpha((float)opacity)
                };

                map.MapElements.Add(circle);
            }
        }

        private static Color GetHeatColor(double t)
        {
            // Gradient: Blue (0.0) → Cyan (0.25) → Green (0.5) → Yellow (0.75) → Red (1.0)
            float r, g, b;

            if (t < 0.25)
            {
                float ratio = (float)(t / 0.25);
                r = 0; g = ratio; b = 1;
            }
            else if (t < 0.5)
            {
                float ratio = (float)((t - 0.25) / 0.25);
                r = 0; g = 1; b = 1 - ratio;
            }
            else if (t < 0.75)
            {
                float ratio = (float)((t - 0.5) / 0.25);
                r = ratio; g = 1; b = 0;
            }
            else
            {
                float ratio = (float)((t - 0.75) / 0.25);
                r = 1; g = 1 - ratio; b = 0;
            }

            return new Color(r, g, b);
        }

        private static double HaversineDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371.0; // Earth's radius in km
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

        private void CenterMapOnPoints(List<Models.DeviceLocation> points)
        {
            double avgLat = points.Average(p => p.Latitude);
            double avgLon = points.Average(p => p.Longitude);

            double latRange = points.Max(p => p.Latitude) - points.Min(p => p.Latitude);
            double lonRange = points.Max(p => p.Longitude) - points.Min(p => p.Longitude);

            // Add some padding to the range
            double spanLat = Math.Max(latRange * 1.5, 0.005);
            double spanLon = Math.Max(lonRange * 1.5, 0.005);

            map.MoveToRegion(new MapSpan(new Location(avgLat, avgLon), spanLat, spanLon));
        }

        private async Task UpdateStatus()
        {
            int count = await _locationService.GetPointCount();
            string trackingState = _locationService.IsTracking ? "On" : "Off";
            statusLabel.Text = $"Tracking: {trackingState} | Points: {count}";
        }
    }
}
