using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApplication_MT4North.Models;

namespace WebApplication_MT4North.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivitiesController : ControllerBase
    {
        private readonly MT4NorthContext _context;
        private readonly ILogger<ActivitiesController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ActivitiesController(
            ILogger<ActivitiesController> logger,
            MT4NorthContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        // GET  /api/Activities
        /// <summary>
        /// Get all activitys for authenticated user
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// All activities for the user
        /// </returns>
        /// <response code="200">OK</response>
        /// <response code="401">Unautherized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [Authorize()]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Activity>>> GetActivities()
        {
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }

            // Get all user projects for the user where the user have R or RW permissions
            var userProjects = await _context.UserProjects.Where(p => p.User.UserName == user.UserName && p.Status == UserProjectStatus.ACCEPTED && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.READ)).ToListAsync<UserProject>();
            // Get all activities from all of the projects the user is a member of
            var activities = new List<Activity>();
            foreach (var userProject in userProjects)
            {
                var projectActivitys = await _context.Activities.Where(a => a.ProjectId == userProject.ProjectId).ToListAsync<Activity>();
                activities.AddRange(projectActivitys);
            }
            // Load Project, CustomActivityInfo, BaseActivityInfo from database
            foreach (var activity in activities)
            {
                // fetch sub-activity and project
                var project = await _context.Projects.Where(p => p.ProjectId == activity.ProjectId).ToListAsync<Project>();
                if (activity.BaseActivityInfoId != null)
                {
                    var baseactivity = await _context.BaseActivityInfos.FirstOrDefaultAsync(b => b.BaseActivityInfoId == activity.BaseActivityInfoId);
                    var theme = await _context.Themes.FirstOrDefaultAsync(t => t.ThemeId == activity.BaseActivityInfo.ThemeId);
                }
                if (activity.CustomActivityInfoId != null)
                {
                    var customactivity = await _context.CustomActivityInfos.FirstOrDefaultAsync(b => b.CustomActivityInfoId == activity.CustomActivityInfoId);
                    var theme = await _context.Themes.FirstOrDefaultAsync(t => t.ThemeId == activity.CustomActivityInfo.ThemeId);
                }
                var notes = await _context.Notes.Where(n => n.ActivityId == activity.ActivityId).ToListAsync<Note>();
                var resources = await _context.Resources.Where(r => r.ActivityId == activity.ActivityId).ToListAsync<Resource>();
            }
            // Return the activites found
            return activities;
        }

        // GET  /api/Activities/{id}
        /// <summary>
        /// Get activity with ActivityId == id
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// Activity with ActivityId == id
        /// </returns>
        /// <response code="200">OK</response>
        /// <response code="401">Unautherized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [Authorize()]
        [HttpGet("{id}")]
        public async Task<ActionResult<Activity>> GetActivity(int id)
        {
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }

            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
            {
                return NotFound();
            }

            // Check if user got R or RW permissions for the project the activity belongs to
            var userproject = await _context.UserProjects.Where(p => p.ProjectId == activity.ProjectId &&
                                                                p.UserId == user.Id && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.READ )).ToListAsync<UserProject>();
            if (userproject.Count == 0)
            {
                return Unauthorized();
            }

            // fetch base or custom -activity, project, notes and resources
            var project = await _context.Projects.Where(p => p.ProjectId == activity.ProjectId).ToListAsync<Project>();
            if (activity.BaseActivityInfoId != null)
            {
                var baseactivity = await _context.BaseActivityInfos.FirstOrDefaultAsync(b => b.BaseActivityInfoId == activity.BaseActivityInfoId);
                var theme = await _context.Themes.FirstOrDefaultAsync(t => t.ThemeId == activity.BaseActivityInfo.ThemeId);
            }
            if (activity.CustomActivityInfoId != null)
            {
                var customactivity = await _context.CustomActivityInfos.FirstOrDefaultAsync(b => b.CustomActivityInfoId == activity.CustomActivityInfoId);
                var theme = await _context.Themes.FirstOrDefaultAsync(t => t.ThemeId == activity.CustomActivityInfo.ThemeId);
            }
            var notes = await _context.Notes.Where(n => n.ActivityId == activity.ActivityId).ToListAsync<Note>();
            var resources = await _context.Resources.Where(r => r.ActivityId == activity.ActivityId).ToListAsync<Resource>();
            return activity;
        }

        // PUT  /api/Activities/{id}
        /// <summary>
        /// Update activity with ActivityId == id
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// The upateted activity
        /// </returns>
        /// <response code="200">OK</response>
        /// <response code="401">Unautherized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [Authorize()]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutActivity(int id, Activity activity)
        {
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }

            if (id != activity.ActivityId)
            {
                return BadRequest();
            }

            // Check if user got W or RW permissions for the project the activity belongs to
            var userproject = await _context.UserProjects.Where(p => p.ProjectId == activity.ProjectId &&
                                                                p.UserId == user.Id && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.WRITE)).ToListAsync<UserProject>();
            if (userproject.Count == 0)
            {
                return Forbid();
            }

            if (activity.CustomActivityInfo != null)
            {
                _context.Entry(activity.CustomActivityInfo).State = EntityState.Modified;
            }

            _context.Entry(activity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                return Ok(activity);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActivityExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // POST /api/Activities/
        /// <summary>
        /// Create a new activity for user in project with ProjectId == projectId
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// The new activity
        /// </returns>
        /// <response code="200">OK</response>
        /// <response code="401">Unautherized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [Authorize()]
        [HttpPost("")]
        public async Task<ActionResult<Activity>> PostActivity(Activity activity)
        {
            // Check input data
            if (activity.BaseActivityInfoId != null && activity.CustomActivityInfoId != null)
            {
                return BadRequest(); // cant have both BaseActivity AND CustomActivity
            }

            if (activity.BaseActivityInfoId == null && activity.CustomActivityInfoId == null)
            {
                return BadRequest(); // We must have a BaseActivity OR CustomActivity
            }

            if (activity.BaseActivityInfo != null && activity.BaseActivityInfoId != activity.BaseActivityInfo.BaseActivityInfoId)
            {
                return BadRequest(); // BaseInfoActivityInfo id's dont match
            }

            if (activity.ProjectId == null /*|| activity.ProjectId != activity.Project.ProjectId*/)
            {
                return BadRequest(); // Projects is null or id's dont match
            }

            // Check that the base activity exixts if attached
            if (activity.BaseActivityInfo != null)
            {
                if (!BaseActivityInfoExists((int)activity.BaseActivityInfoId))
                {
                    return NotFound(); // We didnt find the base activity
                }
            }

            // Check that the project exists
            if (!ProjectExists((int)activity.ProjectId))
            {
                return NotFound(); // We didnt find the base activity
            }

            // Make sure we dont have any notes
            if (activity.Notes.Count != 0)
            {
                return BadRequest();
            }

            // Check permissions
            // Check if the caller got the RW rights! Otherwise return Forbidden
            string callerEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var caller = await _userManager.FindByEmailAsync(callerEmail);
            if (caller == null)
            {
                return Unauthorized();
            }
            var callerUserProject = await _context.UserProjects.FirstOrDefaultAsync<UserProject>(p => p.ProjectId == activity.ProjectId && p.UserId == caller.Id && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.WRITE)); // TODO Enum non R-read RW-readwrite
            if (callerUserProject == null)
            {
                // The caller doesnt have WRITE rights to this project
                return Forbid();
            }

            if (activity.BaseActivityInfoId != null)
            {
                activity.BaseActivityInfo = null;
            }

            // If we got a CustomActivityInfo attached to the activity. Add it to the database
            if (activity.CustomActivityInfo != null)
            {
                activity.CustomActivityInfo.Theme = null;
                _context.CustomActivityInfos.Add(activity.CustomActivityInfo);
                await _context.SaveChangesAsync();

                activity.CustomActivityInfoId = activity.CustomActivityInfo.CustomActivityInfoId;
            }
            // Add the activity to the database
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            // fetch sub-activity and theme
            var project = await _context.Projects.Where(p => p.ProjectId == activity.ProjectId).ToListAsync<Project>();
            if (activity.BaseActivityInfoId != null)
            {
                var baseactivity = await _context.BaseActivityInfos.FirstOrDefaultAsync(b => b.BaseActivityInfoId == activity.BaseActivityInfoId);
                var theme = await _context.Themes.FirstOrDefaultAsync(t => t.ThemeId == activity.BaseActivityInfo.ThemeId);
            }
            if (activity.CustomActivityInfoId != null)
            {
                var customactivity = await _context.CustomActivityInfos.FirstOrDefaultAsync(b => b.CustomActivityInfoId == activity.CustomActivityInfoId);
                var theme = await _context.Themes.FirstOrDefaultAsync(t => t.ThemeId == activity.CustomActivityInfo.ThemeId);
            }
            var notes = await _context.Notes.Where(n => n.ActivityId == activity.ActivityId).ToListAsync<Note>();
            var resources = await _context.Resources.Where(r => r.ActivityId == activity.ActivityId).ToListAsync<Resource>();

            return CreatedAtAction("GetActivity", new { id = activity.ActivityId }, activity);
        }

        // DEL  /api/Activities/{id}
        /// <summary>
        /// Delete activity with ActivityId == id
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// The new activity
        /// </returns>
        /// <response code="200">OK</response>
        /// <response code="401">Unautherized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [Authorize()]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Activity>> DeleteActivity(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
            {
                return NotFound();
            }

            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }
            var roles = await _userManager.GetRolesAsync(user);

            // Check if its a baseactivity, then require that the user got AdminUser-Role
            if (activity.BaseActivityInfoId != null && !roles.Contains("AdminUser"))
            {
                return Forbid();
            }

            // Check if user got W or RW permissions for the project the activity belongs to
            var userproject = await _context.UserProjects.Where(p => p.ProjectId == activity.ProjectId &&
                                                            p.UserId == user.Id && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.WRITE)).ToListAsync<UserProject>();
            if (userproject?.Count < 1)
            {
                return Forbid();
            }

            // Remove custom activity info if we got one
            if (activity.CustomActivityInfoId != null)
            {
                var customActivityToDelete = await _context.CustomActivityInfos.FindAsync(activity.CustomActivityInfoId);
                _context.CustomActivityInfos.Remove(customActivityToDelete);// activity.CustomActivityInfo);
            }
            if (activity.Notes.Count > 0)
            {
                _context.Notes.RemoveRange(activity.Notes);
            }
            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            return Ok(activity);
        }

        // GET  /api/Activities/Project/{projectId}
        /// <summary>
        /// All activitys for project with ProjectId == projectId
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// The activitys for the project with ProjectId == projectId
        /// </returns>
        /// <response code="200">OK</response>
        /// <response code="401">Unautherized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [Authorize()]
        [HttpGet("Project/{projectId}")]
        public async Task<ActionResult<IEnumerable<Activity>>> GetActivitysForProject(int projectId)
        {
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }

            var userproject = await _context.UserProjects.FirstOrDefaultAsync(p => p.ProjectId == projectId &&
                                                                p.UserId == user.Id && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.READ ));
            if (userproject == null)
            {
                return Unauthorized();
            }

            // fetch the project from the database
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == projectId);
            var activities = await _context.Activities.Where(a => a.ProjectId == userproject.ProjectId).ToListAsync<Activity>();
            foreach(var activity in activities)
            {
                // fetch base or custom -activity and theme
                if (activity.BaseActivityInfoId != null)
                {
                    var baseactivity = await _context.BaseActivityInfos.FirstOrDefaultAsync(b => b.BaseActivityInfoId == activity.BaseActivityInfoId);
                    var theme = await _context.Themes.FirstOrDefaultAsync(t => t.ThemeId == activity.BaseActivityInfo.ThemeId);
                }
                if (activity.CustomActivityInfoId != null)
                {
                    var customactivity = await _context.CustomActivityInfos.FirstOrDefaultAsync(b => b.CustomActivityInfoId == activity.CustomActivityInfoId);
                    var theme = await _context.Themes.FirstOrDefaultAsync(t => t.ThemeId == activity.CustomActivityInfo.ThemeId);
                }
                var notes = await _context.Notes.Where(n => n.ActivityId == activity.ActivityId).ToListAsync<Note>();
                var resources = await _context.Resources.Where(r => r.ActivityId == activity.ActivityId).ToListAsync<Resource>();
            }
            return activities;
        }

        private bool ActivityExists(int id)
        {
            return _context.Activities.Any(e => e.ActivityId == id);
        }

        private bool BaseActivityInfoExists(int id)
        {
            return _context.BaseActivityInfos.Any(e => e.BaseActivityInfoId == id);
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }
    }
}
