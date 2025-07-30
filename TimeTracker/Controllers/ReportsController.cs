using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TimeTracker.Data;
using TimeTracker.Models;
using TimeTracker.ViewModels.Reports;

namespace TimeTracker.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ReportsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            await PopulateFilters();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport(ReportFilterViewModel filter)
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");
            var isManager = await _userManager.IsInRoleAsync(user!, "Manager");

            var query = _context.TimeEntries
                .Include(te => te.User)
                .Include(te => te.Project)
                .ThenInclude(p => p.Client)
                .Include(te => te.TaskItem)
                .AsQueryable();

            // Apply date filters
            if (filter.StartDate.HasValue)
                query = query.Where(te => te.StartTime.Date >= filter.StartDate.Value.Date);

            if (filter.EndDate.HasValue)
                query = query.Where(te => te.StartTime.Date <= filter.EndDate.Value.Date);

            // Apply user filter (role-based access)
            if (!isAdmin)
            {
                if (isManager)
                {
                    // Managers can see their team's data (for now, all users - can be enhanced)
                    if (!string.IsNullOrEmpty(filter.UserId))
                        query = query.Where(te => te.UserId == filter.UserId);
                }
                else
                {
                    // Regular users can only see their own data
                    query = query.Where(te => te.UserId == user!.Id);
                }
            }
            else if (!string.IsNullOrEmpty(filter.UserId))
            {
                query = query.Where(te => te.UserId == filter.UserId);
            }

            // Apply project filter
            if (filter.ProjectId.HasValue)
                query = query.Where(te => te.ProjectId == filter.ProjectId.Value);

            // Apply client filter
            if (filter.ClientId.HasValue)
                query = query.Where(te => te.Project.ClientId == filter.ClientId.Value);

            // Apply billable filter
            if (filter.IsBillable.HasValue)
                query = query.Where(te => te.IsBillable == filter.IsBillable.Value);

            var timeEntries = await query
                .Where(te => te.EndTime != null) // Only completed entries
                .OrderByDescending(te => te.StartTime)
                .ToListAsync();

            var reportData = new ReportDataViewModel
            {
                Filter = filter,
                TimeEntries = timeEntries,
                TotalHours = timeEntries.Sum(te => te.DurationHours),
                BillableHours = timeEntries.Where(te => te.IsBillable).Sum(te => te.DurationHours),
                TotalAmount = timeEntries.Where(te => te.IsBillable).Sum(te => te.TotalAmount),
                Summary = timeEntries
                    .GroupBy(te => new { ClientName = te.Project.Client.Name, ProjectName = te.Project.Name })
                    .Select(g => new ProjectSummaryViewModel
                    {
                        ClientName = g.Key.ClientName,
                        ProjectName = g.Key.ProjectName,
                        TotalHours = g.Sum(te => te.DurationHours),
                        BillableHours = g.Where(te => te.IsBillable).Sum(te => te.DurationHours),
                        TotalAmount = g.Where(te => te.IsBillable).Sum(te => te.TotalAmount)
                    })
                    .OrderBy(s => s.ClientName)
                    .ThenBy(s => s.ProjectName)
                    .ToList()
            };

            await PopulateFilters();
            return View("ReportResults", reportData);
        }

        public async Task<IActionResult> ExportToExcel(ReportFilterViewModel filter)
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");
            var isManager = await _userManager.IsInRoleAsync(user!, "Manager");

            var query = _context.TimeEntries
                .Include(te => te.User)
                .Include(te => te.Project)
                .ThenInclude(p => p.Client)
                .Include(te => te.TaskItem)
                .AsQueryable();

            // Apply the same filters as the report
            if (filter.StartDate.HasValue)
                query = query.Where(te => te.StartTime.Date >= filter.StartDate.Value.Date);

            if (filter.EndDate.HasValue)
                query = query.Where(te => te.StartTime.Date <= filter.EndDate.Value.Date);

            if (!isAdmin)
            {
                if (isManager)
                {
                    if (!string.IsNullOrEmpty(filter.UserId))
                        query = query.Where(te => te.UserId == filter.UserId);
                }
                else
                {
                    query = query.Where(te => te.UserId == user!.Id);
                }
            }
            else if (!string.IsNullOrEmpty(filter.UserId))
            {
                query = query.Where(te => te.UserId == filter.UserId);
            }

            if (filter.ProjectId.HasValue)
                query = query.Where(te => te.ProjectId == filter.ProjectId.Value);

            if (filter.ClientId.HasValue)
                query = query.Where(te => te.Project.ClientId == filter.ClientId.Value);

            if (filter.IsBillable.HasValue)
                query = query.Where(te => te.IsBillable == filter.IsBillable.Value);

            var timeEntries = await query
                .Where(te => te.EndTime != null)
                .OrderByDescending(te => te.StartTime)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Time Report");

            // Headers
            worksheet.Cell(1, 1).Value = "Date";
            worksheet.Cell(1, 2).Value = "User";
            worksheet.Cell(1, 3).Value = "Client";
            worksheet.Cell(1, 4).Value = "Project";
            worksheet.Cell(1, 5).Value = "Task";
            worksheet.Cell(1, 6).Value = "Description";
            worksheet.Cell(1, 7).Value = "Start Time";
            worksheet.Cell(1, 8).Value = "End Time";
            worksheet.Cell(1, 9).Value = "Duration (Hours)";
            worksheet.Cell(1, 10).Value = "Billable";
            worksheet.Cell(1, 11).Value = "Hourly Rate";
            worksheet.Cell(1, 12).Value = "Amount";

            // Style headers
            var headerRange = worksheet.Range(1, 1, 1, 12);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Data
            for (int i = 0; i < timeEntries.Count; i++)
            {
                var entry = timeEntries[i];
                var row = i + 2;

                worksheet.Cell(row, 1).Value = entry.StartTime.Date;
                worksheet.Cell(row, 2).Value = entry.User.FullName;
                worksheet.Cell(row, 3).Value = entry.Project.Client.Name;
                worksheet.Cell(row, 4).Value = entry.Project.Name;
                worksheet.Cell(row, 5).Value = entry.TaskItem?.Name ?? "";
                worksheet.Cell(row, 6).Value = entry.Description ?? "";
                worksheet.Cell(row, 7).Value = entry.StartTime;
                worksheet.Cell(row, 8).Value = entry.EndTime;
                worksheet.Cell(row, 9).Value = Math.Round(entry.DurationHours, 2);
                worksheet.Cell(row, 10).Value = entry.IsBillable ? "Yes" : "No";
                worksheet.Cell(row, 11).Value = entry.HourlyRate ?? 0;
                worksheet.Cell(row, 12).Value = entry.IsBillable ? entry.TotalAmount : 0;
            }

            // Auto fit columns
            worksheet.Columns().AdjustToContents();

            // Add summary
            var summaryRow = timeEntries.Count + 3;
            worksheet.Cell(summaryRow, 8).Value = "Total Hours:";
            worksheet.Cell(summaryRow, 9).Value = Math.Round(timeEntries.Sum(te => te.DurationHours), 2);
            worksheet.Cell(summaryRow + 1, 8).Value = "Billable Hours:";
            worksheet.Cell(summaryRow + 1, 9).Value = Math.Round(timeEntries.Where(te => te.IsBillable).Sum(te => te.DurationHours), 2);
            worksheet.Cell(summaryRow + 2, 8).Value = "Total Amount:";
            worksheet.Cell(summaryRow + 2, 9).Value = timeEntries.Where(te => te.IsBillable).Sum(te => te.TotalAmount);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            var fileName = $"TimeReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private async Task PopulateFilters()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");

            // Users dropdown (only for admin/manager)
            if (isAdmin)
            {
                var users = await _userManager.Users
                    .OrderBy(u => u.FullName)
                    .ToListAsync();
                ViewBag.Users = new SelectList(users, "Id", "FullName");
            }

            // Clients dropdown
            var clients = await _context.Clients
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
            ViewBag.Clients = new SelectList(clients, "Id", "Name");

            // Projects dropdown
            var projects = await _context.Projects
                .Include(p => p.Client)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Client.Name)
                .ThenBy(p => p.Name)
                .ToListAsync();
            ViewBag.Projects = new SelectList(projects.Select(p => new { 
                Id = p.Id, 
                Name = $"{p.Client.Name} - {p.Name}" 
            }), "Id", "Name");
        }
    }
}