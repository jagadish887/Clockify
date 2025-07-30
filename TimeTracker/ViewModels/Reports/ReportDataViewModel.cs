using TimeTracker.Models;

namespace TimeTracker.ViewModels.Reports
{
    public class ReportDataViewModel
    {
        public ReportFilterViewModel Filter { get; set; } = new();
        public List<TimeEntry> TimeEntries { get; set; } = new();
        public List<ProjectSummaryViewModel> Summary { get; set; } = new();
        public double TotalHours { get; set; }
        public double BillableHours { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ProjectSummaryViewModel
    {
        public string ClientName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public double TotalHours { get; set; }
        public double BillableHours { get; set; }
        public decimal TotalAmount { get; set; }
    }
}