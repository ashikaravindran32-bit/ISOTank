using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Dapper;
using ISOTankAPI.Models;

namespace ISOTankAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChecklistController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ChecklistController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("template")]
        public async Task<IActionResult> GetChecklistTemplate()
        {
            try
            {
                using var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                
                // Fetch Jobs (Categories)
                var jobs = await connection.QueryAsync<ChecklistGroup>(
                    "SELECT id as JobId, job_description as JobName FROM inspection_job ORDER BY sort_order, id");

                // Fetch Sub Jobs (Items)
                var subJobs = await connection.QueryAsync(
                    "SELECT sub_job_id as SubJobId, job_id as JobId, sn as Sn, sub_job_name as SubJobName FROM inspection_sub_job ORDER BY job_id, CAST(SUBSTRING_INDEX(sn, '.', -1) AS UNSIGNED)");

                var groups = jobs.ToList();
                foreach (var group in groups)
                {
                    group.Items = subJobs.Where(s => s.JobId == group.JobId).Select(s => new ChecklistItem
                    {
                        SubJobId = s.SubJobId,
                        Sn = s.Sn,
                        SubJobName = s.SubJobName
                    }).ToList();
                }

                return Ok(groups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
