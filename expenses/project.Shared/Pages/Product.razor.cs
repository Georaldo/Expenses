using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using project.Shared.Model;

namespace project.Shared.Pages
{
    public partial class Product : ComponentBase
    {
        [Inject] public IJSRuntime JS { get; set; } = default!;

        [Parameter] public string ViewType { get; set; } = "Product";
        [Parameter] public EventCallback OnToggleSidebar { get; set; }

        private string SelectedSec { get; set; } = "All";
        private bool IsFilterVisible { get; set; } = false;
        private bool IsAddMode { get; set; } = false;
        private string ActiveAddTab { get; set; } = "Info";

        // Popup States
        private bool IsCategoryPopupOpen { get; set; } = false;
        private bool IsUploadPopupOpen { get; set; } = false;
        private bool IsAmountPopupOpen { get; set; } = false;
        private bool IsDurationPopupOpen { get; set; } = false;
        private bool IsBarcodePopupOpen { get; set; } = false;
        private bool IsOutletPopupOpen { get; set; } = false;
        private bool IsFilterPopupOpen { get; set; } = false;

        // NEW: Dropdown State for Unit
        private bool IsUnitDropdownOpen { get; set; } = false;

        // Stock In State
        private bool IsStockInMode { get; set; } = false;
        private bool IsStockInProductSelectionOpen { get; set; } = false;
        private string StockInAction { get; set; } = "Stock In";
        private string StockInRemarks { get; set; } = "";
        private string StockInSearchQuery { get; set; } = "";
        private string StockInSortOption { get; set; } = "Name"; // Name, LowPrice, HighPrice
        private bool IsStockInFilterDropdownOpen { get; set; } = false;
        private bool IsStockInSearchVisible { get; set; } = false;
        private string StockInSelectedCategory { get; set; } = "All";
        private ProductItem? _currentStockInItem; // Track item being added to stock

        private List<string> StockInActionList = new List<string> { "Stock In", "Adjustment", "Internal Transferred In", "Recover" };

        // BOM Popup States - kept in code but UI entry removed
        private bool IsBomPopupOpen { get; set; } = false;
        private bool IsAddMaterialPopupOpen { get; set; } = false;

        private List<ProductItem> SelectedBomItems { get; set; } = new List<ProductItem>();
        private List<ProductItem> _bomSnapshot { get; set; } = new List<ProductItem>();

        private string BomSearchQuery { get; set; } = "";
        private string BomSortOption { get; set; } = "NameAsc";
        private bool IsBomSortMenuOpen { get; set; } = false;
        private string BomSectionFilter { get; set; } = "All";

        // Toggles for Advance Tab
        private bool IsMinMaxPriceEnabled { get; set; } = false;
        private bool IsRedeemPointEnabled { get; set; } = false;

        // Calculator Logic
        private string _calculatorTarget { get; set; } = "Price";
        private string _tempAmountString = "0";
        private string _tempImagePreview = "";

        // Filter/Sort variables
        private string FilterSearchQuery { get; set; } = "";
        private string SortOption { get; set; } = "NameAsc";
        private bool FilterShowDisabledOnly { get; set; } = false;
        // Removed FilterShowOnlineOnly
        private string FilterRedeemStatus { get; set; } = "All";

        private bool IsErrorPopupOpen { get; set; } = false;
        private string ErrorMessage { get; set; } = "";
        private bool IsDeleteConfirmOpen { get; set; } = false;
        private ProductItem? _itemToDelete;
        private ProductItem NewItem { get; set; } = new ProductItem();

        // Section Settings Data
        public class SectionModel
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public string Name { get; set; } = "";
            public string AdditionalTitle { get; set; } = "";
            public int FolderCount { get; set; }
            public int SkuCount { get; set; }
            public string Code { get; set; } = "";
            public bool IsCollectionRewardEnabled { get; set; }
            public decimal CreditPerCollection { get; set; }
            public decimal PointPerCollection { get; set; }
            public bool IsReverseRate { get; set; } = false;
        }

