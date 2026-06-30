using Microsoft.AspNetCore.Mvc;
using Dapper;
using MySql.Data.MySqlClient;
using ISOTankAPI.Models;

namespace ISOTankAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InspectionController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public InspectionController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        [HttpPost("submit-full")]
        public async Task<IActionResult> SubmitFull([FromBody] InspectionPayload payload)
        {
            try
            {
                // First save basic info to get the history ID
                var basicInfoResult = await SaveBasicInfo(payload.BasicInfo) as OkObjectResult;
                if (basicInfoResult?.Value is not InspectionBasicInfo savedBasicInfo)
                {
                    return StatusCode(500, "Failed to save basic info");
                }

                int historyId = savedBasicInfo.Id;

                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();

                try
                {
                    // 1. Save Photos
                    await connection.ExecuteAsync(@"
                        CREATE TABLE IF NOT EXISTS inspection_photos (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            history_id INT NOT NULL,
                            category_key VARCHAR(50),
                            image_path VARCHAR(255),
                            is_marked BOOLEAN
                        );", transaction: transaction);

                    await connection.ExecuteAsync("DELETE FROM inspection_photos WHERE history_id = @HistoryId", new { HistoryId = historyId }, transaction);

                    foreach (var photo in payload.Photos)
                    {
                        await connection.ExecuteAsync(@"
                            INSERT INTO inspection_photos (history_id, category_key, image_path, is_marked)
                            VALUES (@HistoryId, @CategoryKey, @ImagePath, @IsMarked)",
                            new { HistoryId = historyId, photo.CategoryKey, photo.ImagePath, photo.IsMarked }, transaction);
                    }

                    // 2. Save Checklist Items
                    await connection.ExecuteAsync(@"
                        CREATE TABLE IF NOT EXISTS inspection_checklist_results (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            history_id INT NOT NULL,
                            sub_job_id INT NOT NULL,
                            is_faulty BOOLEAN,
                            comment TEXT,
                            assigned_photos VARCHAR(500)
                        );", transaction: transaction);

                    await connection.ExecuteAsync("DELETE FROM inspection_checklist_results WHERE history_id = @HistoryId", new { HistoryId = historyId }, transaction);

                    foreach (var item in payload.ChecklistItems)
                    {
                        var assignedPhotosStr = string.Join(",", item.AssignedPhotoKeys);
                        await connection.ExecuteAsync(@"
                            INSERT INTO inspection_checklist_results (history_id, sub_job_id, is_faulty, comment, assigned_photos)
                            VALUES (@HistoryId, @SubJobId, @IsFaulty, @Comment, @AssignedPhotos)",
                            new { HistoryId = historyId, item.SubJobId, item.IsFaulty, item.Comment, AssignedPhotos = assignedPhotosStr }, transaction);
                    }

                    await transaction.CommitAsync();
                    return Ok(savedBasicInfo);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("basic-info")]
        public async Task<IActionResult> SaveBasicInfo([FromBody] InspectionBasicInfo info)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                
                if (info.Id > 0)
                {
                    // UPDATE existing record
                    var updateQuery = @"
                        UPDATE inspection_history 
                        SET tank_number = @TankNumber,
                            frame_type = @FrameType,
                            cabinet_type = @CabinetType,
                            mfgr = @Manufacturer,
                            vacuum_reading = @VacuumReading,
                            notes = @Notes,
                            status_id = @StatusId,
                            inspection_type_id = @InspectionTypeId,
                            location_id = @LocationId,
                            safety_valve_brand_id = @SafetyValveBrandId
                        WHERE id = @Id;";
                    await connection.ExecuteAsync(updateQuery, info);
                    return Ok(info);
                }
                else
                {
                    // INSERT new record
                    if (string.IsNullOrEmpty(info.ReportNumber))
                    {
                        info.ReportNumber = $"SG-T1-{DateTime.Now:ddMMyyyy}-{new Random().Next(10, 99)}";
                    }

                    var insertQuery = @"
                        INSERT INTO inspection_history 
                        (inspection_id, report_number, tank_number, frame_type, cabinet_type, mfgr, vacuum_reading, notes, status_id, inspection_type_id, location_id, safety_valve_brand_id) 
                        VALUES 
                        (@InspectionId, @ReportNumber, @TankNumber, @FrameType, @CabinetType, @Manufacturer, @VacuumReading, @Notes, @StatusId, @InspectionTypeId, @LocationId, @SafetyValveBrandId);
                        SELECT LAST_INSERT_ID();";

                    var newId = await connection.ExecuteScalarAsync<int>(insertQuery, info);
                    info.Id = newId;
                    return Ok(info);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("basic-info/{id}")]
        public async Task<IActionResult> GetBasicInfo(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var query = @"
                    SELECT id as Id,
                           inspection_id as InspectionId,
                           report_number as ReportNumber,
                           tank_number as TankNumber,
                           frame_type as FrameType,
                           cabinet_type as CabinetType,
                           mfgr as Manufacturer,
                           vacuum_reading as VacuumReading,
                           notes as Notes,
                           status_id as StatusId,
                           inspection_type_id as InspectionTypeId,
                           location_id as LocationId,
                           safety_valve_brand_id as SafetyValveBrandId
                    FROM inspection_history 
                    WHERE id = @Id;";

                var result = await connection.QueryFirstOrDefaultAsync<InspectionBasicInfo>(query, new { Id = id });
                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
