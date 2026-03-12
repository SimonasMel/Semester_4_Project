// Models/Car.cs
namespace FrontEnd.Models
{
    public enum FuelCategory { Petrol, Diesel, Electric, Hybrid, PlugInHybrid }
    public enum TransmissionCategory { Manual, Automatic, SemiAutomatic }
    public enum BodyCategory { Sedan, Estate, Hatchback, SUV, Coupe, Minivan }

    public class Car
    {
        public string Id { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int ProductionYear { get; set; }
        public FuelCategory FuelType { get; set; }
        public TransmissionCategory Transmission { get; set; }
        public BodyCategory BodyType { get; set; }
        public int EnginePowerKW { get; set; }
        public double? FuelConsumptionLitersPer100Km { get; set; }
        public double? EnergyConsumptionKWhPer100Km { get; set; }
        public decimal Price { get; set; }
        public int MileageKm { get; set; }
        public string PrimaryImagePath { get; set; } = string.Empty;
        public List<string> AdditionalImagePaths { get; set; } = new();
        public string Description { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public string Location { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
    }
}