using System.ComponentModel.DataAnnotations;

namespace TimeTracker.ViewModels.TimeEntry
{
    public class StartTimerViewModel
    {
        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Project")]
        public int ProjectId { get; set; }

        [Display(Name = "Task")]
        public int? TaskItemId { get; set; }
    }
}