using LocationTrackingApp.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace LocationTrackingApp
{
    public partial class MainPage : ContentPage
    {
        private readonly LocationSyncService _locationService;
        private CancellationTokenSource? _refreshCts;
        private bool _isNewRoute = true;
        private int _lastPointCount;

        public MainPage(LocationSyncService locationService)
        {
            InitializeComponent();
            _locationService = locationService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RequestLocationPermission();

            if (_isNewRoute)
            {
                await _locationService.ClearPoints();
                map.MapElements.Clear();
                map.Pins.Clear();
                _isNewRoute = false;
            }

            await _locationService.StartTracking();
            await UpdateStatus();
            StartAutoRefresh();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _refreshCts?.Cancel();
            _locationService.StopTracking();
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

        private void StartAutoRefresh()
        {
            _refreshCts?.Cancel();
            _refreshCts = new CancellationTokenSource();
            var token = _refreshCts.Token;

            Dispatcher.StartTimer(TimeSpan.FromSeconds(6), () =>
            {
                if (token.IsCancellationRequested)
                    return false;

                MainThread.BeginInvokeOnMainThread(async () => await RefreshHeatMap());
                return !token.IsCancellationRequested;
            });
        }

        private async Task RefreshHeatMap()
        {
            var points = await _locationService.GetPoints();
            map.MapElements.Clear();
            map.Pins.Clear();

            if (points.Count > 0)
            {
                DrawHeatMap(points);

                // Only re-center when new points arrive to avoid resetting zoom/tiles
                if (points.Count != _lastPointCount)
                {
                    CenterMapOnPoints(points);
                    _lastPointCount = points.Count;
                }
            }

            await UpdateStatus();
        }

        private void DrawHeatMap(List<Models.DeviceLocation> points)
        {
            // Sort points by timestamp so the route is drawn in order
            var sorted = points.OrderBy(p => p.Timestamp).ToList();

            // Draw route polyline connecting all points
            var polyline = new Polyline
            {
                StrokeColor = Color.FromArgb("#4A90D9"),
                StrokeWidth = 4
            };
            foreach (var pt in sorted)
            {
                polyline.Geopath.Add(new Location(pt.Latitude, pt.Longitude));
            }
            map.MapElements.Add(polyline);

            // Pre-compute density for each point based on nearby neighbors
            const double proximityThresholdKm = 0.1; // 100 meters
            var densities = new int[sorted.Count];

            for (int i = 0; i < sorted.Count; i++)
            {
                int count = 0;
                for (int j = 0; j < sorted.Count; j++)
                {
                    if (i == j) continue;
                    double dist = HaversineDistanceKm(
                        sorted[i].Latitude, sorted[i].Longitude,
                        sorted[j].Latitude, sorted[j].Longitude);
                    if (dist <= proximityThresholdKm)
                        count++;
                }
                densities[i] = count;
            }

            int maxDensity = densities.Length > 0 ? densities.Max() : 1;
            if (maxDensity == 0) maxDensity = 1;

            // Draw colored dot markers at each point
            for (int i = 0; i < sorted.Count; i++)
            {
                double normalizedDensity = (double)densities[i] / maxDensity;

                // Map density to color: blue (cold/low) → cyan → green → yellow → red (hot/high)
                Color fillColor = GetHeatColor(normalizedDensity);

                // Small pin-like dot markers
                var circle = new Circle
                {
                    Center = new Location(sorted[i].Latitude, sorted[i].Longitude),
                    Radius = new Distance(8),
                    StrokeColor = fillColor,
                    StrokeWidth = 3,
                    FillColor = fillColor.WithAlpha(0.9f)
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

            // Padding around points with a reasonable min/max zoom
            double spanLat = Math.Max(latRange * 1.3, 0.008);
            double spanLon = Math.Max(lonRange * 1.3, 0.008);

            // Cap so it doesn't zoom out too far
            spanLat = Math.Min(spanLat, 0.1);
            spanLon = Math.Min(spanLon, 0.1);

            map.MoveToRegion(new MapSpan(new Location(avgLat, avgLon), spanLat, spanLon));
        }

        private async Task UpdateStatus()
        {
            int count = await _locationService.GetPointCount();
            statusLabel.Text = $"Tracking: On | Points: {count}";
        }
    }
}
