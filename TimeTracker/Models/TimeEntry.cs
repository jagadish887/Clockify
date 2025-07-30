using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Models
{
    public class TimeEntry
    {
        public int Id { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        public TimeSpan Duration 
        { 
            get 
            { 
                if (EndTime.HasValue)
                    return EndTime.Value - StartTime;
                return TimeSpan.Zero;
            } 
        }
        
        public double DurationHours => Duration.TotalHours;
        
        public bool IsBillable { get; set; } = true;
        
        [Range(0, 10000)]
        public decimal? HourlyRate { get; set; }
        
        public decimal TotalAmount => (decimal)DurationHours * (HourlyRate ?? 0);
        
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        
        public int? TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? ModifiedDate { get; set; }
        
        public bool IsRunning => !EndTime.HasValue;
    }
}