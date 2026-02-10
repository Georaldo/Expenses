using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using project.Shared.Model;

namespace project.Shared.Pages
{
    public partial class CustomMenu : ComponentBase
    {
        [Parameter] public EventCallback OnToggleSidebar { get; set; }

        // Navigation States
        private bool IsAddMode { get; set; } = false;
        private bool IsEditMode { get; set; } = false;
        private string CurrentAddStep { get; set; } = "Main"; // "Main" or "Selection"
        private string CurrentSelectionType { get; set; } = ""; // "Service", "Product", "Package", "Discount"

        // Data Models
        private List<CustomMenuItem> MenuList = new List<CustomMenuItem>();
        private CustomMenuItemModel NewMenu { get; set; } = new CustomMenuItemModel();

        // Selection Data
        private List<SelectionItem> CurrentSelectionItems { get; set; } = new List<SelectionItem>();
        private string SelectionSearchQuery { get; set; } = "";
        private string SelectionSectionFilter { get; set; } = "All";
        private bool IsMinMaxPriceEnabled { get; set; } = false;

        // Price Popup Data
        private bool IsPriceMenuOpen { get; set; } = false;
        private bool IsPricePopupOpen { get; set; } = false;
        private string PricePopupType { get; set; } = "Increase"; // "Increase" or "Decrease"

        public class CustomMenuItemModel
        {
            public Guid Id { get; set; } = Guid.Empty;
            public string Name { get; set; } = "";
            public bool ShowService { get; set; } = true;
            public bool ShowProduct { get; set; } = true;
            public bool ShowPackage { get; set; } = true;
            public bool ShowDiscount { get; set; } = true;

            // Selected IDs for each type
            public HashSet<Guid> SelectedServices { get; set; } = new HashSet<Guid>();
            public HashSet<Guid> SelectedProducts { get; set; } = new HashSet<Guid>();
            public HashSet<Guid> SelectedPackages { get; set; } = new HashSet<Guid>();
            public HashSet<Guid> SelectedDiscounts { get; set; } = new HashSet<Guid>();

            // Menu Rules
            public string ApplicableMember { get; set; } = "All";
            public List<string> ApplicableDays { get; set; } = new List<string>();
            public bool IsEverydayDate { get; set; } = false;
            public DateTime StartDate { get; set; } = DateTime.Today;
            public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);
            public bool IsAllDayTime { get; set; } = false;
            public DateTime StartTime { get; set; } = DateTime.Today.AddHours(9);
            public DateTime EndTime { get; set; } = DateTime.Today.AddHours(18);
            public string Priority { get; set; } = "Medium";
            public bool RestrictToEligible { get; set; } = false;
            public bool IsEnabled { get; set; } = true;
        }

