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
    public partial class Menu : ComponentBase
    {
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;

        private string SelectedView { get; set; } = "Product";
        private string SelectedSec { get; set; } = "All";
        private bool IsSidebarOpen { get; set; } = false;
        private bool IsFilterVisible { get; set; } = false;

        private bool IsAddMode { get; set; } = false;
        private string ActiveAddTab { get; set; } = "Info";

        // Popup States
        private bool IsCategoryPopupOpen { get; set; } = false;
        private bool IsSubCategoryPopupOpen { get; set; } = false;
        private bool IsUploadPopupOpen { get; set; } = false;
        private bool IsAmountPopupOpen { get; set; } = false;
        private bool IsDurationPopupOpen { get; set; } = false;
        private bool IsBarcodePopupOpen { get; set; } = false;
        private bool IsOutletPopupOpen { get; set; } = false;
        private bool IsFilterPopupOpen { get; set; } = false;

        // BOM Popup States
        private bool IsBomPopupOpen { get; set; } = false;
        private bool IsAddMaterialPopupOpen { get; set; } = false;

        private List<ServiceItem> SelectedBomItems { get; set; } = new List<ServiceItem>();
        private List<ServiceItem> _bomSnapshot { get; set; } = new List<ServiceItem>();

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
        private string _tempImagePreview = ""; // Still used for immediate preview before save

        // Filter/Sort variables
        private string FilterSearchQuery { get; set; } = "";
        private string FilterStation { get; set; } = "";
        private string SortOption { get; set; } = "NameAsc";

        // NEW FILTER VARIABLES
        private bool FilterShowDisabledOnly { get; set; } = false;
        private bool FilterShowOnlineOnly { get; set; } = false;
        private string FilterRedeemStatus { get; set; } = "All";

        private bool IsStationPopupOpen { get; set; } = false;
        private bool IsFilterStationPopupOpen { get; set; } = false;
        private bool IsDownloadPopupOpen { get; set; } = false;
        private bool IsDeleteConfirmOpen { get; set; } = false;
        private bool IsErrorPopupOpen { get; set; } = false;
        private string ErrorMessage { get; set; } = "";
        private string _stationSearchQuery { get; set; } = "";
        private ServiceItem? _itemToDelete;
        private bool IsModalOpen { get; set; } = false;

        private ServiceItem NewItem { get; set; } = new ServiceItem();

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
        private Dictionary<string, List<MenuSubCategoryItem>> CategoryData = new Dictionary<string, List<MenuSubCategoryItem>>
        {
            { "Appetizers", new List<MenuSubCategoryItem> { new MenuSubCategoryItem { Name = "Salads", Description = "Fresh garden salads" }, new MenuSubCategoryItem { Name = "Soups", Description = "Warm soups" } } },
            { "Mains", new List<MenuSubCategoryItem> { new MenuSubCategoryItem { Name = "Burgers", Description = "All burgers" }, new MenuSubCategoryItem { Name = "Pasta", Description = "Italian pasta" } } },
            { "Desserts", new List<MenuSubCategoryItem> { new MenuSubCategoryItem { Name = "Cakes", Description = "Whole cakes" }, new MenuSubCategoryItem { Name = "Ice Cream", Description = "Flavors" } } },
            { "Beverages", new List<MenuSubCategoryItem> { new MenuSubCategoryItem { Name = "Coffee", Description = "Espresso based" }, new MenuSubCategoryItem { Name = "Tea", Description = "Herbal" } } },
            { "Sides", new List<MenuSubCategoryItem> { new MenuSubCategoryItem { Name = "Fries", Description = "Potato" }, new MenuSubCategoryItem { Name = "Bread", Description = "Toast" } } },
            { "Specials", new List<MenuSubCategoryItem> { new MenuSubCategoryItem { Name = "Seasonal", Description = "Offers" }, new MenuSubCategoryItem { Name = "Chef's Choice", Description = "Recommendations" } } }
        };

        private List<StationItem> StationList = new List<StationItem>
        {
            new StationItem { Name = "Hot Kitchen", Type = "Back of House" },
            new StationItem { Name = "Cold Kitchen", Type = "Back of House" },
            new StationItem { Name = "Bakery", Type = "Back of House" },
            new StationItem { Name = "Bar", Type = "Front of House" },
            new StationItem { Name = "Coffee Station", Type = "Front of House" }
        };

        public class OutletItem { public string Name { get; set; } public string Code { get; set; } }
        private List<OutletItem> OutletList = new List<OutletItem>
        {
            new OutletItem { Name = "Main Branch", Code = "#1001" },
            new OutletItem { Name = "City Mall", Code = "#1002" },
            new OutletItem { Name = "West Wing", Code = "#1003" },
            new OutletItem { Name = "Airport Kiosk", Code = "#1004" }
        };
        private HashSet<string> SelectedOutletCodes { get; set; } = new HashSet<string>();

        private List<ServiceItem> MenuDb = new List<ServiceItem>();

        protected override void OnInitialized()
        {
            MenuDb.Add(new ServiceItem { ItemType = "Product", ServiceSection = "Mains", ServiceFolder = "c://image/burger.png", DisplayImageUrl = "", Station = "Hot Kitchen", ItemName = "Classic Cheeseburger", Price = 25.00m, IsAvailable = true });
            MenuDb.Add(new ServiceItem { ItemType = "Product", ServiceSection = "Beverages", ServiceFolder = "c://image/cappuccino.png", DisplayImageUrl = "", Station = "Coffee Station", ItemName = "Cappuccino", Price = 12.00m, IsAvailable = true });

            // Mock Sections
            SectionsDb.Add(new SectionModel { Name = "Mains", FolderCount = 2, SkuCount = 15, Code = "#829301" });
            SectionsDb.Add(new SectionModel { Name = "Beverages", FolderCount = 3, SkuCount = 24, Code = "#829302" });
        }

        private IEnumerable<string> AvailableSecs => MenuDb
            .Where(x => x.ItemType == SelectedView)
            .Select(x => x.ServiceSection)
            .Distinct()
            .OrderBy(x => x);

        private IEnumerable<ServiceItem> FilteredItems
        {
            get
            {
                var query = MenuDb.Where(x => x.ItemType == SelectedView);

                if (SelectedSec != "All") query = query.Where(e => e.ServiceSection == SelectedSec);
                if (!string.IsNullOrEmpty(FilterStation)) query = query.Where(e => e.Station == FilterStation);
                if (!string.IsNullOrEmpty(FilterSearchQuery)) query = query.Where(e => e.ItemName.Contains(FilterSearchQuery, StringComparison.OrdinalIgnoreCase));

                if (FilterShowDisabledOnly) query = query.Where(e => !e.IsAvailable);

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

        private IEnumerable<StationItem> FilteredStations => string.IsNullOrWhiteSpace(_stationSearchQuery) ? StationList : StationList.Where(s => s.Name.Contains(_stationSearchQuery, StringComparison.OrdinalIgnoreCase) || s.Type.Contains(_stationSearchQuery, StringComparison.OrdinalIgnoreCase));

        private string GetIconForCategory(string category)
        {
            return "<svg class='cat-icon' width='24' height='24' xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'><circle cx='12' cy='12' r='10'></circle></svg>";
        }

        // Calculator Logic
        private void OpenAmountPopup(string target)
        {
            _calculatorTarget = target;
            decimal val = 0;
            if (target == "Price") val = NewItem.Price;
            else if (target == "FreePoint") val = NewItem.FreePoint;
            else if (target == "RedeemPoint") val = NewItem.RedeemPoint;
            else if (target == "MinPrice") val = NewItem.MinPrice;
            else if (target == "MaxPrice") val = NewItem.MaxPrice;
            // Section Rewards
            else if (target == "CreditReward") val = NewSection.CreditPerCollection;
            else if (target == "PointReward") val = NewSection.PointPerCollection;

            _tempAmountString = val == 0 ? "0.00" : val.ToString("0.00");
            IsAmountPopupOpen = true;
        }

        private void AppendToAmount(string val)
        {
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
            }
            IsAmountPopupOpen = false;
        }

        private void ToggleOutletSelection(string code)
        {
            if (SelectedOutletCodes.Contains(code))
                SelectedOutletCodes.Remove(code);
            else
                SelectedOutletCodes.Add(code);

            NewItem.OutletAvailability = SelectedOutletCodes.Any();
        }

        private void ToggleMinMaxPrice()
        {
            IsMinMaxPriceEnabled = !IsMinMaxPriceEnabled;
            if (!IsMinMaxPriceEnabled)
            {
                NewItem.MinPrice = 0;
                NewItem.MaxPrice = 0;
            }
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

        private void SelectView(string viewName)
        {
            SelectedView = viewName;
            IsAddMode = false;
            IsSectionSettingsMode = false;
        }
        private void ToggleSidebar() { IsSidebarOpen = !IsSidebarOpen; }

        private void ToggleFilterVisibility()
        {
            IsFilterPopupOpen = true;
        }

        private void ResetFilters()
        {
            FilterShowDisabledOnly = false;
            FilterShowOnlineOnly = false;
            FilterRedeemStatus = "All";
            FilterSearchQuery = "";
            FilterStation = "";
            SortOption = "NameAsc";
        }

        private void OpenSectionSettings()
        {
            IsSectionSettingsMode = true;
            IsAddSectionMode = false;
            IsEditSectionMode = false;
        }

        private void CloseSectionSettings()
        {
            IsSectionSettingsMode = false;
            IsAddSectionMode = false;
            IsEditSectionMode = false;
        }

        private void OpenAddSectionMode()
        {
            NewSection = new SectionModel();
            IsAddSectionMode = true;
            IsEditSectionMode = false;
        }

        private void OpenEditSectionMode(SectionModel section)
        {
            NewSection = new SectionModel
            {
                Id = section.Id,
                Name = section.Name,
                AdditionalTitle = section.AdditionalTitle,
                Code = section.Code,
                IsCollectionRewardEnabled = section.IsCollectionRewardEnabled,
                CreditPerCollection = section.CreditPerCollection,
                PointPerCollection = section.PointPerCollection,
                IsReverseRate = section.IsReverseRate,
                FolderCount = section.FolderCount,
                SkuCount = section.SkuCount
            };
            IsAddSectionMode = true;
            IsEditSectionMode = true;
        }

        private void CloseAddSectionMode()
        {
            IsAddSectionMode = false;
            IsEditSectionMode = false;
        }

        private void SaveSection()
        {
            if (IsEditSectionMode)
            {
                var existing = SectionsDb.FirstOrDefault(x => x.Id == NewSection.Id);
                if (existing != null)
                {
                    existing.Name = NewSection.Name;
                    existing.AdditionalTitle = NewSection.AdditionalTitle;
                    existing.IsCollectionRewardEnabled = NewSection.IsCollectionRewardEnabled;
                    existing.CreditPerCollection = NewSection.CreditPerCollection;
                    existing.PointPerCollection = NewSection.PointPerCollection;
                    existing.IsReverseRate = NewSection.IsReverseRate;
                }
            }
            else
            {
                NewSection.Code = "#" + new Random().Next(100000, 999999).ToString();
                SectionsDb.Add(NewSection);
            }
            IsAddSectionMode = false;
            IsEditSectionMode = false;
        }

        private void ToggleCollectionReward()
        {
            NewSection.IsCollectionRewardEnabled = !NewSection.IsCollectionRewardEnabled;
        }

        private void ToggleReverseRate()
        {
            NewSection.IsReverseRate = !NewSection.IsReverseRate;
        }

        private void OpenAddServiceMode()
        {
            IsAddMode = true;
            NewItem = new ServiceItem();
            // Set current type to match selected view
            NewItem.ItemType = SelectedView;
            ActiveAddTab = "Info";
            IsMinMaxPriceEnabled = false;
            IsRedeemPointEnabled = false;
            SelectedOutletCodes.Clear();
            SelectedBomItems.Clear();
            _tempImagePreview = ""; // Clear preview
            foreach (var o in OutletList) SelectedOutletCodes.Add(o.Code);
        }

        private void CloseAddMode() { IsAddMode = false; }

        // UPDATED: Save Item Logic
        private void SaveItem()
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(NewItem.ItemName) ||
                string.IsNullOrWhiteSpace(NewItem.ServiceSection) ||
                NewItem.Price <= 0) // Assuming price must be > 0
            {
                ErrorMessage = "Please fill in the Name, Section, and Price fields.";
                IsErrorPopupOpen = true;
                return;
            }

            var existingItem = MenuDb.FirstOrDefault(x => x.Id == NewItem.Id);
            if (existingItem != null)
            {
                existingItem.ItemName = NewItem.ItemName;
                existingItem.ServiceSection = NewItem.ServiceSection;
                existingItem.ServiceFolder = NewItem.ServiceFolder;
                existingItem.DisplayImageUrl = NewItem.DisplayImageUrl;
                existingItem.Price = NewItem.Price;
                existingItem.Duration = NewItem.Duration;
                existingItem.Barcode = NewItem.Barcode;
                existingItem.MinPrice = NewItem.MinPrice;
                existingItem.MaxPrice = NewItem.MaxPrice;
                existingItem.OutletAvailability = NewItem.OutletAvailability;
                existingItem.FreePoint = NewItem.FreePoint;
                existingItem.RedeemPoint = NewItem.RedeemPoint;
                existingItem.BillOfMaterial = NewItem.BillOfMaterial;
                existingItem.Policy = NewItem.Policy;
                existingItem.TermCondition1 = NewItem.TermCondition1;
                existingItem.TermCondition2 = NewItem.TermCondition2;
                existingItem.TermCondition3 = NewItem.TermCondition3;
                existingItem.ImageName = NewItem.ImageName;
            }
            else
            {
                if (string.IsNullOrEmpty(NewItem.Station)) NewItem.Station = "Unassigned";
                if (string.IsNullOrEmpty(NewItem.DisplayImageUrl) && !string.IsNullOrEmpty(_tempImagePreview))
                {
                    NewItem.DisplayImageUrl = _tempImagePreview;
                }

                MenuDb.Add(NewItem);
            }

            IsAddMode = false;
        }

        private void CloseModal() => IsModalOpen = false;
        private void CloseErrorPopup() => IsErrorPopupOpen = false;

        private async Task HandleFileSelected(InputFileChangeEventArgs e)
        {
            IsUploadPopupOpen = false;
            if (e.File != null)
            {
                // Convert to Base64 for display preview
                var format = "image/png";
                var resizedImage = await e.File.RequestImageFileAsync(format, 400, 400);
                var buffer = new byte[resizedImage.Size];
                await resizedImage.OpenReadStream().ReadAsync(buffer);
                _tempImagePreview = $"data:{format};base64,{Convert.ToBase64String(buffer)}";

                // Store path in ServiceFolder as requested by user text requirement
                NewItem.ServiceFolder = $"c://image/{e.File.Name}";
                // Store actual image data in DisplayImageUrl for rendering
                NewItem.DisplayImageUrl = _tempImagePreview;
                NewItem.ImageName = e.File.Name;
            }
        }

        private void OpenStationPopup() { _stationSearchQuery = ""; IsStationPopupOpen = true; }
        private void SelectStation(string name) { NewItem.Station = name; IsStationPopupOpen = false; }

        private void SelectCategoryFromPopup(string cat)
        {
            NewItem.ServiceSection = cat;
            IsCategoryPopupOpen = false;
        }

        private void OpenSubCategoryPopup() => IsSubCategoryPopupOpen = true;
        private void SelectSubCategory(string sub)
        {
            NewItem.ServiceFolder = sub;
            IsSubCategoryPopupOpen = false;
        }

        private void OpenFilterStationPopup() { _stationSearchQuery = ""; IsFilterStationPopupOpen = true; }
        private void SelectStationFilter(string name) { FilterStation = name; IsFilterStationPopupOpen = false; }
        private async Task DownloadReport(string format) { await JS.InvokeVoidAsync("window.print"); IsDownloadPopupOpen = false; }

        private void GoToProfile() { Navigation.NavigateTo("/profile"); }
        private void RequestDelete(ServiceItem item) { _itemToDelete = item; IsDeleteConfirmOpen = true; }
        private void ConfirmDelete() { if (_itemToDelete != null) MenuDb.Remove(_itemToDelete); _itemToDelete = null; IsDeleteConfirmOpen = false; }
        private void CancelDelete() { _itemToDelete = null; IsDeleteConfirmOpen = false; }

        private void ShowItemDetails(ServiceItem item)
        {
            NewItem = new ServiceItem
            {
                Id = item.Id,
                ItemType = item.ItemType,
                ServiceSection = item.ServiceSection,
                ServiceFolder = item.ServiceFolder,
                DisplayImageUrl = item.DisplayImageUrl, // Copy display URL
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
                ImageName = item.ImageName
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
                foreach (var name in names)
                {
                    var found = MenuDb.FirstOrDefault(x => x.ItemName == name);
                    if (found != null) SelectedBomItems.Add(found);
                }
            }

            IsAddMode = true;
        }

        protected void SelectSec(string section)
        {
            SelectedSec = section;
        }

        private void OpenBomPopup()
        {
            SelectedBomItems.Clear();
            if (!string.IsNullOrEmpty(NewItem.BillOfMaterial))
            {
                var names = NewItem.BillOfMaterial.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                foreach (var name in names)
                {
                    var found = MenuDb.FirstOrDefault(x => x.ItemName == name);
                    if (found != null) SelectedBomItems.Add(found);
                }
            }
            IsBomPopupOpen = true;
        }

        private void CloseBomPopup(bool save)
        {
            if (save)
            {
                UpdateBomString();
            }
            IsBomPopupOpen = false;
        }

        private void OpenAddMaterialPopup()
        {
            _bomSnapshot = new List<ServiceItem>(SelectedBomItems);

            BomSearchQuery = "";
            BomSectionFilter = "All";
            BomSortOption = "NameAsc";
            IsBomSortMenuOpen = false;
            IsAddMaterialPopupOpen = true;
        }

        private void CloseAddMaterialPopup(bool save)
        {
            if (!save)
            {
                SelectedBomItems = new List<ServiceItem>(_bomSnapshot);
            }
            IsAddMaterialPopupOpen = false;
        }

        private void AddToBom(ServiceItem item)
        {
            if (!SelectedBomItems.Any(x => x.Id == item.Id))
            {
                SelectedBomItems.Add(item);
            }
        }

        private void RemoveFromBom(ServiceItem item)
        {
            SelectedBomItems.Remove(item);
        }

        private void UpdateBomString()
        {
            NewItem.BillOfMaterial = string.Join(", ", SelectedBomItems.Select(x => x.ItemName));
        }

        private void ToggleBomSortMenu()
        {
            IsBomSortMenuOpen = !IsBomSortMenuOpen;
        }

        private void SelectBomSort(string sort)
        {
            BomSortOption = sort;
            IsBomSortMenuOpen = false;
        }

        private void SelectBomSection(string section)
        {
            BomSectionFilter = section;
        }

        private IEnumerable<ServiceItem> GetFilteredBomMaterials()
        {
            var query = MenuDb.AsEnumerable();

            if (BomSectionFilter != "All")
            {
                query = query.Where(x => x.ServiceSection == BomSectionFilter);
            }

            if (!string.IsNullOrEmpty(BomSearchQuery))
            {
                query = query.Where(x => x.ItemName.Contains(BomSearchQuery, StringComparison.OrdinalIgnoreCase));
            }

            return BomSortOption switch
            {
                "NameAsc" => query.OrderBy(x => x.ItemName),
                "PriceAsc" => query.OrderBy(x => x.Price),
                "PriceDesc" => query.OrderByDescending(x => x.Price),
                _ => query.OrderBy(x => x.ItemName)
            };
        }
    }
}