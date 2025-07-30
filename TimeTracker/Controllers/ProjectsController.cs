using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Models;

namespace TimeTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ProjectsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");
            
            IQueryable<Project> projectsQuery = _context.Projects
                .Include(p => p.Client)
                .Include(p => p.CreatedByUser);

            if (!isAdmin)
            {
                var userId = user!.Id;
                projectsQuery = projectsQuery.Where(p => 
                    p.CreatedByUserId == userId || 
                    p.ProjectUsers.Any(pu => pu.UserId == userId));
            }

            var projects = await projectsQuery
                .OrderBy(p => p.Client.Name)
                .ThenBy(p => p.Name)
                .ToListAsync();

            return View(projects);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.CreatedByUser)
                .Include(p => p.Tasks)
                .Include(p => p.ProjectUsers)
                .ThenInclude(pu => pu.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");
            
            if (!isAdmin && project.CreatedByUserId != user!.Id && 
                !project.ProjectUsers.Any(pu => pu.UserId == user.Id))
            {
                return Forbid();
            }

            return View(project);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            ViewData["ClientId"] = new SelectList(await _context.Clients.Where(c => c.IsActive).ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([Bind("Name,Description,ClientId,IsBillable,HourlyRate")] Project project)
        {
            if (ModelState.IsValid)
            {
                project.CreatedByUserId = _userManager.GetUserId(User);
                project.CreatedDate = DateTime.UtcNow;
                
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["ClientId"] = new SelectList(await _context.Clients.Where(c => c.IsActive).ToListAsync(), "Id", "Name", project.ClientId);
            return View(project);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");
            
            if (!isAdmin && project.CreatedByUserId != user!.Id)
            {
                return Forbid();
            }

            ViewData["ClientId"] = new SelectList(await _context.Clients.Where(c => c.IsActive).ToListAsync(), "Id", "Name", project.ClientId);
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ClientId,IsBillable,HourlyRate,IsActive")] Project project)
        {
            if (id != project.Id) return NotFound();

            var existingProject = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (existingProject == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");
            
            if (!isAdmin && existingProject.CreatedByUserId != user!.Id)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    project.CreatedDate = existingProject.CreatedDate;
                    project.CreatedByUserId = existingProject.CreatedByUserId;
                    
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["ClientId"] = new SelectList(await _context.Clients.Where(c => c.IsActive).ToListAsync(), "Id", "Name", project.ClientId);
            return View(project);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.CreatedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (project == null) return NotFound();

            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                // Soft delete - just mark as inactive
                project.IsActive = false;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}