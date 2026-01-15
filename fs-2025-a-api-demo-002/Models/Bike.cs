namespace fs_2025_a_api_demo_002.Models
{
    public class Position
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class BikeModel
    {
        public int number { get; set; }
        public string contract_name { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;
        public Position position { get; set; } = new Position();
        public bool banking { get; set; }
        public bool bonus { get; set; }
        public int bike_stands { get; set; }
        public int available_bike_stands { get; set; }
        public int available_bikes { get; set; }
        public string status { get; set; } = string.Empty;
        public long last_update { get; set; }

        // Cosmos DB will expect an id. For V1 we just mirror the station number.
        public string id { get; set; } = string.Empty;

        public double Occupancy
        {
            get
            {
                if (bike_stands <= 0)
                {
                    return 0;
                }
                return (double)available_bikes / bike_stands;
            }
        }

        public DateTime LastUpdateUtc
        {
            get
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(last_update).UtcDateTime;
            }
        }

        public DateTime LastUpdateDublin
        {
            get
            {
                try
                {
                    var zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Dublin");
                    return TimeZoneInfo.ConvertTimeFromUtc(LastUpdateUtc, zone);
                }
                catch
                {
                    return LastUpdateUtc;
                }
            }
        }
    }
}
