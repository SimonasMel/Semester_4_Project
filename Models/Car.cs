using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarApp.Models
{
    public enum FuelCategory { Petrol, Diesel, Electric, Hybrid, PlugInHybrid }
    public enum TransmissionCategory { Manual, Automatic, SemiAutomatic }
    public enum BodyCategory { Sedan, Estate, Hatchback, SUV, Coupe, Minivan }

    /// <summary>
    /// Represents a car with detailed information about its identity, specifications, features, and listing attributes
    /// for use in automotive applications.
    /// </summary>
    public class Car
    {
        public string Id { get; set; }  

        [Display(Name = "Brand")]
        [Required(ErrorMessage = "Brand is required")]
        [StringLength(50, ErrorMessage = "Brand must not exceed 50 characters")]
        public string Brand { get; set; }

        [Display(Name = "Model")]
        [Required(ErrorMessage = "Model is required")]
        [StringLength(50, ErrorMessage = "Model must not exceed 50 characters")]
        public string Model { get; set; }

        [Display(Name = "Production Year")]
        [Required(ErrorMessage = "Production year is required")]
        [Range(1980, 2026, ErrorMessage = "Please enter a valid production year")]
        public int ProductionYear { get; set; }

        [Display(Name = "Fuel Type")]
        [Required(ErrorMessage = "Fuel type is required")]
        public FuelCategory FuelType { get; set; }

        [Display(Name = "Transmission")]
        [Required(ErrorMessage = "Transmission type is required")]
        public TransmissionCategory Transmission { get; set; }

        [Display(Name = "Body Type")]
        [Required(ErrorMessage = "Body type is required")]
        public BodyCategory BodyType { get; set; }

        [Display(Name = "Engine Power (kW)")]
        [Required(ErrorMessage = "Engine power is required")]
        [Range(1, 1500, ErrorMessage = "Power must be between 1 and 1500 kW")]
        public int EnginePowerKW { get; set; } 

        [Display(Name = "Fuel Consumption (L/100km)")]
        [Range(0.1, 50, ErrorMessage = "Fuel consumption must be between 0.1 and 50")]
        public double? FuelConsumptionLitersPer100Km { get; set; } 

        [Display(Name = "Energy Consumption (kWh/100km)")]
        [Range(0.1, 100, ErrorMessage = "Energy consumption must be between 0.1 and 100")]
        public double? EnergyConsumptionKWhPer100Km { get; set; }

        [Display(Name = "Price")]
        [Required(ErrorMessage = "Price is required")]
        [Range(100, 1000000, ErrorMessage = "Price must be between €100 and €1,000,000")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Mileage (km)")]
        [Required(ErrorMessage = "Mileage is required")]
        [Range(0, 1000000, ErrorMessage = "Mileage must be between 0 and 1,000,000 km")]
        public int MileageKm { get; set; }

        [Display(Name = "Primary Image")]
        [Required(ErrorMessage = "Car image is required")]
        [StringLength(500, ErrorMessage = "Image path must not exceed 500 characters")]
        public string PrimaryImagePath { get; set; }

        [Display(Name = "Additional Images")]
        public List<string> AdditionalImagePaths { get; set; } = new List<string>();

        [Display(Name = "Description")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        public string Description { get; set; }

        [Display(Name = "Features")]
        public List<string> Features { get; set; } = new List<string>();

        [Display(Name = "Location/City")]
        [Required(ErrorMessage = "Location is required")]
        [StringLength(100, ErrorMessage = "Location must not exceed 100 characters")]
        public string Location { get; set; }

        [Display(Name = "Contact information")]
        [Required(ErrorMessage = "Contact information is required")]
        [StringLength(100, ErrorMessage = "Contact information must not exceed 100 characters")]
        public string ContactInfo { get; set; }

        [Display(Name = "VIN")]
        [StringLength(17, MinimumLength = 17, ErrorMessage = "VIN must be exactly 17 characters")]
        public string VIN { get; set; }

        public Car()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}