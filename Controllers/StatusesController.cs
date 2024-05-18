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
    public class StatusesController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public StatusesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Statuses.ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] StatusDto status)
        {
            var data = new Status();

            var exists = await _context.Statuses.Where(x => x.Name == status.Name).FirstOrDefaultAsync();
            if (exists == null)
            {
                data.Name = status.Name;
                _context.Statuses.Add(data);
                await _context.SaveChangesAsync();
                return Ok(data);
            }
            else
            {
                return BadRequest("Status already exists");
            }

        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] StatusDto status)
        {
            var data = await _context.Statuses.Where(x => x.Id == status.Id).FirstOrDefaultAsync();

            data.Name = status.Name;

            await _context.SaveChangesAsync();

            return Ok(data);

        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var data = await _context.Statuses.FirstOrDefaultAsync(x => x.Id == id);

            _context.Statuses.Remove(data);

            await _context.SaveChangesAsync();

            return Ok(data);
        }
    }
}
