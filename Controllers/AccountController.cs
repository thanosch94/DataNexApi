using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataNexApi.Controllers
{
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
            var apiResponse = new ApiResponseDto();

            var success = false;
            var user = await _context.Users.Where(x => x.Username == dto.Username).FirstOrDefaultAsync();
            if (user != null && dto.Password != null)
            {
                success = BCrypt.Net.BCrypt.EnhancedVerify(dto.Password, user.PasswordHash);
            }
           
            if(success)
            {
                var userToReturn = new UserDto()
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Username = user.Username,
                    UserRole = user.UserRole,
                };
                apiResponse.Success = success;
                apiResponse.Result = userToReturn;
                return Ok(apiResponse);

            }
            else
            {
                return BadRequest("Username or Password is incorrect");
            }

        }

      
    }
}
