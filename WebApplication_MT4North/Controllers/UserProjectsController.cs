using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication_MT4North.Models;
using WebApplication_MT4North.Resources;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication_MT4North.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserProjectsController : ControllerBase
    {
        private readonly MT4NorthContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserProjectsController(
            ILogger<AccountController> logger,
            MT4NorthContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }


        // GET: api/UserProjects
        /// <summary>
        /// Get all accepted userProjects for current authorized user 
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// User UserProjects
        /// </returns>
        /// <response code="200">Ok</response>
        /// <response code="401">Unautherized</response>
        /// <response code="404">User Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [HttpGet("")]
        [Authorize]
        public async Task<ActionResult> GetCurrentUserProjects()
        {
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            } 
            else
            {
                // fetch all user-projects where the user is a member
                var userProjects = await _context.UserProjects.Where(p => p.User.UserName == user.UserName && p.Status == UserProjectStatus.ACCEPTED).ToListAsync<UserProject>();
                foreach(var userProject in userProjects)
                {
                    _context.Projects.FirstOrDefault(p => p.ProjectId == userProject.ProjectId);
                    _context.Users.FirstOrDefault(u => u.Id == userProject.User.Id);
                }
                return Ok(userProjects);
            }
        }

        // GET: api/UserProjects/{id}
        /// <summary>
        /// Get userProject with id 
        /// </summary>
        /// <remarks></remarks>
        /// <param name="id"></param>
        /// <returns>
        /// Requested UserProject
        /// </returns>
        /// <response code="200">Ok</response>
        /// <response code="401">Unautherized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [Authorize()]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserProject>> GetUserProject(int id)
        {
            var userproject = await _context.UserProjects.FindAsync(id);
            if (userproject == null)
            {
                return Unauthorized();
            }

            // Check if the caller got the READ rights for the project the UserProject with id belongs to! Otherwise return Unauthorized
            string callerEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var caller = await _userManager.FindByEmailAsync(callerEmail);
            var callerUserProject = await _context.UserProjects.FirstOrDefaultAsync<UserProject>(p => p.ProjectId == userproject.ProjectId && p.UserId == caller.Id && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.READ));
            if (callerUserProject == null)
            {
                // The caller doesnt have WRITE rights to this project
                return Forbid();
            }
            // fetch the rest
            await _context.Projects.Where(p => p.ProjectId == userproject.ProjectId).ToListAsync<Project>();
            await _context.Users.Where(u => u.Id == userproject.UserId).ToListAsync<ApplicationUser>();
            // return result
            return Ok(userproject);
        }

        /*
        [HttpGet("All")]
        [Authorize(Roles="AdminUser")]
        public async Task<ActionResult> GetAllProjects()
        {
            var userProjects = await _context.UserProjects.ToListAsync<UserProject>();
            foreach (var userProject in userProjects)
            {
                _context.Projects.FirstOrDefault(p => p.ProjectId == userProject.ProjectId);
            }
            return Ok(userProjects);
        }
        */

        // GET: api/UserProjects/Project/{projectId}
        /// <summary>
        /// Get all userProjects for the project with id projectId 
        /// </summary>
        /// <remarks></remarks>
        /// <param name="projectId"></param>
        /// <returns>
        /// All UserProjects for the project
        /// </returns>
        /// <response code="200">Ok</response>
        /// <response code="401">Unautherized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [Authorize()]
        [HttpGet("Project/{projectId}")]
        public async Task<ActionResult> GetAllForProject(int projectId)
        {
            // Check if the caller got the READ rights for project with id projectId! Otherwise return Unauthorized
            string callerEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var caller = await _userManager.FindByEmailAsync(callerEmail);
            if (caller == null)
            {
                return Unauthorized();
            }
            // TODO Enum non R-read RW-readwrite
            var callerUserProject = await _context.UserProjects.FirstOrDefaultAsync<UserProject>(p => p.ProjectId == projectId && p.UserId == caller.Id && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.READ));
            if (callerUserProject == null)
            {
                // The caller doesnt have READ rights to this project
                return Forbid();
            }

            // fetch all user-projects for project with id projectId 
            var userProjects = await _context.UserProjects.Where(p => p.ProjectId == projectId && (p.Status == UserProjectStatus.PENDING || p.Status == UserProjectStatus.ACCEPTED)).ToListAsync<UserProject>();
            foreach (var userProject in userProjects)
            {
                await _context.Projects.Where(p => p.ProjectId == userProject.ProjectId).ToListAsync<Project>();
                await _context.Users.Where(u => u.Id == userProject.UserId).ToListAsync<ApplicationUser>();
            }

            // return found user-projects
            return Ok(userProjects);
        }

        // POST: api/UserProjects/{userEmail}/{projectId}/{role}/{permissions}
        /// <summary>
        /// Creates a new UserProject for Project with id ProjectId and Status pending linked to the user in the userInvite
        /// </summary>
        /// <remarks></remarks>
        /// <param name="email"></param>
        /// <param name="projectId"></param>
        /// <param name="role"></param>
        /// <param name="rights"></param>
        /// <returns>
        /// Created UserProject
        /// </returns>
        /// <response code="201">Created</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unautherized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [Authorize()]
        [HttpPost("{userEmail}/{projectId}/{role}/{permissions}")]
        public async Task<ActionResult> InviteUserToProject(string userEmail, int projectId, int role, UserProjectPermissions permissions)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                var error = new ErrorResult();
                error.Message = "User " + userEmail + " not found";
                return NotFound(error);
            }

            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                var error = new ErrorResult();
                error.Message = "Project with id: " + projectId + " not found";
                return NotFound(error);
            }

            // Check if the caller got the RW rights! Otherwise return Unauthorized
            string callerEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var caller = await _userManager.FindByEmailAsync(callerEmail);
            var callerUserProject = await _context.UserProjects.FirstOrDefaultAsync<UserProject>(p => p.ProjectId == projectId && p.UserId == caller.Id && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.WRITE)); // TODO Enum non R-read RW-readwrite
            if (callerUserProject == null)
            {
                // The caller doesnt have WRITE rights to this project
                return Forbid();
            }

            // fetch all user-projects for project with id projectId (that i NOT rejected)
            var userProjects = await _context.UserProjects.Where(p => p.ProjectId == projectId && p.UserId == user.Id && p.Status != UserProjectStatus.REJECTED).ToListAsync<UserProject>();
            // check if user is already invited to the project (or is a member)
            if (userProjects.Count > 0)
            {
                var error = new ErrorResult();
                var invitationStatus = userProjects.FirstOrDefault<UserProject>().Status;
                var statusText = "";
                switch (invitationStatus)
                {
                    case UserProjectStatus.PENDING:
                        statusText = "pending";
                        break;
                    case UserProjectStatus.ACCEPTED:
                        statusText = "accepted";
                        break;
                    case UserProjectStatus.REJECTED:
                        statusText = "rejected";
                        break;
                    default:
                        statusText = "unknown";
                        break;
                }
                error.Message = "User " + userEmail + " is already invited and the invitation is " + statusText + ".";
                //error.Errors = new List<string>();
                return BadRequest(error);
            }

            // Create a new UserProject with status Pending and the provided role and rights
            var userProject = new UserProject();
            userProject.User = user;
            userProject.Project = project;
            userProject.Role = (UserProjectRoles)role;
            userProject.Rights = (UserProjectPermissions)permissions; //rights;
            userProject.Status = UserProjectStatus.PENDING;
            // Store the user project
            _context.UserProjects.Add(userProject);
            await _context.SaveChangesAsync();

            //return Ok(userProject);
            return CreatedAtAction("InviteUserToProject", new { userEmail, projectId, role, permissions }, userProject);
            //return CreatedAtRoute(nameof(GetUserProject), new { id = userProject.UserProjectId }, userProject);
        }

        // PUT: api/UserProjects/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// <summary>
        /// Update a UserProject
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// Updated UserProject
        /// </returns>
        /// <response code="200">OK</response>
        /// <response code="401">Unautherized</response>
        /// <response code="403">Unautherized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [Authorize()]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserProject(int id, UserProject userproject)
        {
            // Check if the caller got the RW rights! Otherwise return Unauthorized
            string callerEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var caller = await _userManager.FindByEmailAsync(callerEmail);
            if (caller == null)
            {
                return Unauthorized();
            }
            var callerUserProject = await _context.UserProjects.FirstOrDefaultAsync<UserProject>(p => p.ProjectId == userproject.ProjectId && p.UserId == caller.Id && (p.Rights == UserProjectPermissions.READWRITE || p.Rights == UserProjectPermissions.WRITE));
            if (callerUserProject == null)
            {
                // The caller doesnt have WRITE rights to this project
                return Forbid();
            }

            if (id != userproject.UserProjectId)
            {
                return BadRequest();
            }

            if (userproject.Role == UserProjectRoles.OWNER && userproject.Rights != UserProjectPermissions.READWRITE)
            {
                userproject.Rights = UserProjectPermissions.READWRITE;
            }
            if (callerUserProject.UserProjectId == userproject.UserProjectId)
            {
                callerUserProject.Role = userproject.Role;
                callerUserProject.Rights = userproject.Rights;
                callerUserProject.Status = userproject.Status;
                _context.Entry(callerUserProject).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(userproject).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
                // return updateted object
                return Ok(userproject);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserProjectExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //GET: api/UserProjects/Invites/
        /// <summary>
        /// Get the invites for the current authorized user
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// Current user invites
        /// </returns>
        /// <response code="200">OK</response>
        /// <response code="401">Unautherized</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [Authorize()]
        [HttpGet("Invites")]
        public async Task<ActionResult> GetInvitesForUser()
        {
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            // fetch all user-projects for user with id and status Pending 
            var userProjects = await _context.UserProjects.Where(p => p.UserId == user.Id && p.Status == UserProjectStatus.PENDING).ToListAsync<UserProject>();
            foreach (var userProject in userProjects)
            {
                await _context.Projects.Where(p => p.ProjectId == userProject.ProjectId).ToListAsync<Project>();
                await _context.Users.Where(u => u.Id == userProject.UserId).ToListAsync<ApplicationUser>();
            }

            // return found user-projects invites 
            return Ok(userProjects);
        }

        //POST: api/UserProjects/Invite/Accept/{id}
        /// <summary>
        /// Acccept a UserProject invatation with {id}
        /// </summary>
        /// <returns>
        /// The accepted UserProject
        /// </returns>
        /// <param name="id"></param>
        /// <response code="200">OK</response>
        /// <response code="401">Unautherized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [Authorize()]
        [HttpPost("Invites/Accept/{id}")]
        public async Task<ActionResult> AcceptInvite(int id)
        {
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }
            // fetch user-project for user with UserProjectId id and Status pending 
            var invite = await _context.UserProjects.FirstOrDefaultAsync<UserProject>(p => p.UserProjectId == id && p.UserId == user.Id && p.Status == UserProjectStatus.PENDING);
            if (invite == null)
            {
                return NotFound(null);
            }
            invite.Status = UserProjectStatus.ACCEPTED;
            // fetch the rest
            await _context.Projects.Where(p => p.ProjectId == invite.ProjectId).ToListAsync<Project>();
            await _context.Users.Where(u => u.Id == invite.UserId).ToListAsync<ApplicationUser>();
            // Update userProject (invatation)
            _context.Entry(invite).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            // return accepted user-projects invite
            return Ok(invite);
        }

        //POST: api/UserProjects/Invite/Reject/{id} 
        /// <summary>
        /// Rejects a UserProject invatation with {id}
        /// </summary>
        /// <returns>
        /// The rejected UserProject
        /// </returns>
        /// <param name="id"></param>
        /// <response code="200">OK</response>
        /// <response code="401">Unautherized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [Authorize()]
        [HttpPost("Invites/Reject/{id}")]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> RejectInvite(int id)
        {
            string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }
            // fetch user-project for user with UserProjectId id and Status pending 
            var invite = await _context.UserProjects.FirstOrDefaultAsync<UserProject>(p => p.UserProjectId == id && p.UserId == user.Id && p.Status == UserProjectStatus.PENDING);
            if (invite == null)
            {
                return NotFound();
            }
            invite.Status = UserProjectStatus.REJECTED;
            // fetch the rest
            await _context.Projects.Where(p => p.ProjectId == invite.ProjectId).ToListAsync<Project>();
            await _context.Users.Where(u => u.Id == invite.UserId).ToListAsync<ApplicationUser>();
            // Update userProject (invatation)
            _context.Entry(invite).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            // return rejected user-projects invites 
            return Ok(invite);
        }

        // DELETE: api/UserProjects/{id}
        /// <summary>
        /// Deletes a UserProject with {id}
        /// </summary>
        /// <returns>
        /// The deleted UserProject
        /// </returns>
        /// <param name="id"></param>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unautherized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [Authorize()]
        [HttpDelete("{id}")]
        public async Task<ActionResult<UserProject>> DeleteUserProject(int id)
        {
            //string userEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            //var user = await _userManager.FindByEmailAsync(userEmail);

            string callerEmail = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var caller = await _userManager.FindByEmailAsync(callerEmail);
            if (caller == null)
            {
                return Unauthorized();
            }

            var userProject = await _context.UserProjects.FindAsync(id);
            if (userProject == null)
            {
                return NotFound();
            }

            var callerUserProject = await _context.UserProjects.FirstOrDefaultAsync<UserProject>(p => p.ProjectId == userProject.ProjectId && p.UserId == caller.Id);
            var projectOwners = await _context.UserProjects.Where(p => p.ProjectId == userProject.ProjectId && p.Role == UserProjectRoles.OWNER).ToListAsync<UserProject>();

            // set the user of notes created by the user of the user-project to null. Just in case we want to remove the user-project
            // the changes to the notes is only saved to the database if and only if we choose to delete the user-project
            var projectActivities = await _context.Activities.Where(a => a.ProjectId == userProject.ProjectId).ToListAsync<Activity>();
            var userNotes = new List<Note>();
            foreach (var activity in projectActivities)
            {
                var notes = await _context.Notes.Where(n => n.UserId == userProject.UserId && n.ActivityId == activity.ActivityId).ToListAsync<Note>();
                userNotes.AddRange(notes);
            }
            foreach (var note in userNotes)
            {
                note.UserId = null;
            }
            // Is the caller the owner of the project?
            if (callerUserProject.Role == UserProjectRoles.OWNER)
            {
                // Is the owner trying to remove their own userProject?
                if (callerUserProject.UserProjectId == id)
                {
                    // delete if more then one owner
                    if (callerUserProject.UserProjectId == id && projectOwners.Count > 1)
                    {
                        // delete. The owner can remove their own user-project if another owner exists
                        _context.UserProjects.Remove(userProject);
                        //_context.Entry(userNotes).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                        return Ok(userProject);
                    }
                }
                else
                {
                    // delete the UserProject. The owner can delete others participants user-projects
                    _context.UserProjects.Remove(userProject);
                    //_context.Entry(userNotes).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return Ok(userProject);
                }
                
            }
            else
            {
                // Is a project participant trying to remove their own user-project?
                if (callerUserProject.UserProjectId == id)
                {
                    // delete. A user can delete their own user-project
                    _context.UserProjects.Remove(userProject);
                    //_context.Entry(userNotes).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return Ok(userProject);
                }
            }

            // If no case above is satisfied. It must be forbidden (insufficient permissions or not allowed"
            return Forbid();
        }

        private bool UserProjectExists(int id)
        {
            return _context.UserProjects.Any(e => e.UserProjectId == id);
        }
    }
}
