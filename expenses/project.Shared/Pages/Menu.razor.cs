using System;
using Microsoft.AspNetCore.Components;

namespace project.Shared.Pages
{
    public partial class Menu : ComponentBase
    {
        [Inject] public NavigationManager Navigation { get; set; } = default!;

        private string SelectedView { get; set; } = "Product";
        private bool IsSidebarOpen { get; set; } = false;

        private void SelectView(string viewName)
        {
            SelectedView = viewName;
            // Close sidebar on mobile when selecting an item
            if (IsSidebarOpen) IsSidebarOpen = false;
        }

        private void ToggleSidebar()
        {
            IsSidebarOpen = !IsSidebarOpen;
        }

        private void GoToProfile()
        {
            Navigation.NavigateTo("/profile");
        }
    }
}