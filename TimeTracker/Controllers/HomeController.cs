using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Models;
using TimeTracker.ViewModels.Dashboard;

namespace TimeTracker.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);
            var today = DateTime.Today;
            var thisWeek = today.AddDays(-(int)today.DayOfWeek);
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            // Get all time entries for the user that we need for calculations
            var allUserEntries = await _context.TimeEntries
                .Include(te => te.Project)
                .ThenInclude(p => p.Client)
                .Include(te => te.TaskItem)
                .Where(te => te.UserId == userId)
                .ToListAsync();

            // Filter entries in memory to avoid EF Core translation issues
            var todayEntries = allUserEntries
                .Where(te => te.StartTime.Date == today && te.EndTime != null)
                .ToList();

            var thisWeekEntries = allUserEntries
                .Where(te => te.StartTime.Date >= thisWeek && te.EndTime != null)
                .ToList();

            var thisMonthEntries = allUserEntries
                .Where(te => te.StartTime.Date >= thisMonth && te.EndTime != null)
                .ToList();

            // Get running timer
            var runningEntry = allUserEntries
                .Where(te => te.EndTime == null)
                .FirstOrDefault();

            // Get recent entries
            var recentEntries = allUserEntries
                .Where(te => te.EndTime != null)
                .OrderByDescending(te => te.StartTime)
                .Take(10)
                .ToList();

            // Get project statistics for this week
            var projectStats = thisWeekEntries
                .GroupBy(te => new { ClientName = te.Project.Client.Name, ProjectName = te.Project.Name })
                .Select(g => new ProjectStatsViewModel
                {
                    ClientName = g.Key.ClientName,
                    ProjectName = g.Key.ProjectName,
                    TotalHours = g.Sum(te => (te.EndTime!.Value - te.StartTime).TotalHours),
                    BillableHours = g.Where(te => te.IsBillable).Sum(te => (te.EndTime!.Value - te.StartTime).TotalHours)
                })
                .OrderByDescending(ps => ps.TotalHours)
                .Take(5)
                .ToList();

            var dashboardViewModel = new DashboardViewModel
            {
                TodayHours = todayEntries.Sum(te => (te.EndTime!.Value - te.StartTime).TotalHours),
                WeekHours = thisWeekEntries.Sum(te => (te.EndTime!.Value - te.StartTime).TotalHours),
                MonthHours = thisMonthEntries.Sum(te => (te.EndTime!.Value - te.StartTime).TotalHours),
                TodayBillableHours = todayEntries.Where(te => te.IsBillable).Sum(te => (te.EndTime!.Value - te.StartTime).TotalHours),
                WeekBillableHours = thisWeekEntries.Where(te => te.IsBillable).Sum(te => (te.EndTime!.Value - te.StartTime).TotalHours),
                MonthBillableHours = thisMonthEntries.Where(te => te.IsBillable).Sum(te => (te.EndTime!.Value - te.StartTime).TotalHours),
                RunningEntry = runningEntry,
                RecentEntries = recentEntries,
                ProjectStats = projectStats
            };

            return View(dashboardViewModel);
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
