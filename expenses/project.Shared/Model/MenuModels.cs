using System;
using System.Collections.Generic;

namespace project.Shared.Model
{
    // Base class for Service items (left unchanged as per request context)
    public class ServiceItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ItemType { get; set; } = "Service";

        public string ServiceSection { get; set; } = string.Empty;
        public string ServiceFolder { get; set; } = string.Empty;
        public string DisplayImageUrl { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;
        public string Station { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? Duration { get; set; }

        public string Barcode { get; set; } = string.Empty;
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public bool OutletAvailability { get; set; } = true;
        public decimal FreePoint { get; set; }
        public decimal RedeemPoint { get; set; }
        public string BillOfMaterial { get; set; } = string.Empty;

        public string Policy { get; set; } = string.Empty;
        public string TermCondition1 { get; set; } = string.Empty;
        public string TermCondition2 { get; set; } = string.Empty;
        public string TermCondition3 { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public string ImageName { get; set; } = string.Empty;
    }

    // New class specifically for Products with "Product" naming convention
    public class ProductItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ItemType { get; set; } = "Product";

        public string ProductSection { get; set; } = string.Empty;
        public string ProductFolder { get; set; } = string.Empty;
        public string DisplayImageUrl { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;
        public string Station { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? Duration { get; set; }

        public string Barcode { get; set; } = string.Empty;

        public string Unit { get; set; } = "Unit"; // Default
        public int? LowStockAlert { get; set; }

        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public bool OutletAvailability { get; set; } = true;
        public decimal FreePoint { get; set; }
        public decimal RedeemPoint { get; set; }
        public string BillOfMaterial { get; set; } = string.Empty;

        public string Policy { get; set; } = string.Empty;
        public string TermCondition1 { get; set; } = string.Empty;
        public string TermCondition2 { get; set; } = string.Empty;
        public string TermCondition3 { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public string ImageName { get; set; } = string.Empty;
    }

    // NEW CLASS: Duplicated from ProductItem with attributes renamed from Product -> Package
    public class PackageItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ItemType { get; set; } = "Package";

        public string PackageSection { get; set; } = string.Empty;
        public string PackageFolder { get; set; } = string.Empty;
        public string DisplayImageUrl { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;
        public string Station { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? Duration { get; set; }

        public string Barcode { get; set; } = string.Empty;

        // Modified for Package Requirements
        public decimal Credits { get; set; }
        public int Points { get; set; }
        public decimal PackageValue { get; set; }
        public List<string> VoucherList { get; set; } = new List<string>();

        public string Unit { get; set; } = "Unit"; // Default
        public int? LowStockAlert { get; set; }

        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public bool OutletAvailability { get; set; } = true;
        public decimal FreePoint { get; set; }
        public decimal RedeemPoint { get; set; }
        public string BillOfMaterial { get; set; } = string.Empty;

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