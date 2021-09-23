using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication_MT4North.Models;

namespace WebApplication_MT4North.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseActivityInfosController : ControllerBase
    {
        private readonly MT4NorthContext _context;

        public BaseActivityInfosController(MT4NorthContext context)
        {
            _context = context;
        }

        // GET: api/BaseActivityInfos
        /// <summary>
        /// Get all base activities available
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// The available base activities
        /// </returns>
        /// <response code="200">OK</response>
        /// <response code="401">Unautherized</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [Authorize()]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BaseActivityInfo>>> GetBaseActivityInfos()
        {
            var baseActivityInfos = await _context.BaseActivityInfos.ToListAsync();
            foreach(var baseActivityInfo in baseActivityInfos)
            {
                // fetch the theme of the activity
                _context.Themes.FirstOrDefault(t => t.ThemeId == baseActivityInfo.ThemeId);
            }
            return baseActivityInfos;
        }

        // GET: api/BaseActivityInfos/5
        /// <summary>
        /// Get base activity with BaseActivityId == id
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// The base activity with BaseActivityId == id
        /// </returns>
        /// <response code="200">OK</response>
        /// <response code="401">Unautherized</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [Authorize()]
        [HttpGet("{id}")]
        public async Task<ActionResult<BaseActivityInfo>> GetBaseActivityInfo(int id)
        {
            var baseActivityInfo = await _context.BaseActivityInfos.FindAsync(id);

            if (baseActivityInfo == null)
            {
                return NotFound();
            }

            _context.Themes.FirstOrDefault(t => t.ThemeId == baseActivityInfo.ThemeId);
            return baseActivityInfo;
        }

        // PUT: api/BaseActivityInfos/5
        /// <summary>
        /// Update base activity with BaseActivityId == id
        /// Must have AdminUser-role to execute
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// The updated base activity with BaseActivityId == id
        /// </returns>
        /// <response code="200">OK</response>
        /// <response code="204">No content</response>
        /// <response code="400">BadRequest</response>
        /// <response code="401">Forbidden</response>
        /// <response code="403">Unautherized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [Authorize(Roles = "AdminUser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBaseActivityInfo(int id, BaseActivityInfo baseActivityInfo)
        {
            if (id != baseActivityInfo.BaseActivityInfoId)
            {
                return BadRequest();
            }

            _context.Entry(baseActivityInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                // fetch updated baseActivity??
                var updatedBaseActivityInfo = await _context.BaseActivityInfos.FindAsync(id);
                return Ok(updatedBaseActivityInfo); 
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BaseActivityInfoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: api/BaseActivityInfos
        /// <summary>
        /// Create a new base activity
        /// Must have AdminUser-role to execute
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// The new base activity
        /// </returns>
        /// <response code="200">OK</response>
        /// <response code="204">No content</response>
        /// <response code="400">BadRequest</response>
        /// <response code="401">Forbidden</response>
        /// <response code="403">Unautherized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        // To protect from overposting attacks, enable the specific properties you want to bind to, for'
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [Authorize(Roles = "AdminUser")]
        [HttpPost]
        public async Task<ActionResult<BaseActivityInfo>> PostBaseActivityInfo(BaseActivityInfo baseActivityInfo)
        {
            if (baseActivityInfo == null)
            {
                return BadRequest();
            }
            baseActivityInfo.Theme = null;
            _context.BaseActivityInfos.Add(baseActivityInfo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBaseActivityInfo", new { id = baseActivityInfo.BaseActivityInfoId }, baseActivityInfo);
        }

        // DELETE: api/BaseActivityInfos/5
        /// <summary>
        /// Delete base activity with BaseActivityId == id
        /// Must have AdminUser-role to execute
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// The deleted base activity with BaseActivityId == id
        /// </returns>
        /// <response code="200">OK</response>
        /// <response code="204">No content</response>
        /// <response code="400">BadRequest</response>
        /// <response code="401">Forbidden</response>
        /// <response code="403">Unautherized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "AdminUser")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseActivityInfo>> DeleteBaseActivityInfo(int id)
        {
            var baseActivityInfo = await _context.BaseActivityInfos.FindAsync(id);
            if (baseActivityInfo == null)
            {
                return NotFound();
            }

            _context.BaseActivityInfos.Remove(baseActivityInfo);
            await _context.SaveChangesAsync();

            return baseActivityInfo;
        }

        private bool BaseActivityInfoExists(int id)
        {
            return _context.BaseActivityInfos.Any(e => e.BaseActivityInfoId == id);
        }
    }
}
