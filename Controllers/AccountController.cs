using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using DataNexApi.Services;
using DataNex.Model.Enums;
using DataNex.Model.Models;
using Microsoft.AspNetCore.Http;
using DataNex.Model.Dtos.TimeZone;
using Newtonsoft.Json;

namespace DataNexApi.Controllers
{
    [AllowAnonymous]
    public class AccountController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public AccountController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]LoginDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var apiResponse = new ApiResponseDto();

            var success = false;
            var user = await _context.Users.Where(x => x.UserName == dto.UserName && x.CompanyId==dto.CompanyId).FirstOrDefaultAsync();
            if (user != null && dto.Password != null)
            {
                success = BCrypt.Net.BCrypt.EnhancedVerify(dto.Password, user.PasswordHash);
            }
           
            if(success)
            {
                LogService.CreateLog($"User \"{user.UserName}\" logged in successfully.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, user.Id, _context);
                var userToReturn = new UserDto()
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    UserName = user.UserName,
                    UserRole = user.UserRole,
                    Token = TokenService.GenerateToken(user)
                };
                apiResponse.Success = success;
                apiResponse.Result = userToReturn;

                SetUserTimeZone(user);

                return Ok(apiResponse);

            }
            else
            {
                LogService.CreateLog($"Failed login attempt \"{dto.UserName}\".", LogTypeEnum.Warning, LogOriginEnum.DataNexApp, null, _context); ;

                return BadRequest("Username or Password is incorrect");
            }

        }

      
        private void SetUserTimeZone(User user)
        {
            var offsetMinutes = GetUserTimeZone();

            TimeZoneSettings.UserOffsetHours = -(offsetMinutes/60);

        }


    }
}
