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
    public class ProductSizesController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public ProductSizesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.ProductSizes.ToListAsync();

            return Ok(data);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.ProductSizes.Where(x => x.Id == id).FirstOrDefaultAsync();

            var dto = _mapper.Map<ProductSizeDto>(data);

            return Ok(dto);
        }

        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            var data = await _context.ProductSizes.Select(x => new ProductSizeDto()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] ProductSizeDto productSize)

        {
            var data = new ProductSize();
            data.Name = productSize.Name;
            
            _context.ProductSizes.Add(data);

            await _context.SaveChangesAsync();

            var dto = _mapper.Map<ProductSizeDto>(data);

            return Ok(dto);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] ProductSizeDto productSize)
        {
            var data = await _context.ProductSizes.FirstOrDefaultAsync(x => x.Id == productSize.Id);

            data.Name = productSize.Name;
    
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<ProductSizeDto>(data);

            return Ok(dto);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var data = await _context.ProductSizes.FirstOrDefaultAsync(x => x.Id == id);

            _context.ProductSizes.Remove(data);

            await _context.SaveChangesAsync();

            return Ok(data);
        }
    }
}
