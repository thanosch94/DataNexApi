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
    public class UserAppPermissionsController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public UserAppPermissionsController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.UserAppPermissions.Where(x => x.CompanyId == companyId).ToListAsync();

            var dto = _mapper.Map<UserAppPermissionDto[]>(data);

            return Ok(dto);
        }
        

        [HttpGet("getByUserId/{id}")]
        public async Task<IActionResult> GetByUserId(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.UserAppPermissions.Where(x => x.CompanyId == companyId && x.UserId==id).ToListAsync();

            var dto = _mapper.Map<UserAppPermissionDto[]>(data);

            return Ok(dto);
        }


        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] UserAppPermissionDto dto)
        {
            Guid companyId = GetCompanyFromHeader();
            var actionUser = await GetActionUser();

            var source = await _context.UserAppPermissions.Where(x => x.CompanyId == companyId && x.AppPermissionId==dto.AppPermissionId && x.UserId==dto.UserId).FirstOrDefaultAsync();
            if (source == null)
            {

                var data = new UserAppPermission();
                data.AppPermissionId = dto.AppPermissionId;
                data.UserId = dto.UserId;
                data.CompanyId = dto.CompanyId;
                data.UserAdded = actionUser.Id;
                data.CompanyId = companyId;
                lock (_lockObject)
                {
                    var maxNumber = _context.UserAppPermissions.Where(x => x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                    data.SerialNumber = maxNumber + 1;
                    data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                    try
                    {
                        _context.Add(data);
                        _context.SaveChanges();
                        LogService.CreateLog($"User App Permission \"{data.AppPermissionId}\" for user \"{data.UserId}\" inserted by \"{actionUser.UserName}\" User App Permission: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                    }
                    catch (Exception ex)
                    {
                        LogService.CreateLog($"User App Permission \"{data.AppPermissionId}\" for user \"{data.UserId}\" could not be inserted by \"{actionUser.UserName}\" User App Permission: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                        throw;
                    }

                };

                await _context.Entry(data).Reference(e => e.User).LoadAsync();
                var dataToReturn = _mapper.Map<UserAppPermissionDto>(data);
                dataToReturn.UserName = data.User.Name;
                return Ok(dataToReturn);

            }
            else
            {
                return BadRequest("Record already exists");

            }
        }



        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] UserAppPermissionDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var source = await _context.UserAppPermissions.FirstOrDefaultAsync(x => x.Id == dto.Id && x.CompanyId == companyId);
            if (source == null)
            {

                var data = await _context.UserAppPermissions.FirstOrDefaultAsync(x => x.Id == dto.Id && x.CompanyId == companyId);

                data.AppPermissionId = dto.AppPermissionId;
                data.UserId = dto.UserId;
               
                data.UserAdded = actionUser.Id;
                data.CompanyId = companyId;
                data.UserUpdated = actionUser.Id;
                data.DateUpdated = DateTime.Now;
                data.CompanyId = companyId;

                try
                {
                    await _context.SaveChangesAsync();

                    LogService.CreateLog($"User App Permission \"{data.Id}\" updated by \"{actionUser.UserName}\". User App Permission: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"User App Permission \"{data.Id}\" could not be updated by \"{actionUser.UserName}\". User App Permission: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }

                return Ok(data);

            }
            else
            {
                return BadRequest("User Permission already exists");

            }
        }



        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.UserAppPermissions.Include(x=>x.User).FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            var userName = data.User.Name;
            try
            {
                _context.UserAppPermissions.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"User App Permission \"{data.Id}\" deleted by \"{actionUser.UserName}\" User App Permission: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"User App Permission \"{data.Id}\" could not be deleted by \"{actionUser.UserName}\" User App Permission: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            var dataToReturn = _mapper.Map<UserAppPermissionDto>(data);
            dataToReturn.UserName = userName;

            return Ok(dataToReturn);
        }
    }
}
