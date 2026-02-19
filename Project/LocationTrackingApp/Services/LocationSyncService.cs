using LocationTrackingApp.Models;
using SQLite;

namespace LocationTrackingApp.Services
{
    public class LocationSyncService
    {
        private SQLiteAsyncConnection _db;

        public async Task Init()
        {
            if (_db != null) return;
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "locations.db3");
            _db = new SQLiteAsyncConnection(dbPath);
            await _db.CreateTableAsync<DeviceLocation>();
        }

        public async Task StartTracking()
        {
            await Init();
            // Set tracking intervals (1 minute for example)
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(60));

            // This should ideally run in a background task/service
            var location = await Geolocation.Default.GetLocationAsync(request);
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

        public async Task<List<DeviceLocation>> GetPoints() => await _db.Table<DeviceLocation>().ToListAsync();
    }
}
