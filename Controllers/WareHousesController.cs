using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Enums;
using DataNex.Model.Models;
using DataNexApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DataNexApi.Controllers
{
    [Authorize]
    public class WareHousesController : BaseController
    {

        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public WareHousesController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.WareHouses.ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] WareHouseDto wareHouse)
        {
            var actionUser = await GetActionUser();

            var data = new WareHouse();

            var exists = await _context.WareHouses.Where(x => x.Name == wareHouse.Name).FirstOrDefaultAsync();
            if (exists == null)
            {
                data.Name = wareHouse.Name;
                data.UserAdded = actionUser.Id;
                data.IsDefault = wareHouse.IsDefault;
                data.CompanyId = wareHouse.CompanyId;

                lock (_lockObject)
                {
                    var maxNumber = _context.WareHouses.Max(x => (x.SerialNumber)) ?? 0;
                    data.SerialNumber = maxNumber + 1;
                    data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                    try
                    {
                        _context.WareHouses.Add(data);
                        _context.SaveChanges();
                        LogService.CreateLog($"Warehouse \"{data.Name}\" inserted by \"{actionUser.UserName}\". Warehouse: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                    }
                    catch (Exception ex)
                    {
                        LogService.CreateLog($"Warehouse \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\". Warehouse: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                        throw;
                    }
                };

                return Ok(data);
            }
            else
            {
                return BadRequest("WareHouse already exists");
            }

        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] WareHouseDto wareHouse)
        {
            var actionUser = await GetActionUser();

            var data = await _context.WareHouses.Where(x => x.Id == wareHouse.Id).FirstOrDefaultAsync();

            data.Name = wareHouse.Name;
            data.UserAdded = actionUser.Id;
            data.IsDefault = wareHouse.IsDefault;
            data.CompanyId = wareHouse.CompanyId;

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Warehouse \"{data.Name}\" updated by \"{actionUser.UserName}\". Warehouse: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Warehouse \"{data.Name}\" could not be updated by \"{actionUser.UserName}\" Warehouse: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            var dtoData = _mapper.Map<WareHouse>(data);

            return Ok(dtoData);

        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.WareHouses.FirstOrDefaultAsync(x => x.Id == id);

            _context.WareHouses.Remove(data);

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Warehouse \"{data.Name}\" deleted by \"{actionUser.UserName}\". Warehouse: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Warehouse \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  Warehouse: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }
    }

}
