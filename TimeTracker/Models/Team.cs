using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Models
{
    public class Team
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public string? ManagerId { get; set; }
        public AppUser? Manager { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        public ICollection<UserTeam> UserTeams { get; set; } = new List<UserTeam>();
    }
}