        private bool IsSectionSettingsMode { get; set; } = false;
        private bool IsAddSectionMode { get; set; } = false;
        private bool IsEditSectionMode { get; set; } = false;
        private List<SectionModel> SectionsDb { get; set; } = new List<SectionModel>();
        private SectionModel NewSection { get; set; } = new SectionModel();

        private List<string> Categories = new List<string> { "Appetizers", "Mains", "Desserts", "Beverages", "Sides", "Specials" };
        private List<string> UnitList = new List<string> { "centimeter", "gram", "inch", "kati", "kilogram", "liter", "meter", "miligram", "mililiter", "ounce", "piece", "package", "unit" };

        public class OutletItem { public string Name { get; set; } public string Code { get; set; } }
        private List<OutletItem> OutletList = new List<OutletItem>
        {
            new OutletItem { Name = "Main Branch", Code = "#1001" },
            new OutletItem { Name = "City Mall", Code = "#1002" },
            new OutletItem { Name = "West Wing", Code = "#1003" },
            new OutletItem { Name = "Airport Kiosk", Code = "#1004" }
        };
        private HashSet<string> SelectedOutletCodes { get; set; } = new HashSet<string>();

        // Using ProductItem for Product component logic
        private List<ProductItem> MenuDb = new List<ProductItem>();

        protected override void OnInitialized()
        {
            MenuDb.Add(new ProductItem { ItemType = "Product", ProductSection = "Mains", ProductFolder = "c://image/burger.png", DisplayImageUrl = "", Station = "Hot Kitchen", ItemName = "Classic Cheeseburger", Price = 25.00m, IsAvailable = true });
            MenuDb.Add(new ProductItem { ItemType = "Product", ProductSection = "Beverages", ProductFolder = "c://image/cappuccino.png", DisplayImageUrl = "", Station = "Coffee Station", ItemName = "Cappuccino", Price = 12.00m, IsAvailable = true });

            // Mock Sections
            SectionsDb.Add(new SectionModel { Name = "Mains", FolderCount = 2, SkuCount = 15, Code = "#829301" });
            SectionsDb.Add(new SectionModel { Name = "Beverages", FolderCount = 3, SkuCount = 24, Code = "#829302" });
        }

        private async Task TriggerToggleSidebar()
        {
            if (OnToggleSidebar.HasDelegate)
            {
                await OnToggleSidebar.InvokeAsync();
            }
        }

        private IEnumerable<string> AvailableSecs => MenuDb
            .Where(x => x.ItemType == ViewType)
            .Select(x => x.ProductSection)
            .Distinct()
            .OrderBy(x => x);

        private IEnumerable<ProductItem> FilteredItems
        {
            get
            {
                var query = MenuDb.Where(x => x.ItemType == ViewType);

                if (SelectedSec != "All") query = query.Where(e => e.ProductSection == SelectedSec);
                if (!string.IsNullOrEmpty(FilterSearchQuery)) query = query.Where(e => e.ItemName.Contains(FilterSearchQuery, StringComparison.OrdinalIgnoreCase));
                if (FilterShowDisabledOnly) query = query.Where(e => !e.IsAvailable);
                // Removed Show Online Only Logic
                if (FilterRedeemStatus == "Redeemable") query = query.Where(e => e.RedeemPoint > 0);
                if (FilterRedeemStatus == "Not Redeemable") query = query.Where(e => e.RedeemPoint <= 0);

                return SortOption switch
                {
                    "NameAsc" => query.OrderBy(e => e.ItemName),
                    "NameDesc" => query.OrderByDescending(e => e.ItemName),
                    "PriceAsc" => query.OrderBy(e => e.Price),
                    "PriceDesc" => query.OrderByDescending(e => e.Price),
                    _ => query.OrderBy(e => e.ItemName)
                };
            }
        }

        // NEW: Filtered items for Stock In Popup
        private IEnumerable<ProductItem> StockInFilteredItems
        {
            get
            {
                var query = MenuDb.Where(x => x.ItemType == ViewType);

                if (StockInSelectedCategory != "All") query = query.Where(e => e.ProductSection == StockInSelectedCategory);
                if (!string.IsNullOrEmpty(StockInSearchQuery)) query = query.Where(e => e.ItemName.Contains(StockInSearchQuery, StringComparison.OrdinalIgnoreCase));

                return StockInSortOption switch
                {
                    "Name" => query.OrderBy(e => e.ItemName),
                    "Low Price" => query.OrderBy(e => e.Price),
                    "High Price" => query.OrderByDescending(e => e.Price),
                    _ => query.OrderBy(e => e.ItemName)
                };
            }
        }

