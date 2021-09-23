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
    public class ResourcesController : ControllerBase
    {
        private readonly MT4NorthContext _context;
        private readonly ILogger<NotesController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ResourcesController(
            ILogger<NotesController> logger,
            MT4NorthContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Resources/Activity/{activityId}
        // Get resources for Activity with id == activityId
        // (if user got R or RW permission to the Project the Activity belongs to)
        [Authorize()]
        [HttpGet("Activity/{activityId}")]
        public async Task<ActionResult<IEnumerable<Resource>>> GetResourcesForActivity(int activityId)
        {
            // Get user
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }

            // Fetch activity and project
            var activity = await _context.Activities.FindAsync(activityId);
            if (activity == null)
            {
                return BadRequest();
            }
            var project  = await _context.Projects.FindAsync(activity.ProjectId);
            if (project == null)
            {
                return BadRequest();
            }
            // Check user permissions
            var userproj = await _context.UserProjects.Where(p => p.ProjectId == project.ProjectId && p.User.Id == user.Id && p.Status == UserProjectStatus.ACCEPTED && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.READ)).ToListAsync<UserProject>();
            if(userproj.Count == 0)
            {
                return Unauthorized();
            }

            var resources = await _context.Resources.Where(n => n.ActivityId == activity.ActivityId).ToListAsync<Resource>();
            return Ok(resources);
        }

        // GET: api/Resources/5
        [Authorize()]
        [HttpGet("{id}")]
        public async Task<ActionResult<Resource>> GetResource(int id)
        {
            // Get user
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }

            var resource = await _context.Resources.FindAsync(id);
            // Make sure that we got a note to return 
            if (resource == null)
            {
                return NotFound();
            }

            // Fetch activity and project
            var activity = await _context.Activities.FindAsync(resource.ActivityId);
            if (activity == null)
            {
                return BadRequest();
            }
            var project = await _context.Projects.FindAsync(activity.ProjectId);
            if (project == null)
            {
                return BadRequest();
            }
            // Get userprojects with permissions R/RW
            var userproj = await _context.UserProjects.Where(p => p.ProjectId == project.ProjectId && p.User.Id == user.Id && p.Status == UserProjectStatus.ACCEPTED && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.READ)).ToListAsync<UserProject>();

            // Check if the user is allowed to do that
            if (userproj.Count == 0)
            {

                return Forbid();
            }

            return Ok(resource);
        }

        // PUT: api/Resources/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [Authorize()]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutResources(int id, Resource resource)
        {
            // Get user
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }

            // Fetch activity and project
            var activity = await _context.Activities.FindAsync(resource.ActivityId);
            if (activity == null)
            {
                return BadRequest();
            }
            var project = await _context.Projects.FindAsync(activity.ProjectId);
            if (project == null)
            {
                return BadRequest();
            }
            // Get userprojects with permissions W/RW
            var userproj = await _context.UserProjects.Where(p => p.ProjectId == project.ProjectId && p.User.Id == user.Id && p.Status == UserProjectStatus.ACCEPTED && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.WRITE)).ToListAsync<UserProject>();
            // Check if the user is allowed to do that
            if (userproj.Count == 0)
            {

                return Forbid();
            }

            // Check id's
            if (id != resource.ResourceId)
            {
                return BadRequest();
            }
            // Update Note
            _context.Entry(resource).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(resource);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResourceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: api/Resources
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [Authorize()]
        [HttpPost("Activity/{activityId}")]
        public async Task<ActionResult<Resource>> PostResource(int activityId, Resource resource)
        {
            // Get user
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user != null)
            {
                Unauthorized();
            }
            
            // Fetch activity and project
            var activity = await _context.Activities.FindAsync(activityId);
            if (activity == null)
            {
                return BadRequest();
            }
            var project = await _context.Projects.FindAsync(activity.ProjectId);
            if (project == null)
            {
                return BadRequest(); // ??? NoContent() ??? NotFound() ???
            }
            // Get userprojects with permissions W/RW
            var userproj = await _context.UserProjects.Where(p => p.ProjectId == project.ProjectId && p.User.Id == user.Id && p.Status == UserProjectStatus.ACCEPTED && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.WRITE)).ToListAsync<UserProject>();
            // Check if the user is allowed to do that
            if (userproj.Count == 0)
            {
                return Forbid();
            }

            // Note with empty string content is not Ok 
            if (resource.Url.Trim().Length == 0)
            {
                return BadRequest();
            }

            // Set the activity Id
            //note.Activity = activity;
            resource.ActivityId = activity.ActivityId;
            // Trim text 
            resource.Url = resource.Url.Trim();

            // Save to database
            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetResource", new { id = resource.ResourceId }, resource);
        }

        // DELETE: api/Resources/5
        [Authorize()]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Resource>> DeleteResource(int id)
        {
            // Get user
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }

            // Fetch resource
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null)
            {
                return NotFound();
            }

            // Fetch activity and project
            var activity = await _context.Activities.FindAsync(resource.ActivityId);
            if (activity == null)
            {
                return BadRequest();
            }
            var project = await _context.Projects.FindAsync(activity.ProjectId);
            if (project == null)
            {
                return BadRequest();
            }
            // Get userprojects with permissions W/RW
            var userproj = await _context.UserProjects.Where(p => p.ProjectId == project.ProjectId && p.User.Id == user.Id && p.Status == UserProjectStatus.ACCEPTED && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.WRITE)).ToListAsync<UserProject>();
            // Check if the user is allowed to do that
            if (userproj.Count == 0)
            {
                return Forbid();
            }

            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync();

            return Ok(resource);
        }

        private bool ResourceExists(int id)
        {
            return _context.Resources.Any(e => e.ResourceId == id);
        }
    }
}
