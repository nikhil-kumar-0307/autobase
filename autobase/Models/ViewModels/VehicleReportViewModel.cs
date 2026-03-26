namespace autobase.Models.ViewModels
{
    public class VehicleReportViewModel
    {
        public string Period { get; set; } = "week";

        public int TotalRequests { get; set; }
        public string MostUsedVehicle { get; set; }
        public int MostUsedTrips { get; set; }
        public string MostAvailableVehicle { get; set; }
        public int MostAvailableDays { get; set; }
        public int UtilisationPercent { get; set; }

        public List<string> VehicleNames { get; set; } = new();
        public List<int> UsedTrips { get; set; } = new();
        public List<int> AvailableDays { get; set; } = new();
        public List<string> TrendLabels { get; set; } = new();
        public List<VehicleTrendLine> TrendLines { get; set; } = new();

        public int StatusAvailable { get; set; }
        public int StatusInUse { get; set; }
        public int StatusMaintenance { get; set; }
    }

    public class VehicleTrendLine
    {
        public string VehicleName { get; set; }
        public List<int> Data { get; set; } = new();
    }
}