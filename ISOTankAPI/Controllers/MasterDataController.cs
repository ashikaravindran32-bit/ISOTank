using Microsoft.AspNetCore.Mvc;
using Dapper;
using MySql.Data.MySqlClient;
using ISOTankAPI.Models;

namespace ISOTankAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasterDataController : ControllerBase
    {
        private readonly string _connectionString;

        public MasterDataController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        [HttpGet]
        public async Task<IActionResult> GetMasterData()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                
                var response = new MasterDataResponse();

                // Fetch Tank Statuses
                response.TankStatuses = (await connection.QueryAsync<MasterItem>(
                    "SELECT id as Id, status_name as Name FROM tank_status")).ToList();

                // Fetch Inspection Types
                response.InspectionTypes = (await connection.QueryAsync<MasterItem>(
                    "SELECT id as Id, inspection_type_name as Name FROM inspection_type")).ToList();

                // Fetch Locations
                response.Locations = (await connection.QueryAsync<MasterItem>(
                    "SELECT id as Id, location_name as Name FROM location_master")).ToList();

                // Fetch Safety Valve Brands
                response.SafetyValveBrands = (await connection.QueryAsync<MasterItem>(
                    "SELECT id as Id, brand_name as Name FROM safety_valve_brand")).ToList();

                // Fetch Image Types (Photo categories)
                response.ImageTypes = (await connection.QueryAsync<MasterItem>(
                    "SELECT id as Id, image_type as Name FROM image_type")).ToList();

                // Fetch Tanks with Specs
                response.Tanks = (await connection.QueryAsync<TankItem>(
                    @"SELECT 
                        t.id as Id, 
                        t.tank_number as Name, 
                        t.capacity_l as CapacityL, 
                        t.standard as Standard, 
                        t.working_pressure as WorkingPressure, 
                        t.gross_kg as GrossKg, 
                        t.tare_weight_kg as TareWeightKg,
                        t.mfgr as MfgrName,
                        t.ownership as Ownership,
                        t.design_temperature as Temp,
                        t.frame_type as Frame,
                        t.cabinet_type as Cabinet,
                        COALESCE(s.brand_name, 'N/A') as SVBrand
                      FROM tank_details t
                      LEFT JOIN safety_valve_brand s ON t.safety_valve_brand_id = s.id
                      WHERE t.status = 'active'")).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
