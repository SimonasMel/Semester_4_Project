namespace Shared.Models
{
    public class UserPreferences
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        public string? PreferredBrand { get; set; }
        public int? PreferredYear { get; set; }
        public decimal? PreferredPrice { get; set; }
        public int? MileageKm { get; set; }
        public int? EnginePowerKW { get; set; }
        public FuelCategory? FuelType { get; set; }
        public TransmissionCategory? Transmission { get; set; }
        public BodyCategory? BodyType { get; set; }
        public bool UseBrand { get; set; } = true;
        public bool UseYear { get; set; } = true;
        public bool UsePrice { get; set; } = true;
        public bool UseMileage { get; set; } = true;
        public bool UseEnginePower { get; set; } = true;
        public bool UseFuelType { get; set; } = true;
        public bool UseTransmission { get; set; } = true;
        public bool UseBodyType { get; set; } = true;
    }
}