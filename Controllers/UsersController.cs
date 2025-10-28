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
    public class UsersController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public UsersController(ApplicationDbContext context, IMapper mapper):base(context)  
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            //DnAdmin User must not be visible in the users list
            var data = await _context.Users.Where(x=>x.Id != AppBase.DnAdmin && x.CompanyId==companyId).ToListAsync();

            return Ok(data);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.Users.Where(x=>x.Id == id && x.CompanyId == companyId).FirstOrDefaultAsync();

            var dto = _mapper.Map<UserDto>(data);
            var userRole = _context.UserRoles.FirstOrDefault(x => x.UserId == dto.Id);
            dto.UserRoleId = userRole?.RoleId;
            dto.IsPasswordSet = data?.PasswordHash != null;
            return Ok(dto);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] UserDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = new User();

            var exists = await _context.Users.Where(x => x.UserName == dto.UserName && x.CompanyId == companyId).FirstOrDefaultAsync();
            if (exists == null)
            {
                data.Name = dto.Name;
                data.Email = dto.Email;
                data.UserName = dto.UserName;
                data.Image= dto.Image;
                data.Country = dto.Country;
                data.City = dto.City;
                data.PostalCode = dto.PostalCode;
                data.Address = dto.Address;
                data.Phone1 = dto.Phone1;
                data.Phone2 = dto.Phone2;
                data.Notes = dto.Notes;
                data.Occupation = dto.Occupation;
                data.FacebookUrl = dto.FacebookUrl;
                data.InstagramUrl = dto.InstagramUrl;
                data.LinkedInUrl = dto.LinkedInUrl;
                data.UserAdded = actionUser.Id;
                data.CompanyId = companyId;
                data.IsActive = dto.IsActive;

                if (dto.BirthDay != null)
                {
                    data.BirthDay = dto.BirthDay.GetValueOrDefault().ToLocalTime();

                }
                if (dto.Password != null)
                {
                    data.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.Password);
                }

                lock (_lockObject)
                {
                    var maxNumber = _context.Users.Where(x => x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                    data.SerialNumber = maxNumber + 1;
                    data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                    try
                    {
                        _context.Users.Add(data);
                        _context.SaveChanges();
                        LogService.CreateLog($"User \"{data.Name}\" inserted by \"{actionUser.UserName}\". User: {data.Id}, {data.Name}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                        AddUserRole(data.Id, (Guid)dto.UserRoleId, _context);
                    }
                    catch (Exception ex)
                    {
                        LogService.CreateLog($"User \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\". User: {data.Id}, {data.Name} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                        throw;
                    }
                };

                var dtoData = _mapper.Map<UserDto>(data);

                if (data.PasswordHash != null)
                {
                    dtoData.IsPasswordSet = true;
                }
                return Ok(dtoData);
            }
            else
            {
                return BadRequest("Username already exists");
            }

        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] UserDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.Users.Where(x => x.Id == dto.Id && x.CompanyId == companyId).FirstOrDefaultAsync();

            data.Name = dto.Name;
            data.Email = dto.Email;
            data.UserName = dto.UserName;
            data.Image = dto.Image;
            data.Country = dto.Country;
            data.City = dto.City;
            data.PostalCode = dto.PostalCode;
            data.Address = dto.Address;
            data.Phone1 = dto.Phone1;
            data.Phone2 = dto.Phone2;
            data.Notes = dto.Notes;
            data.Occupation = dto.Occupation;
            data.FacebookUrl = dto.FacebookUrl;
            data.InstagramUrl = dto.InstagramUrl;
            data.LinkedInUrl = dto.LinkedInUrl;
            data.CompanyId = companyId;
            data.IsActive = dto.IsActive;

            if(dto.BirthDay != null)
            {
                data.BirthDay = dto.BirthDay.GetValueOrDefault().ToLocalTime();

            }

            if (dto.Password!=null)
            {
                data.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.Password);
            }


            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"User \"{data.Name}\" updated by \"{actionUser.UserName}\". User: {data.Id}, {data.Name}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                UpdateUserRole(data.Id, (Guid)dto.UserRoleId, _context);

               

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"User \"{data.Name}\" could not be updated by \"{actionUser.UserName}\"  User: {data.Id}, {data.Name} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            var dtoData = _mapper.Map<UserDto>(data);

            if (data.PasswordHash != null)
            {
                dtoData.IsPasswordSet = true;
            }

            return Ok(dtoData);

        }

        [HttpPut("updatePassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] UserDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.Users.Where(x => x.Id == dto.Id && x.CompanyId == companyId).FirstOrDefaultAsync();

           
            if (dto.Password != null)
            {
                data.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.Password);
            }

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"User \"{data.Name}\" updated by \"{actionUser.UserName}\". User: {data.Id}, {data.Name}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }
            catch (Exception ex)
            {
                LogService.CreateLog($"User \"{data.Name}\" could not be updated by \"{actionUser.UserName}\"  User: {data.Id}, {data.Name} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            var dtoData = _mapper.Map<UserDto>(data);

            if (data.PasswordHash != null)
            {
                dtoData.IsPasswordSet = true;
            }

            return Ok(dtoData);

        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.Users.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            var role = await _context.UserRoles.FirstOrDefaultAsync(x => x.UserId == data.Id);
            if(role != null)
            {
                _context.UserRoles.Remove(role);
            }

            _context.Users.Remove(data);

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"User \"{data.Name}\" deleted by \"{actionUser.UserName}\". User: {data.Id}, {data.Name}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"User \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  User: {data.Id}, {data.Name} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }

        private void AddUserRole(Guid userId, Guid roleId, ApplicationDbContext context)
        {
            var data = new UserRole();
            data.UserId = userId;
            data.RoleId = roleId;
            context.Add(data);
            context.SaveChanges();
        }        
        private void UpdateUserRole(Guid userId, Guid roleId, ApplicationDbContext context)
        {
            var userRole = _context.UserRoles.FirstOrDefault(x => x.UserId == userId);
            if (roleId != userRole?.RoleId)
            {
                userRole.RoleId = roleId;
                context.SaveChanges();
            }
        }
    }
}

