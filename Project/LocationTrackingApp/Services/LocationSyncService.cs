using LocationTrackingApp.Models;
using SQLite;

namespace LocationTrackingApp.Services
{
    public class LocationSyncService
    {
        private SQLiteAsyncConnection _db;
        private CancellationTokenSource _trackingCts;
        private bool _isTracking;

        public bool IsTracking => _isTracking;

        public async Task Init()
        {
            if (_db != null) return;
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "locations.db3");
            _db = new SQLiteAsyncConnection(dbPath);
            await _db.CreateTableAsync<DeviceLocation>();
        }

        public async Task StartTracking()
        {
            if (_isTracking) return;

            await Init();
            _isTracking = true;
            _trackingCts = new CancellationTokenSource();

            // Run tracking loop in background
            _ = TrackingLoop(_trackingCts.Token);
        }

        public void StopTracking()
        {
            _isTracking = false;
            _trackingCts?.Cancel();
            _trackingCts?.Dispose();
            _trackingCts = null;
        }

        private async Task TrackingLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                    var location = await Geolocation.Default.GetLocationAsync(request, cancellationToken);

                    if (location != null)
                    {
                        await _db.InsertAsync(new DeviceLocation
                        {
                            Latitude = location.Latitude,
                            Longitude = location.Longitude,
                            Timestamp = DateTime.UtcNow
                        });
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    // Geolocation may fail due to permissions or hardware; continue loop
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        public async Task<List<DeviceLocation>> GetPoints()
        {
            await Init();
            return await _db.Table<DeviceLocation>().ToListAsync();
        }

        public async Task<int> GetPointCount()
        {
            await Init();
            return await _db.Table<DeviceLocation>().CountAsync();
        }

        public async Task ClearPoints()
        {
            await Init();
            await _db.DeleteAllAsync<DeviceLocation>();
        }
    }
}