        private string GetIconForCategory(string category)
        {
            return "<svg class='cat-icon' width='24' height='24' xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'><circle cx='12' cy='12' r='10'></circle></svg>";
        }

        private void OpenAmountPopup(string target, ProductItem? item = null)
        {
            _calculatorTarget = target;
            decimal val = 0;
            if (target == "Price") val = NewItem.Price;
            else if (target == "FreePoint") val = NewItem.FreePoint;
            else if (target == "RedeemPoint") val = NewItem.RedeemPoint;
            else if (target == "MinPrice") val = NewItem.MinPrice;
            else if (target == "MaxPrice") val = NewItem.MaxPrice;
            else if (target == "CreditReward") val = NewSection.CreditPerCollection;
            else if (target == "PointReward") val = NewSection.PointPerCollection;
            else if (target == "LowStockAlert") val = NewItem.LowStockAlert ?? 0;
            else if (target == "StockInQty")
            {
                _currentStockInItem = item;
                val = 0; // Default to 0 or 1 for new stock entry
            }

            _tempAmountString = (target == "LowStockAlert" || target == "StockInQty") ? ((int)val).ToString() : (val == 0 ? "0.00" : val.ToString("0.00"));
            if (target == "StockInQty" && val == 0) _tempAmountString = "0"; // Ensure clean start for qty

            IsAmountPopupOpen = true;
        }

        private void AppendToAmount(string val)
        {
            // Block decimal point for LowStockAlert AND StockInQty
            if ((_calculatorTarget == "LowStockAlert" || _calculatorTarget == "StockInQty") && val == ".") return;

            if (_tempAmountString == "0.00" || _tempAmountString == "0") _tempAmountString = val;
            else if (!(val == "." && _tempAmountString.Contains("."))) _tempAmountString += val;
        }

        private void ClearAmount() => _tempAmountString = "0";
        private void BackspaceAmount() => _tempAmountString = _tempAmountString.Length > 1 ? _tempAmountString.Substring(0, _tempAmountString.Length - 1) : "0";

        private void SaveAmount()
        {
            if (decimal.TryParse(_tempAmountString, out decimal result))
            {
                if (_calculatorTarget == "Price") NewItem.Price = result;
                else if (_calculatorTarget == "FreePoint") NewItem.FreePoint = result;
                else if (_calculatorTarget == "RedeemPoint") NewItem.RedeemPoint = result;
                else if (_calculatorTarget == "MinPrice") NewItem.MinPrice = result;
                else if (_calculatorTarget == "MaxPrice") NewItem.MaxPrice = result;
                else if (_calculatorTarget == "CreditReward") NewSection.CreditPerCollection = result;
                else if (_calculatorTarget == "PointReward") NewSection.PointPerCollection = result;
                else if (_calculatorTarget == "LowStockAlert") NewItem.LowStockAlert = (int)result;
                else if (_calculatorTarget == "StockInQty")
                {
                    // Logic to add stock for _currentStockInItem would go here
                    // For now, we just close, but this is where you'd update a list of items to stock in
                    // e.g. StockInItems.Add(new StockEntry { Item = _currentStockInItem, Qty = (int)result });
                    IsStockInProductSelectionOpen = false; // Close selection after adding
                }
            }
            IsAmountPopupOpen = false;
        }

        private void ToggleOutletSelection(string code)
        {
            if (SelectedOutletCodes.Contains(code)) SelectedOutletCodes.Remove(code);
            else SelectedOutletCodes.Add(code);
            NewItem.OutletAvailability = SelectedOutletCodes.Any();
        }

        private void ToggleMinMaxPrice()
        {
            IsMinMaxPriceEnabled = !IsMinMaxPriceEnabled;
            if (!IsMinMaxPriceEnabled) { NewItem.MinPrice = 0; NewItem.MaxPrice = 0; }
        }

