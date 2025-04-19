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
    public class StatusesController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public StatusesController(ApplicationDbContext context, IMapper mapper):base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.Statuses.Where(x=>x.CompanyId==companyId).OrderBy(x => x.Order ?? short.MaxValue).ToListAsync();

            return Ok(data);
        }

        [HttpGet("getallByStatusType/{statusType}")]
        public async Task<IActionResult> GetAllByStatusType(StatusTypeEnum statusType)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.Statuses.Where(x => x.CompanyId == companyId && x.StatusType==statusType).OrderBy(x=>x.Order?? short.MaxValue).ToListAsync();

            return Ok(data);
        }
        
  


        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] StatusDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = new Status();

            var exists = await _context.Statuses.Where(x => x.Name == dto.Name && x.CompanyId == companyId).FirstOrDefaultAsync();
            if (exists == null)
            {
                data.Name = dto.Name;
                data.StatusType = dto.StatusType;
                data.Icon = dto.Icon;
                data.IconColor = dto.IconColor;
                data.Order = dto.Order;
                data.IsDefault = dto.IsDefault;
                data.UserAdded = actionUser.Id;
                data.CompanyId = companyId;

                lock (_lockObject)
                {
                    var maxNumber = _context.Statuses.Where(x => x.CompanyId == companyId && x.StatusType == dto.StatusType).Max(x => (x.SerialNumber)) ?? 0;
                    data.SerialNumber = maxNumber + 1;
                    data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                    try
                    {
                        _context.Statuses.Add(data);
                        _context.SaveChanges();
                        LogService.CreateLog($"Status \"{data.Name}\" inserted by \"{actionUser.UserName}\". Status: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                    }
                    catch (Exception ex)
                    {
                        LogService.CreateLog($"Status \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\". Status: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                        throw;
                    }
                };
                return Ok(data);
            }
            else
            {
                return BadRequest("Status already exists");
            }

        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] StatusDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.Statuses.Where(x => x.Id == dto.Id && x.CompanyId == companyId).FirstOrDefaultAsync();

            data.Name = dto.Name;
            data.StatusType = dto.StatusType;
            data.Icon = dto.Icon;
            data.IconColor = dto.IconColor;
            data.Order = dto.Order;
            data.IsDefault = dto.IsDefault;
            data.CompanyId = companyId;

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Status \"{data.Name}\" updated by \"{actionUser.UserName}\". Status: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Status \"{data.Name}\" could not be updated by \"{actionUser.UserName}\". Status: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            return Ok(data);

        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.Statuses.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            try
            {
                _context.Statuses.Remove(data);

                await _context.SaveChangesAsync();
                LogService.CreateLog($"Status \"{data.Name}\" deleted by \"{actionUser.UserName}\". Status: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Status \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\". Status: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }
                return Ok(data);
        }
    }
}
