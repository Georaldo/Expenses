using System;
using System.Collections.Generic;
using System.Linq; // Added for Where/ToList
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using project.Shared.Model;

namespace project.Shared.Pages
{
    public partial class OnlineMenu : ComponentBase
    {
        [Parameter] public EventCallback OnToggleSidebar { get; set; }

        // Extended model to support selection state locally
        public class OnlineMenuItemViewModel : OnlineMenuItem
        {
            public bool IsSelected { get; set; }
        }

        private List<OnlineMenuItemViewModel> AllItems = new List<OnlineMenuItemViewModel>();
        private List<OnlineMenuItemViewModel> MenuList => AllItems.Where(x => x.IsSelected).ToList();

        private string SelectedSection { get; set; } = "All";
        private bool IsEditMode { get; set; } = false;

        // Advance Popup State
        private bool IsAdvancePopupOpen { get; set; } = false;
        private string MenuDisplayOption { get; set; } = "Selected";

        protected override void OnInitialized()
        {
            // Initialize with some data, some selected, some not
            AllItems.Add(new OnlineMenuItemViewModel { ItemName = "Treatment", Section = "Section 1", Price = 100.00m, Duration = "1 Hour", IsSelected = true });
            AllItems.Add(new OnlineMenuItemViewModel { ItemName = "Pico Laser", Section = "Section 1", Price = 99.00m, Duration = "45 Min", IsSelected = true });
            AllItems.Add(new OnlineMenuItemViewModel { ItemName = "Hydration Facial", Section = "Section 2", Price = 89.00m, Duration = "1 Hour", IsSelected = false }); // Not selected initially
            AllItems.Add(new OnlineMenuItemViewModel { ItemName = "Massage", Section = "Section 2", Price = 150.00m, Duration = "1.5 Hour", IsSelected = false });
        }

        private async Task TriggerToggleSidebar()
        {
            if (OnToggleSidebar.HasDelegate)
            {
                await OnToggleSidebar.InvokeAsync();
            }
        }

        private void SelectSection(string section)
        {
            SelectedSection = section;
        }

        private void OpenAdvancePopup()
        {
            IsAdvancePopupOpen = true;
        }

        private void CloseAdvancePopup()
        {
            IsAdvancePopupOpen = false;
        }

        private void SaveAdvanceSettings()
        {
            IsAdvancePopupOpen = false;
        }

        private void ToggleEditMode()
        {
            IsEditMode = !IsEditMode;
        }

        private void SaveEditMode()
        {
            IsEditMode = false;
            // Logic to persist changes would go here
        }

        private void ToggleItemSelection(OnlineMenuItemViewModel item)
        {
            if (IsEditMode)
            {
                item.IsSelected = !item.IsSelected;
            }
        }
    }
}