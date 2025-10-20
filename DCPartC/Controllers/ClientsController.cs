using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DCPartC.Data;
using APIClasses;

namespace DCPartC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly DBManager _context;

        public ClientsController(DBManager context)
        {
            _context = context;
        }

        // GET: api/Clients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var client = await _context.Client.FindAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            return client;
        }

        // DELETE: api/Clients/5
        [HttpDelete]
        [Route("UseID/{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Client.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            _context.Client.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        [Route("UsePort/{port}")]
        public async Task<IActionResult> DeleteClientPort(int port)
        {
            var client = await _context.Client.FirstOrDefaultAsync(c => c.port == port);
            if (client == null)
            {
                return NotFound();
            }

            _context.Client.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        private bool ClientExists(int id)
        {
            return _context.Client.Any(e => e.clientId == id);
        }

        [HttpPost]
        [Route("Register/{ip}/{port}")]
        public async Task<ActionResult<Client>> RegisterClient(string ip, int port)
        {
            var eclient = await _context.Client.FirstOrDefaultAsync(c => c.port == port);
            if (eclient != null)//client found with the port already exists
            {
                return BadRequest("Duplicate port found");
            }

            var client = new Client { ip = ip, port = port, jobsFinished = 0 };
            _context.Client.Add(client);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetClient", new { id = client.clientId }, client);
        }

        [HttpGet]
        [Route("GetAllClients")]
        public async Task<ActionResult<IEnumerable<Client>>> GetAllClients()
        {
            List<Client> clientList = await _context.Client.ToListAsync();
            if (clientList.Count() < 1)
            {
                return NotFound("Empty Client list");
            }
            return clientList;
        }

        [HttpGet]
        [Route("GetOtherClients/{id}")]
        public async Task<ActionResult<IEnumerable<Client>>> GetOtherClients(int id)
        {
            List<Client> clientList = await _context.Client.Where(c => c.clientId != id).ToListAsync();// exclude the client with the specified id in list returned
            if (clientList.Count() < 1)
            {
                return NotFound("No Other Clients");
            }

            return clientList;
        }

        [HttpPut]
        [Route("FinishedJob/{id}")]
        public async Task<IActionResult> FinishedJob(int id)
        {
            Client client = await _context.Client.FindAsync(id);
            if (client == null)
            {
                return NotFound("Client was not found");
            }
            client.jobsFinished++;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
