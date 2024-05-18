using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataNexApi.Controllers
{
    [Authorize]
    public class UsersController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public UsersController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Users.ToListAsync();

            //var dtoData = _mapper.Map<UserDto>(data);

            return Ok(data);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.Users.Where(x=>x.Id ==id).FirstOrDefaultAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] UserDto dto)
        {
            var data = new User();

            var exists = await _context.Users.Where(x => x.UserName == dto.UserName).FirstOrDefaultAsync();
            if (exists == null)
            {
                data.Name = dto.Name;
                data.Email = dto.Email;
                data.UserName = dto.UserName;
                data.UserRole = dto.UserRole;
                data.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.Password);
                _context.Users.Add(data);
                await _context.SaveChangesAsync();
                var dtoData = _mapper.Map<UserDto>(data);

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
            var data = await _context.Users.Where(x => x.Id == dto.Id).FirstOrDefaultAsync();

            data.Name = dto.Name;
            data.Email = dto.Email;
            data.UserName = dto.UserName;
            data.UserRole = dto.UserRole;

            if (dto.Password!=null)
            {
                data.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();

            var dtoData = _mapper.Map<UserDto>(data);

            return Ok(dtoData);

        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var data = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

            _context.Users.Remove(data);

            await _context.SaveChangesAsync();

            return Ok(data);
        }
    }
}

