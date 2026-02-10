using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using project.Shared.Model;

namespace project.Shared.Pages
{
    public partial class CustomMenu : ComponentBase
    {
        [Parameter] public EventCallback OnToggleSidebar { get; set; }

        private bool IsAddMode { get; set; } = false;
        private List<CustomMenuItem> MenuList = new List<CustomMenuItem>();
        private CustomMenuItemModel NewMenu { get; set; } = new CustomMenuItemModel();

        // Helper model for the form
        public class CustomMenuItemModel
        {
            public string Name { get; set; } = "";

            // Menu Items
            public bool ShowService { get; set; } = true;
            public bool ShowProduct { get; set; } = true;
            public bool ShowPackage { get; set; } = true;
            public bool ShowDiscount { get; set; } = true;

            // Menu Rules
            public string ApplicableMember { get; set; } = "All";
            public List<string> ApplicableDays { get; set; } = new List<string>();

            public bool IsEverydayDate { get; set; } = false;
            public DateTime StartDate { get; set; } = DateTime.Today;
            public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);

            public bool IsAllDayTime { get; set; } = false;
            public DateTime StartTime { get; set; } = DateTime.Today.AddHours(9); // 9:00 AM
            public DateTime EndTime { get; set; } = DateTime.Today.AddHours(18); // 6:00 PM

            public string Priority { get; set; } = "Medium";

            public bool RestrictToEligible { get; set; } = false;
            public bool IsEnabled { get; set; } = true;
        }

        private List<string> DaysOfWeek = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        protected override void OnInitialized()
        {
            // Mock Data
            MenuList.Add(new CustomMenuItem { Name = "jjjnk", Code = "#27252" });
        }

        private async Task TriggerToggleSidebar()
        {
            if (OnToggleSidebar.HasDelegate)
            {
                await OnToggleSidebar.InvokeAsync();
            }
        }

        private void OpenAddMode()
        {
            IsAddMode = true;
            NewMenu = new CustomMenuItemModel(); // Reset form
            // Default all days selected
            NewMenu.ApplicableDays = new List<string>(DaysOfWeek);
        }

        private void CloseAddMode()
        {
            IsAddMode = false;
        }

        private void SaveCustomMenu()
        {
            // Add mock item to list
            if (!string.IsNullOrWhiteSpace(NewMenu.Name))
            {
                MenuList.Add(new CustomMenuItem
                {
                    Name = NewMenu.Name,
                    Code = "#" + new Random().Next(10000, 99999).ToString()
                });
            }
            IsAddMode = false;
        }

        private void ToggleDay(string day)
        {
            if (NewMenu.ApplicableDays.Contains(day))
            {
                NewMenu.ApplicableDays.Remove(day);
            }
            else
            {
                NewMenu.ApplicableDays.Add(day);
            }
        }

        private void ToggleEveryday(ChangeEventArgs e)
        {
            bool isChecked = (bool)(e.Value ?? false);
            if (isChecked)
            {
                NewMenu.ApplicableDays = new List<string>(DaysOfWeek);
            }
            else
            {
                NewMenu.ApplicableDays.Clear();
            }
        }
    }
}