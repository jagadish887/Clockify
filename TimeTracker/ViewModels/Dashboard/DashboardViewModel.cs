using TimeTracker.Models;

namespace TimeTracker.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public double TodayHours { get; set; }
        public double WeekHours { get; set; }
        public double MonthHours { get; set; }
        public double TodayBillableHours { get; set; }
        public double WeekBillableHours { get; set; }
        public double MonthBillableHours { get; set; }
        public TimeEntry? RunningEntry { get; set; }
        public List<TimeEntry> RecentEntries { get; set; } = new();
        public List<ProjectStatsViewModel> ProjectStats { get; set; } = new();
    }

    public class ProjectStatsViewModel
    {
        public string ClientName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public double TotalHours { get; set; }
        public double BillableHours { get; set; }
    }
}