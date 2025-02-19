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
        private readonly AppDbContext _context;//
		private readonly ProducerKafka _kafkaProducer;
		private readonly ServiceElastic _elasticsearchService;
		private readonly ILogger<PermissionsController> _logger;

		public PermissionsController(AppDbContext context, ServiceElastic elasticsearchService, ILogger<PermissionsController> logger)
        {
            _context = context;
			_kafkaProducer = new ProducerKafka("kafka:9092", "permissions_topic");
			_elasticsearchService = elasticsearchService;
			_logger = logger;

		}

        // GET: api/Permissions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permissions>>> GetPermissions()
        {
			_logger.LogInformation("GetPermissions..");
			_logger.LogInformation("Permissions List "+ _context.Permissions.ToListAsync());
			return await _context.Permissions.ToListAsync();
        }

        // GET: api/Permissions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Permissions>> GetPermissions(int id)
        {
			_logger.LogInformation("Permissions Find by Id: "+id);
			var permissions = await _context.Permissions.FindAsync(id);

			_logger.LogInformation("Id found: " + _context.Permissions.FindAsync(id));

			if (permissions == null)
            {
				_logger.LogInformation("Id Not Found: " + id);

				return NotFound();
            }

            return permissions;
        }

        // PUT: api/Permissions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPermissions(int id, Permissions permissions)
        {
			_logger.LogInformation("PutPermissions by id: " + id);


			if (id != permissions.Id)
            {
				_logger.LogError("Error BadRequest, diference between: "+id +" and "+permissions.Id);

				return BadRequest();
            }

            _context.Entry(permissions).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

				_logger.LogInformation("Permissions Saved");
			}
            catch (DbUpdateConcurrencyException ex)
            {
				_logger.LogError("Error Method PutPermissions: "+ex.Message);

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
          

			_logger.LogInformation("Saving Permissions..");


			try
			{
				_context.Permissions.Add(permissions);
				await _context.SaveChangesAsync();

				//{permissions.Id}

				//Se envia la alerta de guardado a kafka
				await _kafkaProducer.SendMessageAsync($"New Permission Saved: {permissions.Id}");

				//Elastic
				await _elasticsearchService.IndexPermissionAsync(permissions);

				//log
				_logger.LogInformation("Permissions save sucessfully");

				return CreatedAtAction("GetPermissions", new { id = permissions.Id }, permissions);
			}
			catch (Exception ex)
			{
				_logger.LogError("ERROR Method PostPermissions Exception: "+ex.Message);

				return null;
			}
			
        }

        // DELETE: api/Permissions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermissions(int id)
        {
			_logger.LogInformation("Permissions Remove by id "+id);

			var permissions = await _context.Permissions.FindAsync(id);
            if (permissions == null)
            {
                return NotFound();
            }

            _context.Permissions.Remove(permissions);
            await _context.SaveChangesAsync();

			_logger.LogInformation("Permissions removed sucessfully" );

			return NoContent();
        }

        private bool PermissionsExists(int id)
        {
            return _context.Permissions.Any(e => e.Id == id);
        }
    }
}
