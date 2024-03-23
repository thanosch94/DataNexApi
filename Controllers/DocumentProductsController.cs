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



        [HttpGet("getbybarcode/{barcode}")]
        public async Task<IActionResult> GetByBarcode(string barcode)
        {
            var product = await _context.ProductBarcodes.Include(x => x.Product).Include(x=>x.Size).Where(x => x.Barcode == barcode).Select(x => new DocumentProductDto()
            {
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                Sku = x.Product.Sku,
                ProductSizeId  =x.Size.Id,
                SizeName = x.Size.Name,
                Price = (decimal)x.Product.Price,
                ProductQuantity = 1,
                
            }).FirstOrDefaultAsync();

            return Ok(product);
        }


        [HttpGet("getbydocumentid/{id}")]
        public async Task<IActionResult> GetByDocumentId(Guid id)
        {
            var data = await _context.DocumentProducts.Include(x=>x.Product).ThenInclude(x=>x.ProductBarcodes).Include(x => x.ProductSize).Where(x => x.DocumentId == id).Select(x => new DocumentProductDto()
            {
                Id = x.Id,
                DocumentId = x.DocumentId,
                ProductId = x.ProductId,
                ProductQuantity =x.ProductQuantity,
                ProductSizeId = x.ProductSizeId,
                SizeName = x.ProductSize.Name,
                ProductName = x.Product.Name,
                Sku =x.Product.Sku,
                //TODO Add Barcode To DocumentProducts Model
                Barcode = x.Product.ProductBarcodes.Where(y=>y.SizeId== x.ProductSizeId && y.ProductId==x.ProductId).FirstOrDefault().Barcode,
                Price = (decimal)x.Product.Price,
                RowTotal = x.ProductQuantity * (decimal)x.Product.Price

            }).ToListAsync();

            var dto = _mapper.Map<List<DocumentProductDto>>(data);


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
