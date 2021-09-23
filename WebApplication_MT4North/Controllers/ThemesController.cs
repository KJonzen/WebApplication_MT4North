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
    public class ThemesController : ControllerBase
    {
        private readonly MT4NorthContext _context;

        public ThemesController(MT4NorthContext context)
        {
            _context = context;
        }

        // GET: api/Themes
        /// <summary>
        /// Get all available themes
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// All available themes
        /// </returns>
        /// <response code="200">OK</response>
        /// <response code="401">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [Authorize()]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Theme>>> GetThemes()
        {
            var themes = await _context.Themes.ToListAsync();
            return Ok(themes);
        }

        // GET: api/Themes/5
        /// <summary>
        /// Get theme with ThemeId == id
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// The theme with ThemeId == id
        /// </returns>
        /// <response code="200">OK</response>
        /// <response code="403">Unautherized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound)]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError)]
        [Authorize()]
        [HttpGet("{id}")]
        public async Task<ActionResult<Theme>> GetTheme(int id)
        {
            var theme = await _context.Themes.FindAsync(id);

            if (theme == null)
            {
                return NotFound();
            }

            return Ok(theme);
        }

        // PUT: api/Themes/5
        /// <summary>
        /// Update the Theme with ThemeId == id
        /// Must have AdminUser role
        /// </summary>
        /// <returns>
        /// The updated theme
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        [Authorize(Roles = "AdminUser")]
        public async Task<IActionResult> PutTheme(int id, Theme theme)
        {
            if (id != theme.ThemeId)
            {
                return BadRequest();
            }

            _context.Entry(theme).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                // return updateted object
                return Ok(theme);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ThemeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: api/Themes
        /// <summary>
        /// Create a new theme 
        /// Must have AdminUser role
        /// </summary>
        /// <returns>
        /// The new theme
        /// </returns>
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize(Roles = "AdminUser")]
        public async Task<ActionResult<Theme>> PosTheme(Theme theme)
        {
            _context.Themes.Add(theme);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTheme", new { id = theme.ThemeId }, theme);
        }

        // DELETE: api/Themes/5
        /// <summary>
        /// Delete the theme with ThemeId == id
        /// Must have AdminUser role
        /// </summary>
        /// <returns>
        /// The Updated theme
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
        [HttpDelete("{id}")]
        [Authorize(Roles = "AdminUser")]
        public async Task<ActionResult<Theme>> DeleteTheme(int id)
        {
            var theme = await _context.Themes.FindAsync(id);
            if (theme == null)
            {
                return NotFound();
            }

            _context.Themes.Remove(theme);
            await _context.SaveChangesAsync();

            return Ok(theme);
        }

        private bool ThemeExists(int id)
        {
            return _context.Themes.Any(e => e.ThemeId == id);
        }
    }
}
