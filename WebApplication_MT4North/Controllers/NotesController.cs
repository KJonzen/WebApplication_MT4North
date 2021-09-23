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
    public class NotesController : ControllerBase
    {
        private readonly MT4NorthContext _context;
        private readonly ILogger<NotesController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotesController(
            ILogger<NotesController> logger,
            MT4NorthContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Notes
        // Get notes for user
        [Authorize()]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesForUser()
        {
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }

            var notes = await _context.Notes.Where(n => n.UserId == user.Id).ToListAsync<Note>();
            return notes;
        }

        // GET: api/Notes/Activity/{activityId}
        // Get notes for Activity with id == activityId
        // (if user got R or RW permission to the Project the Activity belongs to)
        [Authorize()]
        [HttpGet("Activity/{activityId}")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesForActivity(int activityId)
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
                return NotFound();
            }
            var project  = await _context.Projects.FindAsync(activity.ProjectId);
            if (project == null)
            {
                return NoContent();
            }
            // Check user permissions
            var userproj = await _context.UserProjects.Where(p => p.ProjectId == project.ProjectId && p.User.Id == user.Id && p.Status == UserProjectStatus.ACCEPTED && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.READ)).ToListAsync<UserProject>();
            if(userproj.Count == 0)
            {
                return Unauthorized();
            }

            var notes = await _context.Notes.Where(n => n.ActivityId == activity.ActivityId).ToListAsync<Note>();
            return notes;
        }

        // GET: api/Notes/5
        [Authorize()]
        [HttpGet("{id}")]
        public async Task<ActionResult<Note>> GetNote(int id)
        {
            // Get user
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }

            var note = await _context.Notes.FindAsync(id);
            // Make sure that we got a note to return 
            if (note == null)
            {
                return NotFound();
            }

            // Fetch activity and project
            var activity = await _context.Activities.FindAsync(note.ActivityId);
            if (activity == null)
            {
                return NoContent();
            }
            var project = await _context.Projects.FindAsync(activity.ProjectId);
            if (project == null)
            {
                return NoContent();
            }
            // Get userprojects with permissions R/RW
            var userproj = await _context.UserProjects.Where(p => p.ProjectId == project.ProjectId && p.User.Id == user.Id && p.Status == UserProjectStatus.ACCEPTED && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.READ)).ToListAsync<UserProject>();

            // Check if the user is allowed to do that
            if (note.UserId != user.Id && userproj.Count == 0)
            {

                return Unauthorized();
            }

            return note;
        }

        // PUT: api/Notes/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [Authorize()]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNote(int id, Note note)
        {
            // Get user
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }

            // Fetch activity and project
            var activity = await _context.Activities.FindAsync(note.ActivityId);
            if (activity == null)
            {
                return NoContent();
            }
            var project = await _context.Projects.FindAsync(activity.ProjectId);
            if (project == null)
            {
                return NoContent();
            }
            // Get userprojects with permissions W/RW
            var userproj = await _context.UserProjects.Where(p => p.ProjectId == project.ProjectId && p.User.Id == user.Id && p.Status == UserProjectStatus.ACCEPTED && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.WRITE)).ToListAsync<UserProject>();
            // Check if the user is allowed to do that
            if (note.UserId != user.Id && userproj.Count == 0)
            {

                return Unauthorized();
            }

            // Check id's
            if (id != note.NoteId)
            {
                return BadRequest();
            }
            // Update Note
            _context.Entry(note).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(note);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NoteExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: api/Notes
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [Authorize()]
        [HttpPost("Activity/{activityId}")]
        public async Task<ActionResult<Note>> PosNote(int activityId, Note note)
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
                return NotFound();
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
            if (note.Text.Trim().Length == 0)
            {
                return BadRequest();
            }

            // Set the user Id
            //note.User = user;
            note.UserId = user.Id;
            // If no timestamp set from frontend, Use local server time as a bad fallback
            if (note.TimeStamp == null)
            {
                note.TimeStamp = DateTime.Now;
            }
            // Set the activity Id
            //note.Activity = activity;
            note.ActivityId = activity.ActivityId;
            // Trim text 
            note.Text = note.Text.Trim();

            // Save to database
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetNote", new { id = note.NoteId }, note);
        }

        // DELETE: api/Notes/5
        [Authorize()]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Note>> DeleteNote(int id)
        {
            // Get user
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }
            // Fetch note
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound();
            }

            // Fetch activity and project
            var activity = await _context.Activities.FindAsync(note.ActivityId);
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
            if (note.UserId != user.Id || userproj.Count == 0)
            {
                return Unauthorized();
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return note;
        }

        private bool NoteExists(int id)
        {
            return _context.Notes.Any(e => e.NoteId == id);
        }
    }
}
