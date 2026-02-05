using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using project.Shared.Model;

namespace project.Shared.Pages
{
    public partial class Menu : ComponentBase
    {
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;

        // --- State ---
        private string SelectedView { get; set; } = "Product"; // Default view
        private string SelectedSec { get; set; } = "All"; // Renamed from SelectedSection

        // Sidebar State
        private bool IsSidebarOpen { get; set; } = false;

        // Filter Visibility Toggle
        private bool IsFilterVisible { get; set; } = false;

        // Filter States
        private string FilterSearchQuery { get; set; } = "";
        private string FilterCategory { get; set; } = "";
        private string FilterStation { get; set; } = "";
        private string SortOption { get; set; } = "NameAsc";

        // Pop-up states
        private bool IsModalOpen { get; set; } = false;
        private bool IsUploadPopupOpen { get; set; } = false;
        private bool IsAmountPopupOpen { get; set; } = false;
        private bool IsStationPopupOpen { get; set; } = false;
        private bool IsFilterStationPopupOpen { get; set; } = false;
        private bool IsCategoryPopupOpen { get; set; } = false;
        private bool IsSubCategoryPopupOpen { get; set; } = false;
        private bool IsDownloadPopupOpen { get; set; } = false;
        private bool IsDeleteConfirmOpen { get; set; } = false;

        // Error Popup State
        private bool IsErrorPopupOpen { get; set; } = false;
        private string ErrorMessage { get; set; } = "";

        // Search & Filter State
        private string _tempAmountString = "0";
        private string _stationSearchQuery = "";
        private MenuItem? _itemToDelete;

        // --- Data Models ---
        private MenuItem NewItem { get; set; } = new MenuItem();

        // Helper property to check if we are editing an existing item
        private bool IsEditing => MenuDb.Any(e => e.Id == NewItem.Id);

        // Main Categories
        private List<string> Categories = new List<string>
        {
            "Appetizers", "Mains", "Desserts", "Beverages", "Sides", "Specials"
        };

        // Sub-Category Definitions
        private Dictionary<string, List<MenuSubCategoryItem>> CategoryData = new Dictionary<string, List<MenuSubCategoryItem>>
        {
            { "Appetizers", new List<MenuSubCategoryItem> { new MenuSubCategoryItem { Name = "Salads", Description = "Fresh garden salads" }, new MenuSubCategoryItem { Name = "Soups", Description = "Warm soups and broths" }, new MenuSubCategoryItem { Name = "Finger Food", Description = "Easy to eat snacks" } } },
            { "Mains", new List<MenuSubCategoryItem> { new MenuSubCategoryItem { Name = "Burgers", Description = "Beef, Chicken, and Vegan burgers" }, new MenuSubCategoryItem { Name = "Pasta", Description = "Italian classic pasta dishes" }, new MenuSubCategoryItem { Name = "Steak", Description = "Premium cuts" }, new MenuSubCategoryItem { Name = "Rice Dishes", Description = "Asian and local favorites" } } },
            { "Desserts", new List<MenuSubCategoryItem> { new MenuSubCategoryItem { Name = "Cakes", Description = "Slices and whole cakes" }, new MenuSubCategoryItem { Name = "Ice Cream", Description = "Various flavors" }, new MenuSubCategoryItem { Name = "Pastries", Description = "Freshly baked goods" } } },
            { "Beverages", new List<MenuSubCategoryItem> { new MenuSubCategoryItem { Name = "Coffee", Description = "Espresso based drinks" }, new MenuSubCategoryItem { Name = "Tea", Description = "Herbal and black teas" }, new MenuSubCategoryItem { Name = "Juice", Description = "Fresh pressed juices" }, new MenuSubCategoryItem { Name = "Soft Drinks", Description = "Carbonated sodas" } } },
            { "Sides", new List<MenuSubCategoryItem> { new MenuSubCategoryItem { Name = "Fries", Description = "Potato and Sweet Potato" }, new MenuSubCategoryItem { Name = "Bread", Description = "Garlic bread and toast" } } },
            { "Specials", new List<MenuSubCategoryItem> { new MenuSubCategoryItem { Name = "Seasonal", Description = "Limited time offers" }, new MenuSubCategoryItem { Name = "Chef's Choice", Description = "Daily recommendations" } } }
        };

