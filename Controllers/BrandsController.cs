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
    public class BrandsController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public BrandsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Brands.ToListAsync();

            return Ok(data);
        }

    
        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.Brands.Where(x => x.Id == id).FirstOrDefaultAsync();

            var dto = _mapper.Map<BrandDto>(data);

            return Ok(dto);
        }

        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            var data = await _context.Brands.Select(x => new BrandDto()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] BrandDto brand)

        {
            var data = new Brand();
            data.Name = brand.Name;
          
            _context.Brands.Add(data);
            await _context.SaveChangesAsync();
            var dto = _mapper.Map<BrandDto>(data);

            return Ok(dto);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] BrandDto brand)
        {
            var data = await _context.Brands.FirstOrDefaultAsync(x => x.Id == brand.Id);

            data.Name = brand.Name;
            
            await _context.SaveChangesAsync();

            return Ok(data);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var data = await _context.Brands.FirstOrDefaultAsync(x => x.Id == id);

            _context.Brands.Remove(data);

            await _context.SaveChangesAsync();

            return Ok(data);
        }
    }
}
