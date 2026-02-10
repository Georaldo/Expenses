using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using project.Shared.Model;

namespace project.Shared.Pages
{
    public partial class Catalog : ComponentBase
    {
        [Parameter] public EventCallback OnToggleSidebar { get; set; }

        private List<CatalogItem> CatalogList = new List<CatalogItem>();

        protected override void OnInitialized()
        {
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
    }
}