using AutoMapper;
using DataNex.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataNexApi.Controllers
{
    public class LogsController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public LogsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Logs.ToListAsync();

            return Ok(data);
        }
    }
}
