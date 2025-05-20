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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DataNexApi.Controllers
{
    [Authorize]
    public class AppPermissionsController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public AppPermissionsController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.AppPermissions.Where(x => x.CompanyId == companyId).ToListAsync();

            var dto = _mapper.Map<AppPermissionDto[]>(data);

            return Ok(dto);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] AppPermissionDto dto)
        {
            Guid companyId = GetCompanyFromHeader();
            var actionUser = await GetActionUser();

            var source = await _context.AppPermissions.Where(x => x.CompanyId == companyId && x.Key == dto.Key && x.MasterEntityId==dto.MasterEntityId).FirstOrDefaultAsync();
            if (source == null)
            {

                var data = new AppPermission();
                data.Code = dto.Code;
                data.Name = dto.Name;
                data.AppEntity= dto.AppEntity;
                data.MasterEntityId = dto.MasterEntityId;
                data.MasterEntityDescr = dto.MasterEntityDescr;
                data.Key = dto.Key;
                data.UserAdded = actionUser.Id;
                data.CompanyId = companyId;
                lock (_lockObject)
                {
                    var maxNumber = _context.AppPermissions.Where(x => x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                    data.SerialNumber = maxNumber + 1;
                    data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                    try
                    {
                        _context.Add(data);
                        _context.SaveChanges();
                        LogService.CreateLog($"App Permission \"{data.Name}\" inserted by \"{actionUser.UserName}\"  App Permission: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                    }
                    catch (Exception ex)
                    {
                        LogService.CreateLog($"App Permission \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\"  App Permission: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                        throw;
                    }

                };

                var dataToReturn = _mapper.Map<AppPermissionDto>(data);
                return Ok(dataToReturn);

            }
            else
            {
                return BadRequest("Record already exists");

            }
        }


        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] AppPermissionDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var source = await _context.AppPermissions.FirstOrDefaultAsync(x => x.Id == dto.Id && x.Key==dto.Key && x.CompanyId == companyId);
            if (source == null)
            {

            var data = await _context.AppPermissions.FirstOrDefaultAsync(x => x.Id == dto.Id && x.CompanyId == companyId);

            data.Code = dto.Code;
            data.Name = dto.Name;
            data.Key = dto.Key;
            data.AppEntity = dto.AppEntity;
            data.MasterEntityId = dto.MasterEntityId;
            data.MasterEntityDescr = dto.MasterEntityDescr;
            data.UserAdded = actionUser.Id;
            data.CompanyId = companyId;
            data.UserUpdated = actionUser.Id;
            data.DateUpdated = DateTime.Now;
            data.CompanyId = companyId;

            try
            {
                await _context.SaveChangesAsync();

                LogService.CreateLog($"App Permission \"{data.Name}\" updated by \"{actionUser.UserName}\". App Permission: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"App Permission \"{data.Name}\" could not be updated by \"{actionUser.UserName}\"  App Permission: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            return Ok(data);

            }
            else
            {
                return BadRequest("Permission Key already exists");

            }
        }


        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.AppPermissions.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            try
            {
                _context.AppPermissions.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"App Permission \"{data.Name}\" deleted by \"{actionUser.UserName}\"  App Permission: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"App Permission \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  App Permission: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }
    }
}
