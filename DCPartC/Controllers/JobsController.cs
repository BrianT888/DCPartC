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
    public class JobsController : ControllerBase
    {
        private readonly DBManager _context;

        public JobsController(DBManager context)
        {
            _context = context;
        }

        // GET: api/Jobs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Job>>> GetJob()
        {
            return await _context.Job.ToListAsync();
        }

        // GET: api/Jobs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Job>> GetJob(int id)
        {
            var job = await _context.Job.FindAsync(id);

            if (job == null)
            {
                return NotFound();
            }

            return job;
        }

        // POST: api/Jobs
        [HttpPost]
        public async Task<ActionResult<Job>> PostJob(Job job)
        {
            _context.Job.Add(job);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetJob", new { id = job.jobId }, job);
        }

        // DELETE: api/Jobs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _context.Job.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            _context.Job.Remove(job);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool JobExists(int id)
        {
            return _context.Job.Any(e => e.jobId == id);
        }

        [HttpGet]
        [Route("GetJobsByClientId/{id}")]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobsByClientId(int id)
        {
            var jobs = await _context.Job.Where(j => j.clientId == id).ToListAsync();
            if (jobs == null || jobs.Count == 0)
            {
                return NotFound($"No Jobs by Client {id}");
            }
            return jobs;
        }

        [HttpPut]
        [Route("FinishedJob/{result}")]
        public async Task<IActionResult> FinishedJob(string result, [FromBody]Job job)
        {
            if(job == null)
            {
                return NotFound("Job not found");
            }
            Job currentJob = await _context.Job.FindAsync(job.jobId);
            currentJob.Result = result;
            currentJob.jobStatus = job.jobStatus;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
