using System;
using System.Collections.Generic;

namespace project.Shared.Model
{
    // Base class for all service types
    public class ServiceItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ItemType { get; set; } = "Product"; // Service, Product, Package, Discount

        // Info Tab
        public string ServiceSection { get; set; } = string.Empty;

        // Stores the user-facing path string (e.g. c://image/...)
        public string ServiceFolder { get; set; } = string.Empty;

        // Stores the actual displayable image data (Base64 or Web URL)
        public string DisplayImageUrl { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;
        public string Station { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? Duration { get; set; } // In minutes

        // Advance Tab
        public string Barcode { get; set; } = string.Empty;
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public bool OutletAvailability { get; set; } = true;
        public decimal FreePoint { get; set; }
        public decimal RedeemPoint { get; set; }
        public string BillOfMaterial { get; set; } = string.Empty;

        // T&C Tab
        public string Policy { get; set; } = string.Empty;
        public string TermCondition1 { get; set; } = string.Empty;
        public string TermCondition2 { get; set; } = string.Empty;
        public string TermCondition3 { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public string ImageName { get; set; } = string.Empty;
    }

    public class StationItem
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class MenuSubCategoryItem
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}