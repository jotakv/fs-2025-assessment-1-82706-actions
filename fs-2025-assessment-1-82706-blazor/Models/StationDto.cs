using System;

namespace fs_2025_assessment_1_82706_blazor.Models
{
    // Simple position dto to read lat and lng
    public class PositionDto
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    // Simple station dto that matches the API
    public class StationDto
    {
        public string id { get; set; } = string.Empty;
        public int number { get; set; }
        public string contract_name { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;
        public PositionDto position { get; set; } = new PositionDto();
        public bool banking { get; set; }
        public bool bonus { get; set; }
        public int bike_stands { get; set; }
        public int available_bike_stands { get; set; }
        public int available_bikes { get; set; }
        public string status { get; set; } = string.Empty;
        public long last_update { get; set; }
        public double occupancy { get; set; }
        public DateTime lastUpdateDublin { get; set; }
        public DateTime lastUpdateUtc { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }

        // Small helper for percentage
        public double OccupancyPercent()
        {
            if (bike_stands <= 0)
            {
                return 0;
            }
            return Math.Round(((double)available_bikes / bike_stands) * 100, 0);
        }

        // Helper to read latitude even if only position is filled
        public double GetLatitude()
        {
            if (lat != 0)
            {
                return lat;
            }
            return position != null ? position.lat : 0;
        }

        // Helper to read longitude even if only position is filled
        public double GetLongitude()
        {
            if (lng != 0)
            {
                return lng;
            }
            return position != null ? position.lng : 0;
        }
    }
}
