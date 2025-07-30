using System.ComponentModel.DataAnnotations;

namespace TimeTracker.ViewModels.Reports
{
    public class ReportFilterViewModel
    {
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; } = DateTime.Now.AddDays(-30);

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; } = DateTime.Now;

        [Display(Name = "User")]
        public string? UserId { get; set; }

        [Display(Name = "Client")]
        public int? ClientId { get; set; }

        [Display(Name = "Project")]
        public int? ProjectId { get; set; }

        [Display(Name = "Billable")]
        public bool? IsBillable { get; set; }

        [Display(Name = "Group By")]
        public string GroupBy { get; set; } = "Date";

        public List<string> GroupByOptions => new List<string> 
        { 
            "Date", "User", "Project", "Client", "Task" 
        };
    }
}