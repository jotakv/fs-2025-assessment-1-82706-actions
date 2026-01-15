namespace fs_2025_assessment_1_82706_blazor.Models
{
    // Very small dto for summary info
    public class StationsSummaryDto
    {
        public int TotalStations { get; set; }
        public int TotalBikeStands { get; set; }
        public int TotalAvailableBikes { get; set; }
        public int OpenStations { get; set; }
        public int ClosedStations { get; set; }
    }
}