        private void ToggleRedeemPoint()
        {
            IsRedeemPointEnabled = !IsRedeemPointEnabled;
            if (!IsRedeemPointEnabled) NewItem.RedeemPoint = 0;
        }

        private void OpenDurationPopup() => IsDurationPopupOpen = true;
        private void SelectDuration(int? minutes)
        {
            NewItem.Duration = minutes;
            IsDurationPopupOpen = false;
        }

        private void ToggleFilterVisibility() { IsFilterPopupOpen = true; }
        private void ResetFilters() { FilterShowDisabledOnly = false; FilterRedeemStatus = "All"; FilterSearchQuery = ""; SortOption = "NameAsc"; }

        private void OpenSectionSettings() { IsSectionSettingsMode = true; IsAddSectionMode = false; IsEditSectionMode = false; }
        private void GoBackToMain() { IsSectionSettingsMode = false; IsAddSectionMode = false; IsEditSectionMode = false; }

        private void OpenAddSectionMode() { NewSection = new SectionModel(); IsAddSectionMode = true; IsEditSectionMode = false; }
        private void OpenEditSectionMode(SectionModel section)
        {
            NewSection = new SectionModel { Id = section.Id, Name = section.Name, AdditionalTitle = section.AdditionalTitle, Code = section.Code, IsCollectionRewardEnabled = section.IsCollectionRewardEnabled, CreditPerCollection = section.CreditPerCollection, PointPerCollection = section.PointPerCollection, IsReverseRate = section.IsReverseRate, FolderCount = section.FolderCount, SkuCount = section.SkuCount };
            IsAddSectionMode = true; IsEditSectionMode = true;
        }
        private void CloseAddSectionMode() { IsAddSectionMode = false; IsEditSectionMode = false; }

        private void SaveSection()
        {
            if (IsEditSectionMode)
            {
                var existing = SectionsDb.FirstOrDefault(x => x.Id == NewSection.Id);
                if (existing != null)
                {
                    existing.Name = NewSection.Name; existing.AdditionalTitle = NewSection.AdditionalTitle; existing.IsCollectionRewardEnabled = NewSection.IsCollectionRewardEnabled; existing.CreditPerCollection = NewSection.CreditPerCollection; existing.PointPerCollection = NewSection.PointPerCollection; existing.IsReverseRate = NewSection.IsReverseRate;
                }
            }
            else { NewSection.Code = "#" + new Random().Next(100000, 999999).ToString(); SectionsDb.Add(NewSection); }
            IsAddSectionMode = false; IsEditSectionMode = false;
        }

        private void ToggleCollectionReward() { NewSection.IsCollectionRewardEnabled = !NewSection.IsCollectionRewardEnabled; }
        private void ToggleReverseRate() { NewSection.IsReverseRate = !NewSection.IsReverseRate; }

        private void OpenAddProductMode()
        {
            IsAddMode = true;
            NewItem = new ProductItem();
            NewItem.ItemType = ViewType;
            ActiveAddTab = "Info";
            IsMinMaxPriceEnabled = false;
            IsRedeemPointEnabled = false;
            SelectedOutletCodes.Clear();
            SelectedBomItems.Clear();
            _tempImagePreview = "";
            foreach (var o in OutletList) SelectedOutletCodes.Add(o.Code);
        }

        private void CloseAddMode() { IsAddMode = false; }

        // NEW: Stock In Methods
        private void OpenStockInMode()
        {
            IsStockInMode = true;
            StockInAction = "Stock In";
            StockInRemarks = "";
            StockInSearchQuery = "";
            StockInSortOption = "Name";
            StockInSelectedCategory = "All";
            IsStockInProductSelectionOpen = false;
        }

        private void CloseStockInMode()
        {
            IsStockInMode = false;
        }

        private void OpenStockInProductSelection()
        {
            IsStockInProductSelectionOpen = true;
        }

        private void CloseStockInProductSelection()
        {
            IsStockInProductSelectionOpen = false;
        }

        private void ToggleStockInFilter()
        {
            IsStockInFilterDropdownOpen = !IsStockInFilterDropdownOpen;
        }

