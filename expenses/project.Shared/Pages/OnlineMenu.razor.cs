using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using project.Shared.Model;

namespace project.Shared.Pages
{
    public partial class OnlineMenu : ComponentBase
    {
        [Parameter] public EventCallback OnToggleSidebar { get; set; }

        private List<OnlineMenuItem> MenuList = new List<OnlineMenuItem>();
        private string SelectedSection { get; set; } = "All";

        // Advance Popup State
        private bool IsAdvancePopupOpen { get; set; } = false;
        private string MenuDisplayOption { get; set; } = "Selected"; // Default to "Selected item ONLY"

        protected override void OnInitialized()
        {
            MenuList.Add(new OnlineMenuItem { ItemName = "Treatment", Section = "Section 1", Price = 100.00m, Duration = "1 Hour" });
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
            // Logic to filter list would go here
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
            // Logic to save settings
            IsAdvancePopupOpen = false;
        }
    }
}