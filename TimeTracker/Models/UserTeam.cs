namespace TimeTracker.Models
{
    public class UserTeam
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;
        
        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;
        
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
    }
}