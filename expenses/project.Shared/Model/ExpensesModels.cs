using System;

namespace project.Shared.Model
{
    public class ExpenditureItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Staff { get; set; }
        public string InvoiceTitle { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; }
        public string FileName { get; set; }
    }

    public class StaffMember
    {
        public string Name { get; set; }
        public string Phone { get; set; }
    }

    public class SubCategoryItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}