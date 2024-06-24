using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Dtos.Connector;
using DataNex.Model.Enums;
using DataNex.Model.Models;
using DataNexApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DataNexApi.Controllers.Connector
{
    [Authorize]
    public class WooConnectionsController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;

        public WooConnectionsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.WooConnectionsData.ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] WooConnectionsDataDto wooConnectionsDataDto)
        {
            var actionUser = await GetActionUser();

            var data = new WooConnectionsData();
            data.Name = wooConnectionsDataDto.Name;
            data.RequestType = wooConnectionsDataDto.RequestType;
            data.Endpoint = wooConnectionsDataDto.Endpoint;

            try
            {
                _context.WooConnectionsData.Add(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Woo Connection data inserted by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Woo Connection data could not be inserted by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }


            var dto = _mapper.Map<WooConnectionsDataDto>(data);

            return Ok(dto);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] WooConnectionsDataDto wooConnectionsDataDto)
        {
            var actionUser = await GetActionUser();

            var data = await _context.WooConnectionsData.FirstOrDefaultAsync(x => x.Id == wooConnectionsDataDto.Id);
            data.Name = wooConnectionsDataDto.Name;
            data.RequestType = wooConnectionsDataDto.RequestType;
            data.Endpoint = wooConnectionsDataDto.Endpoint;
          
            data.UserUpdated = actionUser.Id;
            data.DateUpdated = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();

                LogService.CreateLog($"Woo Connection data updated by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Woo Connection data could not be updated by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            var dto = _mapper.Map<WooConnectionsDataDto>(data);

            return Ok(dto);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.WooConnectionsData.FirstOrDefaultAsync(x => x.Id == id);

            try
            {
                _context.WooConnectionsData.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Woo Connection data deleted by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Woo Connection data could not be deleted by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }
    }
}
