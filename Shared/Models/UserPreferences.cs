namespace Shared.Models
{
    public class UserPreferences
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Brand (empty = any)
        public string? PreferredBrand { get; set; }

        // Price
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Year
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }

        // Mileage
        public int? MaxMileageKm { get; set; }

        // Engine power
        public int? MinEnginePowerKW { get; set; }

        // Enums (null = no preference)
        public FuelCategory? FuelType { get; set; }
        public TransmissionCategory? Transmission { get; set; }
        public BodyCategory? BodyType { get; set; }
    }
}