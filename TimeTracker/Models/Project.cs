using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Models
{
    public class Project
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;
        
        public bool IsBillable { get; set; } = true;
        
        [Range(0, 10000)]
        public decimal HourlyRate { get; set; } = 0;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        public string? CreatedByUserId { get; set; }
        public AppUser? CreatedByUser { get; set; }
        
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
        public ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
    }
}