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

        // GET: api/Clients
        // can remove probs, already have a duplicate method below with proper route
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Client>>> GetClient()
        //{
        //    return await _context.Client.ToListAsync();
        //}

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

        // PUT: api/Clients/5
        //shouldn't be able to update clients not in the spec
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutClient(int id, Client client)
        //{
        //    if (id != client.clientId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(client).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ClientExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/Clients
        //this method is also duplicate, since below i have a register format specified already?
        //[HttpPost]
        //public async Task<ActionResult<Client>> PostClient(Client client)
        //{
        //    _context.Client.Add(client);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetClient", new { id = client.clientId }, client);
        //}

        // DELETE: api/Clients/5
        [HttpDelete("{id}")]
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

        private bool ClientExists(int id)
        {
            return _context.Client.Any(e => e.clientId == id);
        }

        [HttpPost]
        [Route("Register/{ip}/{port}")]
        public async Task<ActionResult<Client>> RegisterClient(string ip, int port)
        {
            var client = new Client { ip = ip, port = port };
            _context.Client.Add(client);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetClient", new { id = client.clientId }, client);
        }

        [HttpGet]
        [Route("GetAllClients")]
        public async Task<ActionResult<IEnumerable<Client>>> GetAllClients()
        {
            return await _context.Client.ToListAsync();
        }

        [HttpGet]
        [Route("GetOtherClients/{id}")]
        public async Task<ActionResult<IEnumerable<Client>>> GetOtherClients(int id)
        {
            return await _context.Client.Where(c => c.clientId != id).ToListAsync();// exclude the client with the specified id in list returned
        }
    }
}
