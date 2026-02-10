using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using project.Shared.Model;

namespace project.Shared.Pages
{
    public partial class Catalog : ComponentBase
    {
        [Parameter] public EventCallback OnToggleSidebar { get; set; }

        private List<CatalogItem> CatalogList = new List<CatalogItem>();

        // Selection Mode State
        private bool IsSelectionMode { get; set; } = false;
        private string CurrentSelectionTab { get; set; } = "Service";
        private List<SelectionItem> SelectionItems { get; set; } = new List<SelectionItem>();

        // Add Catalog Form State
        private string SelectionStep { get; set; } = "List"; // "List" or "Details"
        private SelectionItem SelectedItem { get; set; } = new SelectionItem();
        private string AddCatalogTab { get; set; } = "Info"; // "Info" or "Advance"

        // Info Tab Fields
        private string FormFolder { get; set; } = "";
        private decimal FormPrice { get; set; }
        private decimal FormCredit { get; set; }
        private int FormPoint { get; set; }
        private DateTime FormStartDate { get; set; } = DateTime.Today;
        private DateTime FormEndDate { get; set; } = DateTime.Today.AddDays(7);

        // Advance Tab Fields
        private string FormDisplayName { get; set; } = "";
        private string FormDisplaySection { get; set; } = "";
        private string FormDisplayFolder { get; set; } = "";

        // Sort State
        private bool IsSortMenuOpen { get; set; } = false;

        // Calculator State
        private bool IsCalculatorOpen { get; set; } = false;
        private string CalculatorTarget { get; set; } = ""; // "Price", "Credit", "Point"
        private string CalculatorDisplay { get; set; } = "0";
        private bool IsCalculatorNewEntry { get; set; } = true;

        public class SelectionItem
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public string Name { get; set; } = "";
            public string Section { get; set; } = "Section 1";
            public decimal Price { get; set; }
        }

        protected override void OnInitialized()
        {
            // Initial Catalog Data
            CatalogList.Add(new CatalogItem
            {
                ItemName = "Treatment",
                Price = 100.00m,
                Section = "Section 1",
                RedeemableCredits = 6333.00m,
                StartDate = new DateTime(2026, 2, 6),
                EndDate = new DateTime(2026, 2, 28)
            });
        }

        private async Task TriggerToggleSidebar()
        {
            if (OnToggleSidebar.HasDelegate)
            {
                await OnToggleSidebar.InvokeAsync();
            }
        }

        // --- Selection Mode Logic ---

        private void OpenSelectionMode()
        {
            IsSelectionMode = true;
            IsSortMenuOpen = false;
            SelectionStep = "List";
            LoadSelectionItems();
        }

        private void CloseSelectionMode()
        {
            IsSelectionMode = false;
            IsSortMenuOpen = false;
            SelectionStep = "List";
            IsCalculatorOpen = false;
        }

        private void SwitchTab(string tab)
        {
            CurrentSelectionTab = tab;
            LoadSelectionItems();
        }

        private void LoadSelectionItems()
        {
            SelectionItems = new List<SelectionItem>();

            if (CurrentSelectionTab == "Service")
            {
                SelectionItems.Add(new SelectionItem { Name = "Skin hydration facial", Price = 89.00m });
                SelectionItems.Add(new SelectionItem { Name = "Skin collagen facial", Price = 129.00m });
                SelectionItems.Add(new SelectionItem { Name = "Antioxidant facial", Price = 99.00m });
                SelectionItems.Add(new SelectionItem { Name = "Signature sparkle hydralaser facial", Price = 219.00m });
                SelectionItems.Add(new SelectionItem { Name = "Antiaging facial", Price = 209.00m });
                SelectionItems.Add(new SelectionItem { Name = "Skin firming", Price = 99.00m });
                SelectionItems.Add(new SelectionItem { Name = "Pigmentation", Price = 159.00m });
                SelectionItems.Add(new SelectionItem { Name = "Skin repair", Price = 119.00m });
            }
            else
            {
                SelectionItems.Add(new SelectionItem { Name = "Facial Cream", Price = 45.00m });
                SelectionItems.Add(new SelectionItem { Name = "Toner", Price = 35.00m });
                SelectionItems.Add(new SelectionItem { Name = "Sunscreen", Price = 55.00m });
            }
        }

        private void ToggleSortMenu()
        {
            IsSortMenuOpen = !IsSortMenuOpen;
        }

        private void SortSelection(string type)
        {
            switch (type)
            {
                case "Name":
                    SelectionItems = SelectionItems.OrderBy(x => x.Name).ToList();
                    break;
                case "LowPrice":
                    SelectionItems = SelectionItems.OrderBy(x => x.Price).ToList();
                    break;
                case "HighPrice":
                    SelectionItems = SelectionItems.OrderByDescending(x => x.Price).ToList();
                    break;
            }
            IsSortMenuOpen = false;
        }

        // --- Add Catalog Logic ---

        private void SelectItemForCatalog(SelectionItem item)
        {
            SelectedItem = item;

            // Pre-fill form
            FormPrice = item.Price;
            FormFolder = "";
            FormCredit = 0;
            FormPoint = 0;
            FormStartDate = DateTime.Today;
            FormEndDate = DateTime.Today.AddDays(7);

            // Pre-fill Advance
            FormDisplayName = item.Name;
            FormDisplaySection = item.Section;
            FormDisplayFolder = "";

            SelectionStep = "Details";
        }

        private void BackToSelectionList()
        {
            SelectionStep = "List";
            IsCalculatorOpen = false;
        }

        private void SaveCatalogItem()
        {
            // Add to main list
            CatalogList.Add(new CatalogItem
            {
                ItemName = SelectedItem.Name,
                Section = SelectedItem.Section,
                Price = FormPrice,
                RedeemableCredits = FormCredit,
                StartDate = FormStartDate,
                EndDate = FormEndDate
            });

            CloseSelectionMode();
        }

        // --- Calculator Logic ---

        private void OpenCalculator(string target)
        {
            CalculatorTarget = target;
            IsCalculatorOpen = true;
            IsCalculatorNewEntry = true;

            // Set initial display based on current value
            if (target == "Price") CalculatorDisplay = FormPrice.ToString("F2");
            else if (target == "Credit") CalculatorDisplay = FormCredit.ToString("F2");
            else if (target == "Point") CalculatorDisplay = FormPoint.ToString();
        }

        private void CalculatorKeyPress(string key)
        {
            if (key == "CLR")
            {
                CalculatorDisplay = "0";
                IsCalculatorNewEntry = true;
                return;
            }

            if (key == ".")
            {
                if (CalculatorTarget == "Point") return; // No decimals for point
                if (CalculatorDisplay.Contains(".")) return; // Already has decimal
                if (IsCalculatorNewEntry) CalculatorDisplay = "0"; // Start with 0. if new
            }

            if (IsCalculatorNewEntry)
            {
                if (key == ".") CalculatorDisplay = "0.";
                else CalculatorDisplay = key;
                IsCalculatorNewEntry = false;
            }
            else
            {
                CalculatorDisplay += key;
            }
        }

        private void ApplyCalculator()
        {
            if (decimal.TryParse(CalculatorDisplay, out decimal result))
            {
                if (CalculatorTarget == "Price") FormPrice = result;
                else if (CalculatorTarget == "Credit") FormCredit = result;
                else if (CalculatorTarget == "Point") FormPoint = (int)result;
            }
            IsCalculatorOpen = false;
        }
    }
}