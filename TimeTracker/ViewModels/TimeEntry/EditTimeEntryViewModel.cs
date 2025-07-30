using System.ComponentModel.DataAnnotations;

namespace TimeTracker.ViewModels.TimeEntry
{
    public class EditTimeEntryViewModel
    {
        public int Id { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Display(Name = "End Time")]
        public DateTime? EndTime { get; set; }

        [Required]
        [Display(Name = "Project")]
        public int ProjectId { get; set; }

        [Display(Name = "Task")]
        public int? TaskItemId { get; set; }

        [Display(Name = "Billable")]
        public bool IsBillable { get; set; }

        [Range(0, 10000)]
        [Display(Name = "Hourly Rate")]
        public decimal? HourlyRate { get; set; }
    }
}