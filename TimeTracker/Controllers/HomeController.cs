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

            // Get dashboard statistics
            var todayEntries = await _context.TimeEntries
                .Where(te => te.UserId == userId && te.StartTime.Date == today && te.EndTime != null)
                .ToListAsync();

            var thisWeekEntries = await _context.TimeEntries
                .Where(te => te.UserId == userId && te.StartTime.Date >= thisWeek && te.EndTime != null)
                .ToListAsync();

            var thisMonthEntries = await _context.TimeEntries
                .Where(te => te.UserId == userId && te.StartTime.Date >= thisMonth && te.EndTime != null)
                .ToListAsync();

            // Get running timer
            var runningEntry = await _context.TimeEntries
                .Include(te => te.Project)
                .Include(te => te.TaskItem)
                .Where(te => te.UserId == userId && te.EndTime == null)
                .FirstOrDefaultAsync();

            // Get recent entries
            var recentEntries = await _context.TimeEntries
                .Include(te => te.Project)
                .ThenInclude(p => p.Client)
                .Include(te => te.TaskItem)
                .Where(te => te.UserId == userId && te.EndTime != null)
                .OrderByDescending(te => te.StartTime)
                .Take(10)
                .ToListAsync();

            // Get project statistics
            var projectStats = await _context.TimeEntries
                .Include(te => te.Project)
                .ThenInclude(p => p.Client)
                .Where(te => te.UserId == userId && te.StartTime.Date >= thisWeek && te.EndTime != null)
                .GroupBy(te => new { te.Project.Client.Name, te.Project.Name })
                .Select(g => new ProjectStatsViewModel
                {
                    ClientName = g.Key.Name,
                    ProjectName = g.Key.Name,
                    TotalHours = g.Sum(te => te.DurationHours),
                    BillableHours = g.Where(te => te.IsBillable).Sum(te => te.DurationHours)
                })
                .OrderByDescending(ps => ps.TotalHours)
                .Take(5)
                .ToListAsync();

            var dashboardViewModel = new DashboardViewModel
            {
                TodayHours = todayEntries.Sum(te => te.DurationHours),
                WeekHours = thisWeekEntries.Sum(te => te.DurationHours),
                MonthHours = thisMonthEntries.Sum(te => te.DurationHours),
                TodayBillableHours = todayEntries.Where(te => te.IsBillable).Sum(te => te.DurationHours),
                WeekBillableHours = thisWeekEntries.Where(te => te.IsBillable).Sum(te => te.DurationHours),
                MonthBillableHours = thisMonthEntries.Where(te => te.IsBillable).Sum(te => te.DurationHours),
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