        private void SelectStockInSort(string option)
        {
            StockInSortOption = option;
            IsStockInFilterDropdownOpen = false;
        }

        private void ToggleStockInSearch()
        {
            IsStockInSearchVisible = !IsStockInSearchVisible;
            if (!IsStockInSearchVisible) StockInSearchQuery = "";
        }

        private void SelectStockInCategory(string category)
        {
            StockInSelectedCategory = category;
        }

        private void SaveStockIn()
        {
            // Implementation for saving stock in record
            IsStockInMode = false;
        }

        private void SaveItem()
        {
            if (string.IsNullOrWhiteSpace(NewItem.ItemName) || string.IsNullOrWhiteSpace(NewItem.ProductSection) || NewItem.Price <= 0)
            {
                ErrorMessage = "Please fill in the Name, Section, and Price fields.";
                IsErrorPopupOpen = true;
                return;
            }

            var existingItem = MenuDb.FirstOrDefault(x => x.Id == NewItem.Id);
            if (existingItem != null)
            {
                existingItem.ItemName = NewItem.ItemName; existingItem.ProductSection = NewItem.ProductSection; existingItem.ProductFolder = NewItem.ProductFolder; existingItem.DisplayImageUrl = NewItem.DisplayImageUrl; existingItem.Price = NewItem.Price; existingItem.Duration = NewItem.Duration; existingItem.Barcode = NewItem.Barcode; existingItem.MinPrice = NewItem.MinPrice; existingItem.MaxPrice = NewItem.MaxPrice; existingItem.OutletAvailability = NewItem.OutletAvailability; existingItem.FreePoint = NewItem.FreePoint; existingItem.RedeemPoint = NewItem.RedeemPoint; existingItem.BillOfMaterial = NewItem.BillOfMaterial; existingItem.Policy = NewItem.Policy; existingItem.TermCondition1 = NewItem.TermCondition1; existingItem.TermCondition2 = NewItem.TermCondition2; existingItem.TermCondition3 = NewItem.TermCondition3; existingItem.ImageName = NewItem.ImageName; existingItem.Unit = NewItem.Unit; existingItem.LowStockAlert = NewItem.LowStockAlert;
            }
            else
            {
                if (string.IsNullOrEmpty(NewItem.Station)) NewItem.Station = "Unassigned";
                if (string.IsNullOrEmpty(NewItem.DisplayImageUrl) && !string.IsNullOrEmpty(_tempImagePreview)) NewItem.DisplayImageUrl = _tempImagePreview;
                MenuDb.Add(NewItem);
            }
            IsAddMode = false;
        }

        private void CloseErrorPopup() => IsErrorPopupOpen = false;

        private async Task HandleFileSelected(InputFileChangeEventArgs e)
        {
            IsUploadPopupOpen = false;
            if (e.File != null)
            {
                var format = "image/png";
                var resizedImage = await e.File.RequestImageFileAsync(format, 400, 400);
                var buffer = new byte[resizedImage.Size];
                await resizedImage.OpenReadStream().ReadAsync(buffer);
                _tempImagePreview = $"data:{format};base64,{Convert.ToBase64String(buffer)}";
                NewItem.ProductFolder = $"c://image/{e.File.Name}";
                NewItem.DisplayImageUrl = _tempImagePreview;
                NewItem.ImageName = e.File.Name;
            }
        }

        private void SelectCategoryFromPopup(string cat) { NewItem.ProductSection = cat; IsCategoryPopupOpen = false; }
        private async Task DownloadReport(string format) { await JS.InvokeVoidAsync("window.print"); }

        private void RequestDelete(ProductItem item) { _itemToDelete = item; IsDeleteConfirmOpen = true; }
        private void ConfirmDelete() { if (_itemToDelete != null) MenuDb.Remove(_itemToDelete); _itemToDelete = null; IsDeleteConfirmOpen = false; }

