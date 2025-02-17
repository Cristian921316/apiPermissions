using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using apiPermissions.Context;
using apiPermissions.Models;
using apiPermissions.Kafka;
using apiPermissions.Elastic;

namespace apiPermissions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly AppDbContext _context;
		private readonly ProducerKafka _kafkaProducer;
		private readonly ServiceElastic _elasticsearchService;
		public PermissionsController(AppDbContext context, ServiceElastic elasticsearchService)
        {
            _context = context;
			_kafkaProducer = new ProducerKafka("kafka:9092", "permissions_topic");
			_elasticsearchService = elasticsearchService;

		}

        // GET: api/Permissions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permissions>>> GetPermissions()
        {
            return await _context.Permissions.ToListAsync();
        }

        // GET: api/Permissions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Permissions>> GetPermissions(int id)
        {
            var permissions = await _context.Permissions.FindAsync(id);

            if (permissions == null)
            {
                return NotFound();
            }

            return permissions;
        }

        // PUT: api/Permissions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPermissions(int id, Permissions permissions)
        {
            if (id != permissions.Id)
            {
                return BadRequest();
            }

            _context.Entry(permissions).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PermissionsExists(id))
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

        // POST: api/Permissions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Permissions>> PostPermissions(Permissions permissions)
        {
            _context.Permissions.Add(permissions);
            await _context.SaveChangesAsync();

			//{permissions.Id}

			//Se envia la alerta de guardado a kafka
			await _kafkaProducer.SendMessageAsync($"Nueva Permiso Guardado: {permissions.Id}");

			//Elastic
			await _elasticsearchService.IndexPermissionAsync(permissions);

			return CreatedAtAction("GetPermissions", new { id = permissions.Id }, permissions);
        }

        // DELETE: api/Permissions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermissions(int id)
        {
            var permissions = await _context.Permissions.FindAsync(id);
            if (permissions == null)
            {
                return NotFound();
            }

            _context.Permissions.Remove(permissions);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PermissionsExists(int id)
        {
            return _context.Permissions.Any(e => e.Id == id);
        }
    }
}
