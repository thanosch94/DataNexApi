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
using System.ComponentModel.DataAnnotations;

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

            var data = await _context.AppPermissions.Where(x => x.CompanyId == companyId).Select(x => new AppPermissionDto()
            {
                Id = x.Id,
                Name = x.Name,
                Key = x.Key,
                AppEntity = x.AppEntity,
                MasterEntityId = x.MasterEntityId,
                MasterEntityDescr = x.MasterEntityDescr,
                CompanyId = x.CompanyId,
                UserAppPermissions = _context.UserAppPermissions.Include(y => y.User).Where(y => y.AppPermissionId == x.Id).Select(y => new UserAppPermissionDto()
                {
                    Id = y.Id,
                    AppPermissionId = y.AppPermissionId,
                    UserId = y.UserId,
                    UserName = y.User.Name,
                    CompanyId = y.CompanyId,
                }).ToList()
            }).ToListAsync();


            return Ok(data);
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

                var users = await _context.Users.Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();

                lock (_lockObject)
                {
                    var maxNumber = _context.AppPermissions.Where(x => x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                    data.SerialNumber = maxNumber + 1;
                    data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                    data.UserAppPermissions = new List<UserAppPermission>();
            

                    foreach(var user in users)
                    {
                        var userAppPermission = new UserAppPermission();
                        userAppPermission.UserId = user.Id;
                        userAppPermission.AppPermissionId = data.Id;
                        userAppPermission.CompanyId = companyId;
                        data.UserAppPermissions.Add(userAppPermission);
                    }
                    try
                    {
                        _context.Add(data);
                        _context.AddRange(data.UserAppPermissions);

                        _context.SaveChanges();


                        LogService.CreateLog($"App Permission \"{data.Name}\" inserted by \"{actionUser.UserName}\"  App Permission: {JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                        })}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                    }
                    catch (Exception ex)
                    {
                        LogService.CreateLog($"App Permission \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\"  App Permission: {JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                        })} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                        throw;
                    }

                };

                var dataToReturn = _mapper.Map<AppPermissionDto>(data);

                foreach (var userPermission in dataToReturn.UserAppPermissions)
                {
                    userPermission.UserName = users.Where(x => x.Id == userPermission.UserId).FirstOrDefault()?.Name;
                }
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

            var userAppPermissions = await _context.UserAppPermissions.Where(x=>x.AppPermissionId== id).ToListAsync();  

            try
            {
                _context.RemoveRange(userAppPermissions);

                _context.AppPermissions.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"App Permission \"{data.Name}\" deleted by \"{actionUser.UserName}\"  App Permission: {JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                })}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"App Permission \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  App Permission: {JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                })} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            var dataToReturn = _mapper.Map<AppPermissionDto>(data);

            return Ok(dataToReturn);
        }
    }
}
