using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataNexApi.Controllers
{
    public class LoginController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public LoginController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> GetAll([FromBody]LoginDto dto)
        {
            var success = false;
            var user = await _context.Users.Where(x => x.Username == dto.Username).FirstOrDefaultAsync();
            if (user != null && dto.Password != null)
            {
                success = BCrypt.Net.BCrypt.EnhancedVerify(dto.Password, user.PasswordHash);
            
            }
           
            if(success)
            {
                return Ok(success);

            }
            else
            {
                return BadRequest("Username or Password is incorrect");
            }

        }
    }
}
