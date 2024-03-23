using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataNexApi.Controllers
{
    public class ProductBarcodesController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public ProductBarcodesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.ProductBarcodes.ToListAsync();

            return Ok(data);
        }

        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            var data = await _context.ProductBarcodes.Select(x => x.Barcode).ToListAsync();

            return Ok(data);
        }


        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.ProductBarcodes.Where(x => x.Id == id).FirstOrDefaultAsync();

            var dto = _mapper.Map<ProductBarcodeDto>(data);


            return Ok(dto);
        }

        [HttpGet("getbyproductid/{productid}")]
        public async Task<IActionResult> GetByProductId(Guid productid)
        {
            var data = await _context.ProductBarcodes.Include(x=>x.Size).Where(x => x.ProductId == productid).Select(x=> new ProductBarcodeDto()
            {
                Id=x.Id,
                ProductId = x.ProductId,
                SizeId = x.SizeId,
                SizeName =x.Size.Name,
                Barcode = x.Barcode,


            }).ToListAsync();

           // var dto = _mapper.Map<ProductBarcodeDto>(data);


            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] ProductBarcodeDto productBarcode)

        {
            var data = new ProductBarcode();

            data.ProductId = productBarcode.ProductId;
            data.SizeId = productBarcode.SizeId;
            data.Barcode = productBarcode.Barcode;
            //TODO Check if exists for size and productid
            var exists = await  _context.ProductBarcodes.Where(x => x.ProductId == productBarcode.ProductId && x.SizeId == productBarcode.SizeId).FirstOrDefaultAsync();
            if (exists==null)
            {
                _context.ProductBarcodes.Add(data);
                await _context.SaveChangesAsync();

                var dto = _mapper.Map<ProductBarcodeDto>(data);

                return Ok(dto);
            }
            else
            {
                var result = new
                {
                    result = "Record already exists"
                };
                return Ok(result);
            }
           
        }


        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] ProductBarcodeDto productBarcode)
        {
            var data = await _context.ProductBarcodes.FirstOrDefaultAsync(x => x.Id == productBarcode.Id);

            data.ProductId = productBarcode.ProductId;
            data.SizeId = productBarcode.SizeId;
            data.Barcode = productBarcode.Barcode;
            
            await _context.SaveChangesAsync();
            var dto = _mapper.Map<ProductBarcodeDto>(data);

            return Ok(dto);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var data = await _context.ProductBarcodes.FirstOrDefaultAsync(x => x.Id == id);

            _context.ProductBarcodes.Remove(data);

            await _context.SaveChangesAsync();
            return Ok(data);
        }
    }
}
