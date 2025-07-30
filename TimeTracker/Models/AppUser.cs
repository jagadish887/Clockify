using Microsoft.AspNetCore.Identity;

namespace TimeTracker.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<UserTeam> UserTeams { get; set; } = new List<UserTeam>();
    }
}