        private List<StationItem> StationList = new List<StationItem>
        {
            new StationItem { Name = "Hot Kitchen", Type = "Back of House" },
            new StationItem { Name = "Cold Kitchen", Type = "Back of House" },
            new StationItem { Name = "Bakery", Type = "Back of House" },
            new StationItem { Name = "Bar", Type = "Front of House" },
            new StationItem { Name = "Coffee Station", Type = "Front of House" }
        };

        // Hardcoded Database of Menu Items
        private List<MenuItem> MenuDb = new List<MenuItem>();

        protected override void OnInitialized()
        {
            // Product Items
            MenuDb.Add(new MenuItem { ItemType = "Product", MenuCategory = "Mains", SubCategory = "Burgers", Station = "Hot Kitchen", ItemName = "Classic Cheeseburger", Price = 25.00m, Description = "Beef patty, cheddar, lettuce, tomato", IsAvailable = true });
            MenuDb.Add(new MenuItem { ItemType = "Product", MenuCategory = "Beverages", SubCategory = "Coffee", Station = "Coffee Station", ItemName = "Cappuccino", Price = 12.00m, Description = "Double shot espresso with milk foam", IsAvailable = true });
            MenuDb.Add(new MenuItem { ItemType = "Product", MenuCategory = "Desserts", SubCategory = "Cakes", Station = "Bakery", ItemName = "Chocolate Lava Cake", Price = 18.00m, Description = "Served with vanilla ice cream", IsAvailable = false });
            MenuDb.Add(new MenuItem { ItemType = "Product", MenuCategory = "Appetizers", SubCategory = "Salads", Station = "Cold Kitchen", ItemName = "Caesar Salad", Price = 16.50m, Description = "Romaine lettuce, croutons, parmesan", IsAvailable = true });

            // Service Items (Dummy data)
            MenuDb.Add(new MenuItem { ItemType = "Service", MenuCategory = "Specials", SubCategory = "", Station = "Bar", ItemName = "Table Service", Price = 0.00m, Description = "Standard table service", IsAvailable = true });

            // Package Items (Dummy data)
            MenuDb.Add(new MenuItem { ItemType = "Package", MenuCategory = "Mains", SubCategory = "", Station = "Hot Kitchen", ItemName = "Lunch Set A", Price = 35.00m, Description = "Burger + Drink + Fries", IsAvailable = true });
        }

        // Logic to get distinct sections (categories) available for the current View
        // Renamed AvailableSections -> AvailableSecs
        private IEnumerable<string> AvailableSecs => MenuDb
            .Where(x => x.ItemType == SelectedView)
            .Select(x => x.MenuCategory)
            .Distinct()
            .OrderBy(x => x);

        // Filter Logic
        private IEnumerable<MenuItem> FilteredItems
        {
            get
            {
                var query = MenuDb.Where(x => x.ItemType == SelectedView);

                // Filter by Section Button (All vs Specific Category)
                // Renamed SelectedSection -> SelectedSec
                if (SelectedSec != "All")
                {
                    query = query.Where(e => e.MenuCategory == SelectedSec);
                }

                // Apply Dropdown Filters
                if (!string.IsNullOrEmpty(FilterStation))
                    query = query.Where(e => e.Station == FilterStation);

                if (!string.IsNullOrEmpty(FilterSearchQuery))
                    query = query.Where(e => e.ItemName.Contains(FilterSearchQuery, StringComparison.OrdinalIgnoreCase) || (e.Description != null && e.Description.Contains(FilterSearchQuery, StringComparison.OrdinalIgnoreCase)));

                // Apply Sort
                switch (SortOption)
                {
                    case "NameAsc": return query.OrderBy(e => e.ItemName);
                    case "NameDesc": return query.OrderByDescending(e => e.ItemName);
                    case "PriceAsc": return query.OrderBy(e => e.Price);
                    case "PriceDesc": return query.OrderByDescending(e => e.Price);
                    default: return query.OrderBy(e => e.ItemName);
                }
            }
        }