        // Helper class for the selection list UI
        public class SelectionItem
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public string Name { get; set; } = "";
            public string Section { get; set; } = "";
            public decimal Price { get; set; }
            public string ImageUrl { get; set; } = "";
            public bool IsSelected { get; set; }
        }

        private List<string> DaysOfWeek = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        protected override void OnInitialized()
        {
            MenuList.Add(new CustomMenuItem { Name = "VIP Lunch Menu", Code = "#27252" });
        }

        private async Task TriggerToggleSidebar()
        {
            if (OnToggleSidebar.HasDelegate) await OnToggleSidebar.InvokeAsync();
        }

        private void OpenAddMode()
        {
            IsAddMode = true;
            IsEditMode = false;
            CurrentAddStep = "Main";
            NewMenu = new CustomMenuItemModel();
            NewMenu.ApplicableDays = new List<string>(DaysOfWeek);
        }

        private void OpenEditMode(CustomMenuItem item)
        {
            IsAddMode = true;
            IsEditMode = true;
            CurrentAddStep = "Main";

            // Populate NewMenu with item data (Mock logic)
            NewMenu = new CustomMenuItemModel
            {
                Id = item.Id,
                Name = item.Name,
                ApplicableDays = new List<string>(DaysOfWeek)
            };
        }

        private void CloseAddMode()
        {
            IsAddMode = false;
            IsEditMode = false;
            CurrentAddStep = "Main";
        }

        // Navigation to Selection View
        private void OpenSelectionView(string type)
        {
            CurrentSelectionType = type;
            CurrentAddStep = "Selection";
            SelectionSearchQuery = "";
            SelectionSectionFilter = "All";
            IsMinMaxPriceEnabled = false;
            IsPriceMenuOpen = false; // Reset menu state

            // Generate Mock Data based on Type
            CurrentSelectionItems = new List<SelectionItem>();

            if (type == "Service")
            {
                CurrentSelectionItems.Add(new SelectionItem { Name = "Treatment", Section = "Section 1", Price = 100.00m });
                CurrentSelectionItems.Add(new SelectionItem { Name = "Pico laser", Section = "Section 1", Price = 99.00m });
                CurrentSelectionItems.Add(new SelectionItem { Name = "Skin hydration facial", Section = "Section 1", Price = 89.00m });
                CurrentSelectionItems.Add(new SelectionItem { Name = "Skin collagen facial", Section = "Section 1", Price = 129.00m });
                CurrentSelectionItems.Add(new SelectionItem { Name = "Antioxidant facial", Section = "Section 1", Price = 99.00m });
            }
            else if (type == "Product")
            {
                CurrentSelectionItems.Add(new SelectionItem { Name = "Shampoo", Section = "Hair Care", Price = 45.00m });
                CurrentSelectionItems.Add(new SelectionItem { Name = "Conditioner", Section = "Hair Care", Price = 45.00m });
            }
            // ... add others as needed

            // Restore selection state
            var selectedSet = GetSelectedSetForType(type);
            foreach (var item in CurrentSelectionItems)
            {
                if (selectedSet.Contains(item.Id)) item.IsSelected = true;
            }
        }

        private void SaveSelection()
        {
            var selectedSet = GetSelectedSetForType(CurrentSelectionType);
            selectedSet.Clear();
            foreach (var item in CurrentSelectionItems.Where(x => x.IsSelected))
            {
                selectedSet.Add(item.Id);
            }

            CurrentAddStep = "Main";
        }

        private HashSet<Guid> GetSelectedSetForType(string type)
        {
            return type switch
            {
                "Service" => NewMenu.SelectedServices,
                "Product" => NewMenu.SelectedProducts,
                "Package" => NewMenu.SelectedPackages,
                "Discount" => NewMenu.SelectedDiscounts,
                _ => new HashSet<Guid>()
            };
        }

        private void ToggleSelection(SelectionItem item)
        {
            item.IsSelected = !item.IsSelected;
        }

        private void SaveCustomMenu()
        {
            if (!string.IsNullOrWhiteSpace(NewMenu.Name))
            {
                if (IsEditMode)
                {
                    // Update existing
                    var existing = MenuList.FirstOrDefault(x => x.Id == NewMenu.Id);
                    if (existing != null) existing.Name = NewMenu.Name;
                }
                else
                {
                    // Add new
                    MenuList.Add(new CustomMenuItem { Name = NewMenu.Name, Code = "#" + new Random().Next(10000, 99999).ToString() });
                }
            }
            IsAddMode = false;
        }

        private void ToggleDay(string day)
        {
            if (NewMenu.ApplicableDays.Contains(day)) NewMenu.ApplicableDays.Remove(day);
            else NewMenu.ApplicableDays.Add(day);
        }

        private void ToggleEveryday(ChangeEventArgs e)
        {
            if ((bool)(e.Value ?? false)) NewMenu.ApplicableDays = new List<string>(DaysOfWeek);
            else NewMenu.ApplicableDays.Clear();
        }

        private void TogglePriceMenu()
        {
            IsPriceMenuOpen = !IsPriceMenuOpen;
        }

        private void OpenPricePopup(string type)
        {
            IsPriceMenuOpen = false; // Close menu
            PricePopupType = type;
            IsPricePopupOpen = true;
        }

        private void ApplyPriceChange()
        {
            // Logic to apply price change to selected items
            IsPricePopupOpen = false;
        }
    }
}