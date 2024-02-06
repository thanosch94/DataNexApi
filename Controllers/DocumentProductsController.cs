using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataNexApi.Controllers
{
    public class DocumentProductsController:BaseController
    {

        private ApplicationDbContext _context;
        private IMapper _mapper;
        public DocumentProductsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.DocumentProducts.ToListAsync();

            return Ok(data);
        }


        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.DocumentProducts.Where(x => x.Id == id).FirstOrDefaultAsync();

            var dto = _mapper.Map<DocumentProductDto>(data);


            return Ok(dto);
        }

        [HttpGet("getbydocumentid/{id}")]
        public async Task<IActionResult> GetByDocumentId(Guid id)
        {
            var data = await _context.DocumentProducts.Where(x => x.DocumentId == id).FirstOrDefaultAsync();

            var dto = _mapper.Map<DocumentProductDto>(data);


            return Ok(dto);
        }
        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] DocumentProductDto documentProduct)

        {
            var data = new DocumentProduct();
            data.DocumentId = documentProduct.DocumentId;
            data.ProductId = documentProduct.ProductId;
            data.ProductQuantity = documentProduct.ProductQuantity;
            data.ProductSizeId = documentProduct.ProductSizeId;

            _context.DocumentProducts.Add(data);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<DocumentProductDto>(data);

            return Ok(dto);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] DocumentProductDto documentProduct)
        {
            var data = await _context.DocumentProducts.FirstOrDefaultAsync(x => x.Id == documentProduct.Id);

            data.DocumentId = documentProduct.DocumentId;
            data.ProductId = documentProduct.ProductId;
            data.ProductQuantity = documentProduct.ProductQuantity;
            data.ProductSizeId = documentProduct.ProductSizeId;


            await _context.SaveChangesAsync();
            var dto = _mapper.Map<DocumentProductDto>(data);

            return Ok(dto);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var data = await _context.DocumentProducts.FirstOrDefaultAsync(x => x.Id == id);

            _context.DocumentProducts.Remove(data);

            await _context.SaveChangesAsync();
            return Ok(data);
        }

    }
}
