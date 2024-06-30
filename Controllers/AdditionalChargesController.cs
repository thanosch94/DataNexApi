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
    public class AdditionalChargesController : BaseController
    {

        private ApplicationDbContext _context;
        private IMapper _mapper;
        public AdditionalChargesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.AdditionalCharges.ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] AdditionalChargeDto additionalCharge)
        {
            var actionUser = await GetActionUser();

            var data = new AdditionalCharge();
            data.Name = additionalCharge.Name;

            try
            {
                _context.AdditionalCharges.Add(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Additional Charge \"{data.Name}\" inserted by \"{actionUser.UserName}\"  Additional Charge: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Additional Charge \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\"  Additional Charge: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            var dto = _mapper.Map<AdditionalChargeDto>(data);

            return Ok(dto);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] AdditionalChargeDto additionalCharge)
        {
            var actionUser = await GetActionUser();

            var data = await _context.AdditionalCharges.FirstOrDefaultAsync(x => x.Id == additionalCharge.Id);

            data.Name = additionalCharge.Name;
            data.UserUpdated = actionUser.Id;
            data.DateUpdated = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();

                LogService.CreateLog($"Additional Charge \"{data.Name}\" updated by \"{actionUser.UserName}\"  Additional Charge: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Additional Charge \"{data.Name}\" could not be updated by \"{actionUser.UserName}\"  Additional Charge: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            return Ok(data);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.AdditionalCharges.FirstOrDefaultAsync(x => x.Id == id);

            try
            {
                _context.AdditionalCharges.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Additional Charge \"{data.Name}\" deleted by \"{actionUser.UserName}\"  Additional Charge: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Additional Charge \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  Additional Charge: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }
    }
}
