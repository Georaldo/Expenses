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
    public partial class Discount : ComponentBase
    {
        [Inject] public IJSRuntime JS { get; set; } = default!;

        [Parameter] public string ViewType { get; set; } = "Discount";
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

        // Voucher Selection Popup State
        private bool IsVoucherPopupOpen { get; set; } = false;
        private string VoucherSelectionType { get; set; } = "Service"; // "Service" or "Product"
        private string VoucherSearchQuery { get; set; } = "";

        // NEW: Dropdown State for Unit (kept for potential future reuse)
        private bool IsUnitDropdownOpen { get; set; } = false;

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
        private string FilterRedeemStatus { get; set; } = "All";

        // DELETE FUNCTIONALITY
        private bool IsErrorPopupOpen { get; set; } = false;
        private string ErrorMessage { get; set; } = "";
        private bool IsDeleteConfirmOpen { get; set; } = false;
        private DiscountItem? _itemToDelete;
        private DiscountItem NewItem { get; set; } = new DiscountItem();

        // Section Settings Data
        public class SectionModel
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public string Name { get; set; } = "";
            public int FolderCount { get; set; }
            public int SkuCount { get; set; }
            public string Code { get; set; } = "";
        }

        private bool IsSectionSettingsMode { get; set; } = false;
        private bool IsAddSectionMode { get; set; } = false;
        private bool IsEditSectionMode { get; set; } = false;
        private List<SectionModel> SectionsDb { get; set; } = new List<SectionModel>();
        private SectionModel NewSection { get; set; } = new SectionModel();

        // Mock Categories tailored for Discounts
        private List<string> Categories = new List<string> { "Promos", "Holiday Specials", "Clearance", "Members Only", "Vouchers" };
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

        // Using DiscountItem for Discount component logic
        private List<DiscountItem> MenuDb = new List<DiscountItem>();

        // Mock Data for Services and Products for the Popup
        private List<DiscountItem> MockServices = new List<DiscountItem>();
        private List<DiscountItem> MockProducts = new List<DiscountItem>();

        protected override void OnInitialized()
        {
            // Initial Data for Discounts
            MenuDb.Add(new DiscountItem { ItemType = "Discount", DiscountSection = "Promos", DiscountFolder = "c://image/promo1.png", DisplayImageUrl = "", Station = "Counter", ItemName = "New Year Sale", Price = 50.00m, IsAvailable = true });
            MenuDb.Add(new DiscountItem { ItemType = "Discount", DiscountSection = "Holiday Specials", DiscountFolder = "c://image/xmas_sale.png", DisplayImageUrl = "", Station = "Counter", ItemName = "Xmas Bundle", Price = 99.00m, IsAvailable = true });

            // Mock Sections
            SectionsDb.Add(new SectionModel { Name = "Promos", FolderCount = 1, SkuCount = 5, Code = "#9001" });
            SectionsDb.Add(new SectionModel { Name = "Holiday Specials", FolderCount = 2, SkuCount = 10, Code = "#9002" });

            // Mock Data for Popup (reusing DiscountItem class for simplicity in this mocked context)
            MockServices.Add(new DiscountItem { ItemName = "Hair Cut", DiscountSection = "Hair", Price = 50 });
            MockServices.Add(new DiscountItem { ItemName = "Manicure", DiscountSection = "Nails", Price = 80 });
            MockServices.Add(new DiscountItem { ItemName = "Massage", DiscountSection = "Body", Price = 150 });

            MockProducts.Add(new DiscountItem { ItemName = "Shampoo", DiscountSection = "Care", Price = 25 });
            MockProducts.Add(new DiscountItem { ItemName = "Gel", DiscountSection = "Care", Price = 15 });
            MockProducts.Add(new DiscountItem { ItemName = "Towel", DiscountSection = "Accessories", Price = 10 });
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
            .Select(x => x.DiscountSection)
            .Distinct()
            .OrderBy(x => x);

        private IEnumerable<DiscountItem> FilteredItems
        {
            get
            {
                var query = MenuDb.Where(x => x.ItemType == ViewType);

                if (SelectedSec != "All") query = query.Where(e => e.DiscountSection == SelectedSec);
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

        private IEnumerable<DiscountItem> VoucherFilteredItems
        {
            get
            {
                var list = VoucherSelectionType == "Service" ? MockServices : MockProducts;
                if (!string.IsNullOrEmpty(VoucherSearchQuery))
                {
                    return list.Where(x => x.ItemName.Contains(VoucherSearchQuery, StringComparison.OrdinalIgnoreCase));
                }
                return list;
            }
        }

        private string GetIconForCategory(string category)
        {
            return "<svg class='cat-icon' width='24' height='24' xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'><rect x='2' y='2' width='20' height='20' rx='5' ry='5'></rect><path d='M16 11.37A4 4 0 1 1 12.63 8 4 4 0 0 1 16 11.37z'></path><line x1='17.5' y1='6.5' x2='17.51' y2='6.5'></line></svg>";
        }

        private void OpenAmountPopup(string target, DiscountItem? item = null)
        {
            _calculatorTarget = target;
            decimal val = 0;

            // Map targets to values
            if (target == "Price") val = NewItem.Price;
            else if (target == "DiscountValue") val = NewItem.DiscountValue;

            // Existing mappings
            else if (target == "RedeemPoint") val = NewItem.RedeemPoint;
            else if (target == "LowStockAlert") val = NewItem.LowStockAlert ?? 0;

            // Points, LowStockAlert are Integers
            bool isInt = target == "Points" || target == "LowStockAlert";
            _tempAmountString = isInt ? ((int)val).ToString() : (val == 0 ? "0.00" : val.ToString("0.00"));

            if (target == "Points" && val == 0) _tempAmountString = "0";

            IsAmountPopupOpen = true;
        }

        private void AppendToAmount(string val)
        {
            // Block decimals for Integer fields
            if ((_calculatorTarget == "Points" || _calculatorTarget == "LowStockAlert") && val == ".") return;

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
                else if (_calculatorTarget == "DiscountValue") NewItem.DiscountValue = result;
                else if (_calculatorTarget == "RedeemPoint") NewItem.RedeemPoint = result;
                else if (_calculatorTarget == "LowStockAlert") NewItem.LowStockAlert = (int)result;
            }
            IsAmountPopupOpen = false;
        }

        // --- Voucher Selection Logic ---
        private void OpenVoucherSelection(string type)
        {
            VoucherSelectionType = type;
            VoucherSearchQuery = "";
            IsVoucherPopupOpen = true;
        }

        private void AddVoucherToPackage(DiscountItem voucher)
        {
            if (NewItem.VoucherList.Count >= 10)
            {
                ErrorMessage = "Maximum 10 vouchers allowed per package.";
                IsErrorPopupOpen = true;
                return;
            }

            NewItem.VoucherList.Add($"{VoucherSelectionType}: {voucher.ItemName}");
            // Close popup after selection (optional, keeping it open allows multi-select, closing for now)
            IsVoucherPopupOpen = false;
        }

        private void RemoveVoucher(string voucherString)
        {
            NewItem.VoucherList.Remove(voucherString);
        }

        // --------------------------------

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
            NewSection = new SectionModel { Id = section.Id, Name = section.Name, Code = section.Code, FolderCount = section.FolderCount, SkuCount = section.SkuCount };
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
                    existing.Name = NewSection.Name;
                }
            }
            else { NewSection.Code = "#" + new Random().Next(100000, 999999).ToString(); SectionsDb.Add(NewSection); }
            IsAddSectionMode = false; IsEditSectionMode = false;
        }

        private void OpenAddDiscountMode()
        {
            IsAddMode = true;
            NewItem = new DiscountItem();
            NewItem.ItemType = ViewType;
            ActiveAddTab = "Info";
            IsMinMaxPriceEnabled = false;
            IsRedeemPointEnabled = false;
            SelectedOutletCodes.Clear();
            _tempImagePreview = "";
            foreach (var o in OutletList) SelectedOutletCodes.Add(o.Code);
        }

        private void CloseAddMode() { IsAddMode = false; }

        private void SaveItem()
        {
            if (string.IsNullOrWhiteSpace(NewItem.ItemName) || string.IsNullOrWhiteSpace(NewItem.DiscountSection) || NewItem.DiscountValue <= 0)
            {
                ErrorMessage = "Please fill in the Name, Section, and Discount Value fields.";
                IsErrorPopupOpen = true;
                return;
            }

            var existingItem = MenuDb.FirstOrDefault(x => x.Id == NewItem.Id);
            if (existingItem != null)
            {
                existingItem.ItemName = NewItem.ItemName; existingItem.DiscountSection = NewItem.DiscountSection; existingItem.DiscountFolder = NewItem.DiscountFolder; existingItem.DisplayImageUrl = NewItem.DisplayImageUrl; existingItem.Price = NewItem.Price; existingItem.Duration = NewItem.Duration; existingItem.Barcode = NewItem.Barcode; existingItem.MinPrice = NewItem.MinPrice; existingItem.MaxPrice = NewItem.MaxPrice; existingItem.OutletAvailability = NewItem.OutletAvailability; existingItem.FreePoint = NewItem.FreePoint; existingItem.RedeemPoint = NewItem.RedeemPoint; existingItem.BillOfMaterial = NewItem.BillOfMaterial; existingItem.Policy = NewItem.Policy; existingItem.TermCondition1 = NewItem.TermCondition1; existingItem.TermCondition2 = NewItem.TermCondition2; existingItem.TermCondition3 = NewItem.TermCondition3; existingItem.ImageName = NewItem.ImageName; existingItem.Unit = NewItem.Unit; existingItem.LowStockAlert = NewItem.LowStockAlert;

                // Save new fields
                existingItem.DiscountValue = NewItem.DiscountValue;
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
                NewItem.DiscountFolder = $"c://image/{e.File.Name}";
                NewItem.DisplayImageUrl = _tempImagePreview;
                NewItem.ImageName = e.File.Name;
            }
        }

        private void SelectCategoryFromPopup(string cat) { NewItem.DiscountSection = cat; IsCategoryPopupOpen = false; }
        private async Task DownloadReport(string format) { await JS.InvokeVoidAsync("window.print"); }

        private void RequestDelete(DiscountItem item) { _itemToDelete = item; IsDeleteConfirmOpen = true; }
        private void ConfirmDelete() { if (_itemToDelete != null) MenuDb.Remove(_itemToDelete); _itemToDelete = null; IsDeleteConfirmOpen = false; }

        private void ShowItemDetails(DiscountItem item)
        {
            NewItem = new DiscountItem
            {
                Id = item.Id,
                ItemType = item.ItemType,
                DiscountSection = item.DiscountSection,
                DiscountFolder = item.DiscountFolder,
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
                LowStockAlert = item.LowStockAlert,
                DiscountValue = item.DiscountValue,
                VoucherList = new List<string>(item.VoucherList)
            };
            _tempImagePreview = item.DisplayImageUrl;
            IsMinMaxPriceEnabled = NewItem.MinPrice > 0 || NewItem.MaxPrice > 0;
            IsRedeemPointEnabled = NewItem.RedeemPoint > 0;
            SelectedOutletCodes.Clear();
            foreach (var o in OutletList) SelectedOutletCodes.Add(o.Code);
            IsAddMode = true;
        }

        protected void SelectSec(string section) { SelectedSec = section; }

        // Unit Dropdown logic
        private void ToggleUnitDropdown() => IsUnitDropdownOpen = !IsUnitDropdownOpen;
        private void SelectUnit(string unit) { NewItem.Unit = unit; IsUnitDropdownOpen = false; }
    }
}