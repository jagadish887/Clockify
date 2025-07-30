namespace TimeTracker.Models
{
    public class ProjectUser
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;
        
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        
        public string? Role { get; set; } // Manager, Developer, etc.
    }
}