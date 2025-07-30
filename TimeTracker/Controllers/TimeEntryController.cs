using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Models;
using TimeTracker.ViewModels.TimeEntry;

namespace TimeTracker.Controllers
{
    [Authorize]
    public class TimeEntryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public TimeEntryController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var timeEntries = await _context.TimeEntries
                .Include(te => te.Project)
                .ThenInclude(p => p.Client)
                .Include(te => te.TaskItem)
                .Where(te => te.UserId == userId)
                .OrderByDescending(te => te.StartTime)
                .Take(50)
                .ToListAsync();

            return View(timeEntries);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTimeEntryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var project = await _context.Projects.FindAsync(model.ProjectId);
                
                var timeEntry = new TimeEntry
                {
                    Description = model.Description,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    ProjectId = model.ProjectId,
                    TaskItemId = model.TaskItemId == 0 ? null : model.TaskItemId,
                    UserId = userId!,
                    IsBillable = model.IsBillable,
                    HourlyRate = model.HourlyRate ?? project?.HourlyRate
                };

                _context.TimeEntries.Add(timeEntry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var timeEntry = await _context.TimeEntries
                .Where(te => te.Id == id && te.UserId == userId)
                .FirstOrDefaultAsync();

            if (timeEntry == null) return NotFound();

            var model = new EditTimeEntryViewModel
            {
                Id = timeEntry.Id,
                Description = timeEntry.Description,
                StartTime = timeEntry.StartTime,
                EndTime = timeEntry.EndTime,
                ProjectId = timeEntry.ProjectId,
                TaskItemId = timeEntry.TaskItemId,
                IsBillable = timeEntry.IsBillable,
                HourlyRate = timeEntry.HourlyRate
            };

            await PopulateDropdowns();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditTimeEntryViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var timeEntry = await _context.TimeEntries
                    .Where(te => te.Id == id && te.UserId == userId)
                    .FirstOrDefaultAsync();

                if (timeEntry == null) return NotFound();

                timeEntry.Description = model.Description;
                timeEntry.StartTime = model.StartTime;
                timeEntry.EndTime = model.EndTime;
                timeEntry.ProjectId = model.ProjectId;
                timeEntry.TaskItemId = model.TaskItemId == 0 ? null : model.TaskItemId;
                timeEntry.IsBillable = model.IsBillable;
                timeEntry.HourlyRate = model.HourlyRate;
                timeEntry.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> StartTimer(StartTimerViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            
            // Stop any existing running timer
            var runningEntry = await _context.TimeEntries
                .Where(te => te.UserId == userId && te.EndTime == null)
                .FirstOrDefaultAsync();
                
            if (runningEntry != null)
            {
                runningEntry.EndTime = DateTime.UtcNow;
                runningEntry.ModifiedDate = DateTime.UtcNow;
            }

            var project = await _context.Projects.FindAsync(model.ProjectId);
            
            var timeEntry = new TimeEntry
            {
                Description = model.Description,
                StartTime = DateTime.UtcNow,
                ProjectId = model.ProjectId,
                TaskItemId = model.TaskItemId == 0 ? null : model.TaskItemId,
                UserId = userId!,
                IsBillable = project?.IsBillable ?? true,
                HourlyRate = project?.HourlyRate
            };

            _context.TimeEntries.Add(timeEntry);
            await _context.SaveChangesAsync();

            return Json(new { success = true, timeEntryId = timeEntry.Id });
        }

        [HttpPost]
        public async Task<IActionResult> StopTimer(int timeEntryId)
        {
            var userId = _userManager.GetUserId(User);
            var timeEntry = await _context.TimeEntries
                .Where(te => te.Id == timeEntryId && te.UserId == userId && te.EndTime == null)
                .FirstOrDefaultAsync();

            if (timeEntry == null)
                return Json(new { success = false, message = "Timer not found or already stopped" });

            timeEntry.EndTime = DateTime.UtcNow;
            timeEntry.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, duration = timeEntry.Duration.ToString(@"hh\:mm\:ss") });
        }

        [HttpGet]
        public async Task<IActionResult> GetRunningTimer()
        {
            var userId = _userManager.GetUserId(User);
            var runningEntry = await _context.TimeEntries
                .Include(te => te.Project)
                .Include(te => te.TaskItem)
                .Where(te => te.UserId == userId && te.EndTime == null)
                .FirstOrDefaultAsync();

            if (runningEntry == null)
                return Json(new { isRunning = false });

            return Json(new 
            { 
                isRunning = true, 
                timeEntryId = runningEntry.Id,
                description = runningEntry.Description,
                startTime = runningEntry.StartTime,
                projectName = runningEntry.Project.Name,
                taskName = runningEntry.TaskItem?.Name
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetTasksByProject(int projectId)
        {
            var tasks = await _context.TaskItems
                .Where(t => t.ProjectId == projectId && !t.IsCompleted)
                .Select(t => new { id = t.Id, name = t.Name })
                .ToListAsync();

            return Json(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var timeEntry = await _context.TimeEntries
                .Where(te => te.Id == id && te.UserId == userId)
                .FirstOrDefaultAsync();

            if (timeEntry == null) return NotFound();

            _context.TimeEntries.Remove(timeEntry);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        private async Task PopulateDropdowns()
        {
            var userId = _userManager.GetUserId(User);
            
            var projects = await _context.Projects
                .Include(p => p.Client)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Client.Name)
                .ThenBy(p => p.Name)
                .ToListAsync();

            ViewBag.Projects = new SelectList(projects, "Id", "Name");
            ViewBag.ProjectsWithClient = projects.Select(p => new 
            { 
                Value = p.Id, 
                Text = $"{p.Client.Name} - {p.Name}" 
            }).ToList();
        }
    }
}