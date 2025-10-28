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
    public class LotSettingsController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public LotSettingsController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.LotsSettings.Where(x => x.CompanyId == companyId).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] LotSettingsDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = new LotSettings();

            var exists = await _context.LotsSettings.Where(x => x.CompanyId == companyId).FirstOrDefaultAsync();
            if (exists == null)
            {
                data.LotStrategy = dto.LotStrategy;
                data.LotStrategyApplyField = dto.LotStrategyApplyField;
                data.UserAdded = actionUser.Id;
                data.CompanyId = companyId;

                lock (_lockObject)
                {
                    var maxNumber = _context.LotsSettings.Where(x => x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                    data.SerialNumber = maxNumber + 1;
                    data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                    try
                    {
                        _context.LotsSettings.Add(data);
                        _context.SaveChanges();
                        LogService.CreateLog($"Lot Settings inserted by \"{actionUser.UserName}\". Lot Settings: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                    }
                    catch (Exception ex)
                    {
                        LogService.CreateLog($"Lot Settings could not be inserted by \"{actionUser.UserName}\". Lot Settings: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                        throw;
                    }
                };
                return Ok(data);
            }
            else
            {
                return BadRequest("Lot Settings already exists");
            }

        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] LotSettingsDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.LotsSettings.Where(x => x.Id == dto.Id && x.CompanyId == companyId).FirstOrDefaultAsync();

            data.LotStrategy = dto.LotStrategy;
            data.LotStrategyApplyField = dto.LotStrategyApplyField;
            data.CompanyId = companyId;

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Lot Settings updated by \"{actionUser.UserName}\". Lot Settings: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Lot Settings could not be updated by \"{actionUser.UserName}\". Lot Settings: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            return Ok(data);

        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.LotsSettings.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            try
            {
                _context.LotsSettings.Remove(data);

                await _context.SaveChangesAsync();
                LogService.CreateLog($"Lot Settings deleted by \"{actionUser.UserName}\". Lot Settings: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Lot Settings could not be deleted by \"{actionUser.UserName}\". Lot Settings: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }
            return Ok(data);
        }

    }
}