        // Filtered Station Logic
        private IEnumerable<StationItem> FilteredStations => string.IsNullOrWhiteSpace(_stationSearchQuery)
            ? StationList
            : StationList.Where(s => s.Name.Contains(_stationSearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                   s.Type.Contains(_stationSearchQuery, StringComparison.OrdinalIgnoreCase));

        // --- HELPER: Icons ---
        private string GetIconForCategory(string category)
        {
            string path = "";
            switch (category)
            {
                case "Appetizers": path = "<path d='M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5'></path>"; break;
                case "Mains": path = "<path d='M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z'></path>"; break;
                case "Desserts": path = "<path d='M12 21a9 9 0 0 0 9-9c0-3.87-2.61-7.14-6.19-8.49a1 1 0 0 0-1.11.41l-1.7 2.55-1.7-2.55a1 1 0 0 0-1.11-.41C3.61 4.86 1 8.13 1 12a9 9 0 0 0 9 9z'></path>"; break;
                case "Beverages": path = "<path d='M18 8h1a4 4 0 0 1 0 8h-1'></path><path d='M2 8h16v9a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4V8z'></path><line x1='6' y1='1' x2='6' y2='4'></line><line x1='10' y1='1' x2='10' y2='4'></line><line x1='14' y1='1' x2='14' y2='4'></line>"; break;
                case "Sides": path = "<circle cx='12' cy='12' r='10'></circle><circle cx='12' cy='12' r='3'></circle>"; break;
                case "Specials": path = "<polygon points='12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2'></polygon>"; break;
                default: path = "<circle cx='12' cy='12' r='10'></circle>"; break;
            }
            return $"<svg class='cat-icon' xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'>{path}</svg>";
        }

        // --- Main Actions ---
        private void SelectView(string viewName)
        {
            SelectedView = viewName;
            SelectedSec = "All"; // Reset sec filter when switching views
            if (IsSidebarOpen) IsSidebarOpen = false;
        }

        // Renamed SelectSection -> SelectSec
        private void SelectSec(string sec)
        {
            SelectedSec = sec;
        }

        private void ToggleFilterVisibility()
        {
            IsFilterVisible = !IsFilterVisible;
        }

        private void ToggleSidebar()
        {
            IsSidebarOpen = !IsSidebarOpen;
        }

        private void OpenAddModal()
        {
            NewItem = new MenuItem
            {
                ItemType = SelectedView,
                MenuCategory = Categories[0],
                SubCategory = "",
                IsAvailable = true
            };
            IsModalOpen = true;
        }

        private void CloseModal()
        {
            IsModalOpen = false;
            IsUploadPopupOpen = false;
            IsAmountPopupOpen = false;
            IsStationPopupOpen = false;
            IsCategoryPopupOpen = false;
            IsSubCategoryPopupOpen = false;
            IsDownloadPopupOpen = false;
            IsFilterStationPopupOpen = false;
            IsDeleteConfirmOpen = false;
            IsErrorPopupOpen = false;
        }

        private void CloseErrorPopup()
        {
            IsErrorPopupOpen = false;
        }

        private void SaveItem()
        {
            if (string.IsNullOrWhiteSpace(NewItem.Station))
            {
                ErrorMessage = "Please select a station.";
                IsErrorPopupOpen = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(NewItem.ItemName))
            {
                ErrorMessage = "Please enter an item name.";
                IsErrorPopupOpen = true;
                return;
            }

            if (NewItem.Price < 0)
            {
                ErrorMessage = "Please enter a valid price.";
                IsErrorPopupOpen = true;
                return;
            }

            var existingItem = MenuDb.FirstOrDefault(e => e.Id == NewItem.Id);

            if (existingItem != null)
            {
                existingItem.ItemType = NewItem.ItemType;
                existingItem.MenuCategory = NewItem.MenuCategory;
                existingItem.SubCategory = NewItem.SubCategory;
                existingItem.Station = NewItem.Station;
                existingItem.ItemName = NewItem.ItemName;
                existingItem.Price = NewItem.Price;
                existingItem.Description = NewItem.Description;
                existingItem.ImageName = NewItem.ImageName;
                existingItem.IsAvailable = NewItem.IsAvailable;
            }
            else
            {
                MenuDb.Add(NewItem);
            }

            IsModalOpen = false;
        }

        private async Task HandleFileSelected(InputFileChangeEventArgs e)
        {
            IsUploadPopupOpen = false;
            var file = e.File;
            if (file != null)
            {
                NewItem.ImageName = file.Name;
            }
            await Task.CompletedTask;
        }

        // --- Keypad/Popup Logic ---

        private void OpenAmountPopup()
        {
            _tempAmountString = NewItem.Price == 0 ? "0" : NewItem.Price.ToString("0.##");
            IsAmountPopupOpen = true;
        }

        private void AppendToAmount(string val)
        {
            if (_tempAmountString == "0" && val != ".") _tempAmountString = val;
            else
            {
                if (val == "." && _tempAmountString.Contains(".")) return;
                _tempAmountString += val;
            }
        }

        private void BackspaceAmount()
        {
            if (_tempAmountString.Length > 0) _tempAmountString = _tempAmountString.Substring(0, _tempAmountString.Length - 1);
            if (string.IsNullOrEmpty(_tempAmountString)) _tempAmountString = "0";
        }

        private void ClearAmount()
        {
            _tempAmountString = "0";
        }

        private void SaveAmount()
        {
            if (decimal.TryParse(_tempAmountString, out decimal result)) NewItem.Price = result;
            IsAmountPopupOpen = false;
        }

        private void OpenStationPopup()
        {
            _stationSearchQuery = "";
            IsStationPopupOpen = true;
        }

        private void SelectStation(string stationName)
        {
            NewItem.Station = stationName;
            IsStationPopupOpen = false;
        }

        private void SelectCategoryFromPopup(string category)
        {
            if (NewItem.MenuCategory != category)
            {
                NewItem.MenuCategory = category;
                NewItem.SubCategory = "";
            }
            IsCategoryPopupOpen = false;
            IsSubCategoryPopupOpen = true;
        }

        private void OpenSubCategoryPopup()
        {
            IsSubCategoryPopupOpen = true;
        }

        private void SelectSubCategory(string subCategory)
        {
            NewItem.SubCategory = subCategory;
            IsSubCategoryPopupOpen = false;
        }

        private async Task DownloadReport(string format)
        {
            if (format == "pdf")
            {
                await JS.InvokeVoidAsync("window.print");
            }
            IsDownloadPopupOpen = false;
        }

        private void OpenFilterStationPopup()
        {
            _stationSearchQuery = "";
            IsFilterStationPopupOpen = true;
        }

        private void SelectStationFilter(string stationName)
        {
            FilterStation = stationName;
            IsFilterStationPopupOpen = false;
        }

        private void GoToProfile()
        {
            Navigation.NavigateTo("/profile");
        }

        private void RequestDelete(MenuItem item)
        {
            _itemToDelete = item;
            IsDeleteConfirmOpen = true;
        }

        private void ConfirmDelete()
        {
            if (_itemToDelete != null)
            {
                MenuDb.Remove(_itemToDelete);
                _itemToDelete = null;
            }
            IsDeleteConfirmOpen = false;
        }

        private void CancelDelete()
        {
            _itemToDelete = null;
            IsDeleteConfirmOpen = false;
        }

        private void ShowItemDetails(MenuItem item)
        {
            NewItem = new MenuItem
            {
                Id = item.Id,
                ItemType = item.ItemType,
                MenuCategory = item.MenuCategory,
                SubCategory = item.SubCategory,
                IsAvailable = item.IsAvailable,
                Station = item.Station,
                ItemName = item.ItemName,
                Price = item.Price,
                Description = item.Description,
                ImageName = item.ImageName
            };
            IsModalOpen = true;
        }
    }
}