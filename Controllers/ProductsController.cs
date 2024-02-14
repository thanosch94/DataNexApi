using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataNexApi.Controllers
{
    public class ProductsController : BaseController
    {


        private ApplicationDbContext _context;
        private IMapper _mapper;
        public ProductsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Products.ToListAsync();

            return Ok(data);
        }


        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.Products.Where(x => x.Id == id).FirstOrDefaultAsync();

            var dto = _mapper.Map<ProductDto>(data);


            return Ok(dto);
        }

        [HttpGet("getbysku/{sku}")]
        public async Task<IActionResult> GetBySku(string sku)
        {
            var data = await _context.Products.Where(x => x.Sku == sku).FirstOrDefaultAsync();

            var dto = _mapper.Map<ProductDto>(data);


            return Ok(dto);
        }

        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            var data = await _context.Products.Select(x => new ProductDto()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(data);
        }


        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] ProductDto product)

        {
            var data = new Product();
            data.Name = product.Name;
            data.Sku = product.Sku;
            data.Description = product.Description;
            data.Image = product.Image;
            data.Price = product.Price;
            data.BrandId = product.BrandId;


            _context.Products.Add(data);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<ProductDto>(data);

            return Ok(dto);
        }


        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] ProductDto product)
        {
            var data = await _context.Products.FirstOrDefaultAsync(x => x.Id == product.Id);

            data.Name = product.Name;
            data.Sku = product.Sku;
            data.Description = product.Description;
            data.Image = product.Image;
            data.Price = product.Price;
            data.BrandId = product.BrandId;


            await _context.SaveChangesAsync();
            var dto = _mapper.Map<ProductDto>(data);

            return Ok(dto);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var data = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

            _context.Products.Remove(data);

            await _context.SaveChangesAsync();
            return Ok(data);
        }
    }
}

