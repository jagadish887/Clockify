using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public bool IsCompleted { get; set; } = false;
        
        public DateTime? CompletedDate { get; set; }
        
        public int EstimatedHours { get; set; } = 0;
        
        public string? CreatedByUserId { get; set; }
        public AppUser? CreatedByUser { get; set; }
        
        public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    }
}