        private void ShowItemDetails(ProductItem item)
        {
            NewItem = new ProductItem
            {
                Id = item.Id,
                ItemType = item.ItemType,
                ProductSection = item.ProductSection,
                ProductFolder = item.ProductFolder,
                DisplayImageUrl = item.DisplayImageUrl,
                Station = item.Station,
                ItemName = item.ItemName,
                Price = item.Price,
                Duration = item.Duration,
                Barcode = item.Barcode,
                FreePoint = item.FreePoint,
                RedeemPoint = item.RedeemPoint,
                BillOfMaterial = item.BillOfMaterial,
                Policy = item.Policy,
                TermCondition1 = item.TermCondition1,
                TermCondition2 = item.TermCondition2,
                TermCondition3 = item.TermCondition3,
                ImageName = item.ImageName,
                Unit = item.Unit,
                LowStockAlert = item.LowStockAlert
            };
            _tempImagePreview = item.DisplayImageUrl;
            IsMinMaxPriceEnabled = NewItem.MinPrice > 0 || NewItem.MaxPrice > 0;
            IsRedeemPointEnabled = NewItem.RedeemPoint > 0;
            SelectedOutletCodes.Clear();
            foreach (var o in OutletList) SelectedOutletCodes.Add(o.Code);
            SelectedBomItems.Clear();
            if (!string.IsNullOrEmpty(item.BillOfMaterial))
            {
                var names = item.BillOfMaterial.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                foreach (var name in names) { var found = MenuDb.FirstOrDefault(x => x.ItemName == name); if (found != null) SelectedBomItems.Add(found); }
            }
            IsAddMode = true;
        }

        protected void SelectSec(string section) { SelectedSec = section; }

        private void OpenBomPopup()
        {
            SelectedBomItems.Clear();
            if (!string.IsNullOrEmpty(NewItem.BillOfMaterial))
            {
                var names = NewItem.BillOfMaterial.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                foreach (var name in names) { var found = MenuDb.FirstOrDefault(x => x.ItemName == name); if (found != null) SelectedBomItems.Add(found); }
            }
            IsBomPopupOpen = true;
        }

        private void CloseBomPopup(bool save) { if (save) UpdateBomString(); IsBomPopupOpen = false; }
        private void OpenAddMaterialPopup() { _bomSnapshot = new List<ProductItem>(SelectedBomItems); BomSearchQuery = ""; BomSectionFilter = "All"; BomSortOption = "NameAsc"; IsBomSortMenuOpen = false; IsAddMaterialPopupOpen = true; }
        private void CloseAddMaterialPopup(bool save) { if (!save) SelectedBomItems = new List<ProductItem>(_bomSnapshot); IsAddMaterialPopupOpen = false; }
        private void AddToBom(ProductItem item) { if (!SelectedBomItems.Any(x => x.Id == item.Id)) SelectedBomItems.Add(item); }
        private void RemoveFromBom(ProductItem item) { SelectedBomItems.Remove(item); }
        private void UpdateBomString() { NewItem.BillOfMaterial = string.Join(", ", SelectedBomItems.Select(x => x.ItemName)); }
        private void ToggleBomSortMenu() { IsBomSortMenuOpen = !IsBomSortMenuOpen; }
        private void SelectBomSort(string sort) { BomSortOption = sort; IsBomSortMenuOpen = false; }
        private void SelectBomSection(string section) { BomSectionFilter = section; }

        private IEnumerable<ProductItem> GetFilteredBomMaterials()
        {
            var query = MenuDb.AsEnumerable();
            if (BomSectionFilter != "All") query = query.Where(x => x.ProductSection == BomSectionFilter);
            if (!string.IsNullOrEmpty(BomSearchQuery)) query = query.Where(x => x.ItemName.Contains(BomSearchQuery, StringComparison.OrdinalIgnoreCase));
            return BomSortOption switch { "NameAsc" => query.OrderBy(x => x.ItemName), "PriceAsc" => query.OrderBy(x => x.Price), "PriceDesc" => query.OrderByDescending(x => x.Price), _ => query.OrderBy(x => x.ItemName) };
        }

        // Unit Dropdown logic
        private void ToggleUnitDropdown() => IsUnitDropdownOpen = !IsUnitDropdownOpen;
        private void SelectUnit(string unit) { NewItem.Unit = unit; IsUnitDropdownOpen = false; }
    }
}