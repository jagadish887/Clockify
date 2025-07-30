using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Models
{
    public class Client
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}