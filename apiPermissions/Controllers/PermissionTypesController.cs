using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using apiPermissions.Context;
using apiPermissions.Models;

namespace apiPermissions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionTypesController : ControllerBase
    {
        private readonly AppDbContext _context;
		private readonly ILogger<PermissionsController> _logger;

		public PermissionTypesController(AppDbContext context, ILogger<PermissionsController> logger)
        {
            _context = context;
			_logger = logger;
		}

        // GET: api/PermissionTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PermissionType>>> GetPermissionTypes()
        {
			_logger.LogInformation("GetPermissionsTypes..");
			_logger.LogInformation("Permissions List " + _context.Permissions.ToListAsync());

			return await _context.PermissionTypes.ToListAsync();
        }

        // GET: api/PermissionTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PermissionType>> GetPermissionType(int id)
        {
			_logger.LogInformation("PermissionsType Find by Id: " + id);

			var permissionType = await _context.PermissionTypes.FindAsync(id);

            if (permissionType == null)
            {
				_logger.LogInformation("PermissionsType Not Found by Id: " + id);
				return NotFound();
            }

            return permissionType;
        }

        // PUT: api/PermissionTypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPermissionType(int id, PermissionType permissionType)
        {
			_logger.LogInformation("PutPermissionsTypes by id: " + id);

			if (id != permissionType.Id)
            {
				_logger.LogError("Error BadRequest, diference between: " + id + " and " + permissionType.Id);
				return BadRequest();
            }

            _context.Entry(permissionType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
				_logger.LogInformation("PermissionsType Saved");
			}
            catch (DbUpdateConcurrencyException ex)
            {
				_logger.LogError("Error Method PutPermissionsTypes: " + ex.Message);

				if (!PermissionTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/PermissionTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PermissionType>> PostPermissionType(PermissionType permissionType)
        {
			_logger.LogInformation("Saving PermissionsTypes..");

			_context.PermissionTypes.Add(permissionType);
            await _context.SaveChangesAsync();

			_logger.LogInformation("Permissions save sucessfully");

			return CreatedAtAction("GetPermissionType", new { id = permissionType.Id }, permissionType);
        }

        // DELETE: api/PermissionTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermissionType(int id)
        {
			_logger.LogInformation("PermissionsTypes Remove by id " + id);

			var permissionType = await _context.PermissionTypes.FindAsync(id);
            if (permissionType == null)
            {
				_logger.LogError("PermissionsTypes not found by id "+id);
				return NotFound();
            }

            _context.PermissionTypes.Remove(permissionType);
            await _context.SaveChangesAsync();

			_logger.LogInformation("PermissionsTypes removed sucessfully");

			return NoContent();
        }

        private bool PermissionTypeExists(int id)
        {
            return _context.PermissionTypes.Any(e => e.Id == id);
        }
    }
}
